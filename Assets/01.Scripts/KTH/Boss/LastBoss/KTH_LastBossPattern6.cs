using System;
using _01.Scripts.TAE.Agents;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class Pattern6Data
{
    [Header("차지")]
    [Tooltip("레이저 발사 전 예고(차지) 시간")]
    public float chargeTime = 1.2f;
    [Tooltip("차지 중 켤 이펙트 (비우면 없음)")]
    public GameObject chargeEffect;

    [Header("레이저")]
    [Tooltip("레이저 유지 시간. 이 시간 동안 패리 키를 꾹 누르고 있어야 한다")]
    public float beamDuration = 2.5f;
    [Tooltip("레이저 굵기")]
    public float beamWidth = 1.2f;
    [Tooltip("플레이어 뒤로 얼마나 더 뻗어 나갈지")]
    public float beamOverreach = 6f;
    [Tooltip("레이저가 굵어지는/가늘어지는 시간")]
    public float beamGrowTime = 0.15f;
    public Color beamColor = new(1f, 0.25f, 0.25f, 1f);
    [Tooltip("막는 중일 때 레이저 색")]
    public Color blockedColor = new(1f, 0.9f, 0.4f, 1f);
    [Tooltip("레이저 머티리얼 (비우면 Sprites/Default)")]
    public Material beamMaterial;
    public int sortingOrder = 20;

    [Header("홀딩 판정")]
    [Tooltip("레이저 시간 중 이 비율 이상 누르고 있으면 막아낸다")]
    [Range(0, 1)] public float successRatio = 0.8f;
    [Tooltip("레이저가 닿고 나서 이 시간 안에는 안 누르고 있어도 봐준다")]
    public float startGrace = 0.15f;

    [Header("피해")]
    [Tooltip("안 누르고 있을 때 이 간격마다 피해")]
    public float tickInterval = 0.25f;
    [Tooltip("안 누르고 있을 때 틱당 피해")]
    public int tickDamage = 5;
    [Tooltip("막는 중일 때 카메라 흔들림 세기 (틱마다)")]
    public float holdShakeForce = 0.15f;
    [Tooltip("뚫렸을 때 카메라 흔들림 세기 (틱마다)")]
    public float hitShakeForce = 0.35f;

    [Header("막아냈을 때")]
    [Tooltip("켜면 막아낸 뒤 보스에게 피해를 되돌린다")]
    public bool reflectOnSuccess = true;
    public int reflectDamage = 30;
}

/// <summary>
/// 2페이즈 대형 레이저: 차지 → 플레이어를 향해 레이저 발사 → 레이저가 나가는 동안 패리 키를 꾹 누르고 있어야 한다.
/// 누르고 있는 동안은 레이저가 플레이어 앞에서 막히고, 떼면 뚫려서 틱 피해를 입는다.
/// 끝났을 때 누른 시간 비율이 successRatio 이상이면 막아낸 것(패리 성공).
/// </summary>
public class KTH_LastBossPattern6 : KTH_BossAction
{
    [Tooltip("레이저가 나가는 위치 (비우면 보스 위치)")]
    [SerializeField] private Transform muzzle;
    [SerializeField] private HealthModule hm;
    [SerializeField] private KTH_AttackBundle bundle = new() { baseDamage = 0f };
    [SerializeField] private Pattern6Data p6d = new();

    [Header("애니메이션")]
    [SerializeField] private string chargeTrigger = "IsLaserCharge";
    [SerializeField] private string fireBool = "IsLaser";

    private LineRenderer line;
    private Sequence seq;
    private Action endBeam;

    private void Awake()
    {
        if (hm == null) hm = GetComponentInParent<HealthModule>();
        CreateLine();
    }

    private void OnDisable()
    {
        seq?.Kill();
        endBeam?.Invoke();
    }

    public override void Execute(KTH_Boss kthBoss)
    {
        seq?.Kill();

        Animator animator = kthBoss.GetComponentInChildren<Animator>();
        SetTrigger(animator, chargeTrigger);
        SetActive(p6d.chargeEffect, true);

        KTH_Throw.Windup(kthBoss, bundle, p6d.chargeTime, () =>
        {
            SetActive(p6d.chargeEffect, false);
            SetBool(animator, fireBool, true);
            Fire(kthBoss, success => SetBool(animator, fireBool, false));
        });
    }

