using System;
using _01.Scripts.TAE.Agents;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class Pattern4Data
{
    public float windupTime = 0.4f;
    public float stanceDuration = 0f; // 0이면 맞을 때까지 계속 자세 유지
    public float reflectMultiplier = 1f;

    [Header("연타")]
    [Min(1)] public int requiredPress = 5;
    public float mashTime = 1.5f;
    public float mashShakeForce = 0.2f;

    [Header("연타 성공 시")]
    public bool counterOnSuccess = true;
}

// 반격 자세 : 자세 잡은 상태에서 맞으면 피해 없던 걸로 하고 받아침
// 시간 안에 연타하면 막고 못하면 받아친 피해를 맞음
public class KTH_LastBossPattern4 : KTH_BossAction
{
    [SerializeField] private HealthModule hm;
    [SerializeField] private Pattern4Data p4d = new();

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
        DOVirtual.DelayedCall(p4d.windupTime, BeginStance).SetLink(gameObject);
    }

    private void BeginStance()
    {
        inStance = true;
        SetStance(true);

        stanceTimer?.Kill();
        if (p4d.stanceDuration > 0f)
            stanceTimer = DOVirtual.DelayedCall(p4d.stanceDuration, EndStance).SetLink(gameObject);
    }

    private void EndStance()
    {
        inStance = false;
        stanceTimer?.Kill();
        SetStance(false);
    }

    private void OnBossHealthChanged(int prev, int next)
    {
        if (!inStance || countering || next >= prev || hm.IsDead) return;

        EndStance();
        Counter(prev - next);
    }

    private void Counter(int damage)
    {
        countering = true;
        hm.Heal(damage);

        if (animator != null && !string.IsNullOrEmpty(counterTrigger))
            animator.SetTrigger(counterTrigger);
        if (boss.TryGetComponent(out KTH_ParryTelegraph telegraph))
            telegraph.Play();

        int reflected = Mathf.RoundToInt(damage * p4d.reflectMultiplier);

        boss.target.TryGetComponent(out KTH_ParryModule parry);
        Mash(parry, success =>
        {
            countering = false;

            if (success)
            {
                if (p4d.counterOnSuccess) hm.TakeDamage(reflected, boss.target.position);
            }
            else if (boss.target.TryGetComponent(out HealthModule targetHm))
            {
                targetHm.TakeDamage(reflected, boss.transform.position);
            }
        });
    }

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
                KTH_DamageEffectManager.instance.CameraShake(p4d.mashShakeForce);
            if (++presses >= p4d.requiredPress) End(true);
        }

        parry.BeginMash();
        parry.OnPressed += OnPress;
        timeout = DOVirtual.DelayedCall(p4d.mashTime, () => End(false)).SetLink(gameObject);
    }

    private void SetStance(bool on)
    {
        if (animator == null) return;
        foreach (AnimatorControllerParameter p in animator.parameters)
            if (p.name == stanceBool && p.type == AnimatorControllerParameterType.Bool)
                animator.SetBool(stanceBool, on);
    }
}
