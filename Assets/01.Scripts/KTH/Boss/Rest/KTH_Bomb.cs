using System;
using _01.Scripts.TAE.Agents;
using UnityEngine;
using DG.Tweening;
using Unity.Cinemachine;

[Serializable]
public class KTH_BombStyle
{
    public float windup = 0.5f;
    public float flyDuration = 1f;
    public float stopDistance = 1.5f;
    public float mashTime = 2f;
    public GameObject BombCharging;
    public GameObject BombFly;
    public GameObject Bomb;

    [Header("폭발 넉백")]
    public float explosionKnockback = 1.5f;
    public float explosionKnockbackTime = 0.5f;
    public bool clampToCamera = true;
    public float cameraEdgeMargin = 1f;
    public Ease explosionKnockbackEase = Ease.OutCubic;

    [Header("폭발 링")]
    public ParticleSystem ring;
    public float ringScale = 2f;
    [Range(0.05f, 2f)] public float ringSpeed = 0.35f;

    [Header("폭발 패널")]
    public CanvasGroup flashPanel;
    public float flashFadeTime = 1.5f;
    public Ease flashFadeEase = Ease.InQuad;

    [Header("연타 카메라")]
    public CinemachineCamera mashCamera;
    public float mashShakeForce = 0.3f;
    public float mashMinSize = 3f;
    public Ease mashZoomEase = Ease.InOutSine;
    public float zoomRestoreTime = 0.4f;
}

// 태초의 폭발 : 차지 -> 플레이어 앞까지 날아감 -> 연타
// 연타 성공하면 보스가 죽고 실패하면 플레이어가 죽음
public static class KTH_Bomb
{
    public static void Explosion(KTH_Boss boss, KTH_AttackBundle bundle, KTH_BombStyle style, Action<bool> onDone = null)
    {
        Vector3 position = boss.transform.position;
        Transform target = boss.target;
        int required = Mathf.Max(1, bundle.inputCount);

        if (bundle.ShowsParryTelegraph && boss.TryGetComponent(out KTH_ParryTelegraph telegraph))
            telegraph.Play();

        KTH_ParryModule parry = null;
        if (target != null) target.TryGetComponent(out parry);

        SetActive(style.BombCharging, true);

        DOVirtual.DelayedCall(style.windup, () =>
        {
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

            CinemachineCamera cam = GetCamera(style);
            float originSize = cam != null ? cam.Lens.OrthographicSize : 0f;

            move.OnComplete(() =>
            {
                if (cam != null) ZoomTo(cam, style.mashMinSize, style.mashTime, style.mashZoomEase);

                Mash(parry, required, style.mashTime, () => Shake(style.mashShakeForce), success =>
                {
                    SetActive(style.BombFly, false);

                    Transform victim = success ? boss.transform : target;
                    Vector3 from = success ? stopPos : position;
                    PlayBomb(style, fly != null ? fly.position : stopPos);

                    if (victim != null && victim.TryGetComponent(out HealthModule hm))
                        hm.TakeDamage(hm.CurrentHealth, from);
                    Push(victim, from, style);

                    Flash(style);
                    if (cam != null && originSize > 0f) ZoomTo(cam, originSize, style.zoomRestoreTime, Ease.OutCubic);

                    onDone?.Invoke(success);
                });
            });
        });
    }

    private static void Mash(KTH_ParryModule parry, int required, float time, Action onPress, Action<bool> onEnd)
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

    // 맞은 쪽을 위아래로 더 밀어냄 (보스는 위, 플레이어는 아래)
    private static void Push(Transform victim, Vector3 from, KTH_BombStyle style)
    {
        if (victim == null || style.explosionKnockback <= 0f) return;

        float dir = victim.position.y >= from.y ? 1f : -1f;
        victim.DOKill();
        float y = victim.position.y + dir * style.explosionKnockback;

        Camera mainCam = Camera.main;
        if (style.clampToCamera && mainCam != null && mainCam.orthographic)
        {
            float half = mainCam.orthographicSize - style.cameraEdgeMargin;
            float camY = mainCam.transform.position.y;
            y = Mathf.Clamp(y, camY - half, camY + half);
        }

        victim.DOMoveY(y, style.explosionKnockbackTime).SetEase(style.explosionKnockbackEase).SetUpdate(true).SetTarget(victim);
    }

    private static void Flash(KTH_BombStyle style)
    {
        CanvasGroup panel = style.flashPanel;
        if (panel == null) return;

        panel.gameObject.SetActive(true);
        panel.DOKill();
        panel.alpha = 1f;
        panel.DOFade(0f, style.flashFadeTime).SetEase(style.flashFadeEase).SetUpdate(true);
    }

    private static CinemachineCamera GetCamera(KTH_BombStyle style)
    {
        if (style.mashCamera != null) return style.mashCamera;
        if (Camera.main != null && Camera.main.TryGetComponent(out CinemachineBrain brain))
            return brain.ActiveVirtualCamera as CinemachineCamera;
        return null;
    }

    private static void Shake(float force)
    {
        if (KTH_DamageEffectManager.instance != null)
            KTH_DamageEffectManager.instance.CameraShake(force);
    }

    private static void ZoomTo(CinemachineCamera cam, float size, float time, Ease ease)
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
        SetRing(style);
        style.Bomb.SetActive(false);
        style.Bomb.SetActive(true);
    }

    private static void SetRing(KTH_BombStyle style)
    {
        if (style.ring == null)
        {
            foreach (ParticleSystem ps in style.Bomb.GetComponentsInChildren<ParticleSystem>(true))
                if (ps.name == "Shockwave_3") { style.ring = ps; break; }
        }
        if (style.ring == null) return;

        style.ring.gameObject.SetActive(true);
        if (style.ringScale > 0f) style.ring.transform.localScale = Vector3.one * style.ringScale;

        var main = style.ring.main;
        main.simulationSpeed = style.ringSpeed;
        main.useUnscaledTime = true; // 히트스탑 중에도 퍼지게
    }

    private static void SetActive(GameObject go, bool active)
    {
        if (go != null) go.SetActive(active);
    }
}
