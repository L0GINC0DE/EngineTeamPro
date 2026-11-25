using System;
using _01.Scripts.TAE.Agents;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class Pattern6Data
{
    [Tooltip("반격 자세를 잡기 전 예고 시간")]
    public float windupTime = 0.4f;
    [Tooltip("반격 자세 유지 시간 (0 이하면 다음에 맞을 때까지 계속 유지)")]
    public float stanceDuration = 0f;
    [Tooltip("받아친 공격을 되돌려줄 때 피해 배율")]
    public float reflectMultiplier = 1f;

    [Header("연타")]
    [Tooltip("받아치기를 막기 위해 눌러야 하는 최소 횟수")]
    [Min(1)] public int requiredPress = 5;
    [Tooltip("연타 제한 시간")]
    public float mashTime = 1.5f;
    [Tooltip("연타 1번당 카메라 흔들림 세기")]
    public float mashShakeForce = 0.2f;

    [Header("연타 성공 시")]
    [Tooltip("켜면 연타 성공 시 받아친 피해가 다시 보스에게 들어간다")]
    public bool counterOnSuccess = true;
}

/// <summary>
/// 반격 자세: 보스가 자세를 잡은 뒤 플레이어에게 맞으면 그 피해를 무효화하고 역으로 받아친다.
/// 플레이어가 제한 시간 안에 requiredPress번 연타하면 막아내고(옵션으로 보스에게 되돌림), 실패하면 받아친 피해를 입는다.
/// </summary>
public class KTH_LastBossPattern6 : KTH_BossAction
{
    [SerializeField] private HealthModule hm;
    [SerializeField] private Pattern6Data p6d = new();

    [Header("애니메이션")]
    [SerializeField] private string stanceBool = "IsCounter";
    [SerializeField] private string counterTrigger = "IsCounterAttack";

    private KTH_Boss boss;
    private Animator animator;
    private bool inStance;
    private bool countering;
    private Tween stanceTimer;

    private void Awake()
    {
        if (hm == null) hm = GetComponentInParent<HealthModule>();
    }

    private void OnEnable() => hm.OnHealthChanged += OnBossHealthChanged;

    private void OnDisable()
    {
        hm.OnHealthChanged -= OnBossHealthChanged;
        EndStance();
    }

    public override void Execute(KTH_Boss kthBoss)
    {
        boss = kthBoss;
        animator = kthBoss.GetComponentInChildren<Animator>();

        DOVirtual.DelayedCall(p6d.windupTime, BeginStance).SetLink(gameObject);
    }

    private void BeginStance()
    {
        inStance = true;
        SetStanceAnim(true);
        Debug.Log("[반격] 반격 자세");

        stanceTimer?.Kill();
        if (p6d.stanceDuration > 0f)
            stanceTimer = DOVirtual.DelayedCall(p6d.stanceDuration, EndStance).SetLink(gameObject);
    }

    private void EndStance()
    {
        inStance = false;
        stanceTimer?.Kill();
        SetStanceAnim(false);
    }

    private void OnBossHealthChanged(int prev, int next)
    {
        // 반격 중 회복/피해로 다시 들어오는 경우는 무시
        if (!inStance || countering || next >= prev || hm.IsDead) return;

        int damage = prev - next;
        EndStance();
        Counter(damage);
    }

    private void Counter(int damage)
    {
        countering = true;
        hm.Heal(damage); // 받은 피해 무효화

        if (animator != null && !string.IsNullOrEmpty(counterTrigger))
            animator.SetTrigger(counterTrigger);
        if (boss.TryGetComponent(out KTH_ParryTelegraph telegraph))
            telegraph.Play();

        int reflected = Mathf.RoundToInt(damage * p6d.reflectMultiplier);
        Debug.Log($"[반격] {damage} 피해를 받아침 → {p6d.requiredPress}번 연타");

        boss.target.TryGetComponent(out KTH_ParryModule parry);
        Mash(parry, success =>
        {
            countering = false;
            Vector3 from = boss.transform.position;

            if (success)
            {
                Debug.Log("[반격] 연타 성공");
                if (p6d.counterOnSuccess) hm.TakeDamage(reflected, boss.target.position);
            }
            else
            {
                Debug.Log("[반격] 연타 실패");
                if (boss.target.TryGetComponent(out HealthModule targetHm))
                    targetHm.TakeDamage(reflected, from);
            }
        });
    }

    /// <summary>제한 시간 안에 requiredPress번 누르면 즉시 성공, 시간이 다 되면 실패.</summary>
    private void Mash(KTH_ParryModule parry, Action<bool> onEnd)
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
            if (KTH_DamageEffectManager.instance != null)
                KTH_DamageEffectManager.instance.CameraShake(p6d.mashShakeForce);
            if (++presses >= p6d.requiredPress) End(true);
        }

        parry.BeginMash();
        parry.OnPressed += OnPress;
        timeout = DOVirtual.DelayedCall(p6d.mashTime, () => End(false)).SetLink(gameObject);
    }

    private void SetStanceAnim(bool on)
    {
        if (animator == null || string.IsNullOrEmpty(stanceBool)) return;
        foreach (AnimatorControllerParameter p in animator.parameters)
            if (p.name == stanceBool && p.type == AnimatorControllerParameterType.Bool)
            {
                animator.SetBool(stanceBool, on);
                return;
            }
    }
}
