using System;
using _01.Scripts.TAE.Agents;
using UnityEngine;
using DG.Tweening;
using Unity.Cinemachine;


[Serializable]
public class KTH_BombStyle
{
    [Tooltip("차지 이펙트 유지 시간")]
    public float windup = 0.5f;
    [Tooltip("플레이어 앞까지 날아가는 시간")]
    public float flyDuration = 1f;
    [Tooltip("플레이어와 이 거리만큼 떨어진 곳에서 멈춘다")]
    public float stopDistance = 1.5f;
    [Tooltip("멈춘 상태에서 연타를 받는 제한 시간")]
    public float mashTime = 2f;
    public GameObject BombCharging;
    public GameObject BombFly;
    public GameObject Bomb;

    [Header("폭발 넉백")]
    [Tooltip("폭발에 맞은 쪽이 추가로 밀려나는 거리")]
    public float explosionKnockback = 1.5f;
    public float explosionKnockbackTime = 0.5f;
    [Tooltip("켜면 화면 밖으로 밀려나지 않는다")]
    public bool clampToCamera = true;
    [Tooltip("화면 가장자리에서 이만큼 안쪽까지만 밀린다")]
    public float cameraEdgeMargin = 1f;
    public Ease explosionKnockbackEase = Ease.OutCubic;

    [Header("폭발 링")]
    [Tooltip("크게 퍼질 링 (비워두면 Bomb 자식 중 'Shockwave_3'을 찾는다). 이 링에만 아래 값이 적용된다")]
    public ParticleSystem ring;
    [Tooltip("링 크기 배율")]
    public float ringScale = 2f;
    [Tooltip("링 재생 속도. 1보다 작을수록 천천히 퍼진다")]
    [Range(0.05f, 2f)] public float ringSpeed = 0.35f;

    [Header("폭발 패널")]
    [Tooltip("폭발 순간 알파 1로 켰다가 서서히 사라지는 패널")]
    public CanvasGroup flashPanel;
    public float flashFadeTime = 1.5f;
    public Ease flashFadeEase = Ease.InQuad;

    [Header("연타 카메라")]
    [Tooltip("비워두면 현재 활성 시네머신 카메라 사용")]
    public CinemachineCamera mashCamera;
    [Tooltip("연타 1번당 카메라 흔들림 세기")]
    public float mashShakeForce = 0.3f;
    [Tooltip("연타 시간 동안 이 Orthographic Size까지 서서히 줌인")]
    public float mashMinSize = 3f;
    public Ease mashZoomEase = Ease.InOutSine;
    [Tooltip("폭발 후 원래 크기로 돌아가는 시간")]
    public float zoomRestoreTime = 0.4f;
}

/// <summary>
/// 태초의 폭발: 차지 → 플레이어 바로 앞까지 날아가서 멈춤 → 제한 시간 안에 bundle.inputCount번 연타.
/// 성공하면 터지고 보스 사망, 실패하면 터지고 플레이어 사망.
/// </summary>
public static class KTH_Bomb
{
    public static void Explosion(KTH_Boss boss, KTH_AttackBundle bundle, KTH_BombStyle style,
        Action<bool> onDone = null)
    {
        Vector3 position = boss.transform.position;
        Transform target = boss.target;
        int required = Mathf.Max(1, bundle.inputCount);

        if (bundle.ShowsParryTelegraph && boss.TryGetComponent(out KTH_ParryTelegraph telegraph))
            telegraph.Play();

        KTH_ParryModule parry = null;
        if (target != null) target.TryGetComponent(out parry);

        // 1. 차지
        SetActive(style.BombCharging, true);

        DOVirtual.DelayedCall(style.windup, () =>
        {
            // 2. 차지 끄고 플라이 켜서 플레이어 바로 앞까지
            Vector3 startPos = style.BombCharging != null ? style.BombCharging.transform.position : position;
            SetActive(style.BombCharging, false);

            Transform fly = style.BombFly != null ? style.BombFly.transform : null;
            if (fly != null)
            {
                fly.position = startPos;
                SetActive(style.BombFly, true);
            }

            Vector3 stopPos = startPos;
            if (target != null)
            {
                Vector3 toTarget = target.position - startPos;
                stopPos = target.position - toTarget.normalized * Mathf.Min(style.stopDistance, toTarget.magnitude);
            }

            Tween move = fly != null
                ? fly.DOMove(stopPos, style.flyDuration).SetEase(Ease.OutQuad)
                : DOVirtual.DelayedCall(style.flyDuration, () => { });

            CinemachineCamera cam = ResolveCamera(style);
            float originSize = cam != null ? cam.Lens.OrthographicSize : 0f;

            // 3. 멈춘 상태에서 연타 (누를 때마다 카메라 흔들림, 줌은 연타 시간 동안 서서히)
            move.OnComplete(() =>
            {
                // 연타 시간 동안 스으윽 줌인
                if (cam != null) TweenSize(cam, style.mashMinSize, style.mashTime, style.mashZoomEase);

                Mash(parry, required, style.mashTime, () => OnMashPress(style), success =>
            {
                // 4. 플라이 끄고 터짐 → 성공이면 보스, 실패면 플레이어 사망
                SetActive(style.BombFly, false);

                Transform victim = success ? boss.transform : target;
                Vector3 from = success ? stopPos : position;
                PlayBomb(style, fly != null ? fly.position : stopPos);

                if (victim != null && victim.TryGetComponent(out HealthModule hm))
                    hm.TakeDamage(hm.CurrentHealth, from);
                PushFurther(victim, from, style);

                PlayFlash(style);
                RestoreZoom(cam, originSize, style.zoomRestoreTime);

                onDone?.Invoke(success);
            });
            });
        });
    }