    private void Fire(KTH_Boss kthBoss, Action<bool> onEnd)
    {
        Transform target = kthBoss.target;
        target.TryGetComponent(out KTH_ParryModule parry);
        target.TryGetComponent(out HealthModule targetHm);

        // 홀딩 중 누르는 입력이 헛친 입력으로 잠기지 않게
        parry?.BeginMash();

        float elapsed = 0f, held = 0f, tickTimer = 0f;
        bool ended = false;

        line.enabled = true;
        line.widthMultiplier = 0f;
        DOTween.To(() => line.widthMultiplier, w => line.widthMultiplier = w, p6d.beamWidth, p6d.beamGrowTime)
            .SetTarget(line);

        void End()
        {
            if (ended) return;
            ended = true;
            endBeam = null;
            parry?.EndMash();

            float ratio = p6d.beamDuration > 0f ? held / p6d.beamDuration : 1f;
            bool success = ratio >= p6d.successRatio;
            Debug.Log($"[레이저] 홀딩 {ratio:P0} → {(success ? "막아냄" : "실패")}");

            if (success)
            {
                parry?.OnParryEvent?.Invoke();
                if (p6d.reflectOnSuccess && hm != null) hm.TakeDamage(p6d.reflectDamage, target.position);
            }

            if (line == null) return; // 오브젝트 파괴 중
            DOTween.Kill(line);
            DOTween.To(() => line.widthMultiplier, w => line.widthMultiplier = w, 0f, p6d.beamGrowTime)
                .SetTarget(line)
                .OnComplete(() => line.enabled = false);

            onEnd?.Invoke(success);
        }
        endBeam = End;

        seq = DOTween.Sequence().SetLink(gameObject);
        seq.AppendInterval(p6d.beamDuration);
        seq.OnUpdate(() =>
        {
            if (ended || target == null) return;
            float dt = Time.deltaTime;
            elapsed += dt;

            bool holding = parry != null && parry.IsHolding;
            if (holding || elapsed <= p6d.startGrace) held += dt;

            Vector3 from = muzzle != null ? muzzle.position : kthBoss.transform.position;
            DrawBeam(from, target.position, holding);

            tickTimer += dt;
            if (tickTimer < p6d.tickInterval) return;
            tickTimer -= p6d.tickInterval;

            if (holding)
            {
                Shake(p6d.holdShakeForce);
            }
            else if (elapsed > p6d.startGrace)
            {
                Shake(p6d.hitShakeForce);
                if (targetHm != null) targetHm.TakeDamage(p6d.tickDamage, from);
            }
        });
        seq.OnComplete(End);
        seq.OnKill(End);
    }

    /// <summary>막는 중이면 플레이어 앞에서 끊기고, 아니면 플레이어를 뚫고 뒤로 뻗는다.</summary>
    private void DrawBeam(Vector3 from, Vector3 to, bool blocked)
    {
        Vector3 dir = to - from;
        float dist = dir.magnitude;
        if (dist < 0.001f) return;
        dir /= dist;

        Vector3 end = blocked
            ? to - dir * (p6d.beamWidth * 0.5f)
            : to + dir * p6d.beamOverreach;

        line.SetPosition(0, from);
        line.SetPosition(1, end);

        Color c = blocked ? p6d.blockedColor : p6d.beamColor;
        line.startColor = c;
        line.endColor = c;
    }

    private void CreateLine()
    {
        GameObject go = new GameObject("Laser");
        go.transform.SetParent(transform, false);
        line = go.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.useWorldSpace = true;
        line.numCapVertices = 6;
        line.sortingOrder = p6d.sortingOrder;
        line.material = p6d.beamMaterial != null ? p6d.beamMaterial : new Material(Shader.Find("Sprites/Default"));
        line.enabled = false;
    }

    private static void Shake(float force)
    {
        if (force > 0f && KTH_DamageEffectManager.instance != null)
            KTH_DamageEffectManager.instance.CameraShake(force);
    }

    private static void SetActive(GameObject go, bool active)
    {
        if (go != null) go.SetActive(active);
    }

    private static void SetTrigger(Animator animator, string name)
    {
        if (animator == null || string.IsNullOrEmpty(name)) return;
        foreach (AnimatorControllerParameter p in animator.parameters)
            if (p.name == name && p.type == AnimatorControllerParameterType.Trigger)
            {
                animator.SetTrigger(name);
                return;
            }
    }

    private static void SetBool(Animator animator, string name, bool on)
    {
        if (animator == null || string.IsNullOrEmpty(name)) return;
        foreach (AnimatorControllerParameter p in animator.parameters)
            if (p.name == name && p.type == AnimatorControllerParameterType.Bool)
            {
                animator.SetBool(name, on);
                return;
            }
    }
}