    /// <summary>제한 시간 안에 required번 누르면 즉시 성공, 시간이 다 되면 실패.</summary>
    private static void Mash(KTH_ParryModule parry, int required, float time, Action onPress,
        Action<bool> onEnd)
    {
        if (parry == null)
        {
            onEnd(false);
            return;
        }

        int presses = 0;
        bool ended = false;
        Tween timeout = null;

        void End(bool success)
        {
            if (ended) return;
            ended = true;
            parry.OnPressed -= OnPress;
            parry.EndMash();
            timeout?.Kill();
            onEnd(success);
        }

        void OnPress()
        {
            onPress?.Invoke();
            if (++presses >= required) End(true);
        }

        parry.BeginMash();
        parry.OnPressed += OnPress;
        timeout = DOVirtual.DelayedCall(time, () => End(false));
    }

    /// <summary>폭발에 맞은 대상을 기본 넉백보다 더 멀리 날려 보낸다.</summary>
    private static void PushFurther(Transform victim, Vector3 from, KTH_BombStyle style)
    {
        if (victim == null || style.explosionKnockback <= 0f) return;

        float dir = victim.position.x >= from.x ? 1f : -1f;
        victim.DOKill(); // 기본 넉백(원위치 복귀 포함)을 멈추고 그 자리에서 더 밀어낸다
        float targetX = victim.position.x + dir * style.explosionKnockback;

        Camera mainCam = Camera.main;
        if (style.clampToCamera && mainCam != null && mainCam.orthographic)
        {
            float halfWidth = mainCam.orthographicSize * mainCam.aspect - style.cameraEdgeMargin;
            float camX = mainCam.transform.position.x;
            targetX = Mathf.Clamp(targetX, camX - halfWidth, camX + halfWidth);
        }

        victim.DOMoveX(targetX, style.explosionKnockbackTime)
            .SetEase(style.explosionKnockbackEase)
            .SetUpdate(true)
            .SetTarget(victim);
    }

    /// <summary>패널 알파를 1로 켰다가 서서히 0으로.</summary>
    private static void PlayFlash(KTH_BombStyle style)
    {
        CanvasGroup panel = style.flashPanel;
        if (panel == null) return;

        panel.gameObject.SetActive(true);
        panel.DOKill();
        panel.alpha = 1f;
        panel.DOFade(0f, style.flashFadeTime).SetEase(style.flashFadeEase).SetUpdate(true);
    }

    private static CinemachineCamera ResolveCamera(KTH_BombStyle style)
    {
        if (style.mashCamera != null) return style.mashCamera;
        if (Camera.main != null && Camera.main.TryGetComponent(out CinemachineBrain brain))
            return brain.ActiveVirtualCamera as CinemachineCamera;
        return null;
    }

    /// <summary>연타할 때마다 카메라를 흔든다.</summary>
    private static void OnMashPress(KTH_BombStyle style)
    {
        if (KTH_DamageEffectManager.instance != null)
            KTH_DamageEffectManager.instance.CameraShake(style.mashShakeForce);
    }

    private static void RestoreZoom(CinemachineCamera cam, float originSize, float time)
    {
        if (cam == null || originSize <= 0f) return;
        TweenSize(cam, originSize, time, Ease.OutCubic);
    }

    private static void TweenSize(CinemachineCamera cam, float size, float time, Ease ease)
    {
        DOTween.Kill(cam);
        DOTween.To(() => cam.Lens.OrthographicSize, v =>
            {
                LensSettings lens = cam.Lens;
                lens.OrthographicSize = v;
                cam.Lens = lens;
            }, size, time)
            .SetEase(ease)
            .SetUpdate(true)
            .SetTarget(cam);
    }

    private static void PlayBomb(KTH_BombStyle style, Vector3 pos)
    {
        if (style.Bomb == null) return;
        style.Bomb.transform.position = pos;
        SetupRing(style);
        style.Bomb.SetActive(false); // 파티클 재생을 위해 재활성화
        style.Bomb.SetActive(true);
    }

    /// <summary>링 하나만 켜고, 크게 + 천천히 퍼지게 설정한다.</summary>
    private static void SetupRing(KTH_BombStyle style)
    {
        if (style.ring == null)
        {
            foreach (ParticleSystem ps in style.Bomb.GetComponentsInChildren<ParticleSystem>(true))
                if (ps.name == "Shockwave_3") { style.ring = ps; break; }
        }
        if (style.ring == null) return;

        style.ring.gameObject.SetActive(true);
        if (style.ringScale > 0f) style.ring.transform.localScale = Vector3.one * style.ringScale;

        ParticleSystem.MainModule main = style.ring.main;
        main.simulationSpeed = style.ringSpeed;
        main.useUnscaledTime = true; // 히트스톱(timeScale 0) 중에도 멈추지 않게
    }

    private static void SetActive(GameObject go, bool active)
    {
        if (go != null) go.SetActive(active);
    }
}
