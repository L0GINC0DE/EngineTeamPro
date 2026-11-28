using System;
using _01.Scripts.TAE.Agents;
using UnityEngine;

public sealed class KTH_DefenseBundle
{
    private readonly KTH_ParryModule owner;
    private readonly DLJ_ParryProfile profile;
    private readonly int contactCount;
    private readonly Action<int, int, int> resolve;

    private int contacts;
    private int perfect;
    private int good;
    private int fail;

    internal KTH_DefenseBundle(KTH_ParryModule owner, DLJ_ParryProfile profile, int contactCount,
        Action<int, int, int> resolve)
    {
        this.owner = owner;
        this.profile = profile;
        this.contactCount = contactCount;
        this.resolve = resolve;
    }

    public void Hit()
    {
        if (contacts >= contactCount) return;
        contacts++;
        owner.JudgeContact(profile, true, Count);
    }

    private void Count(DLJ_ParryGrade grade)
    {
        switch (grade)
        {
            case DLJ_ParryGrade.Perfect: perfect++; break;
            case DLJ_ParryGrade.Good: good++; break;
            default: fail++; break;
        }

        if (perfect + good + fail == contactCount)
            resolve(perfect, good, fail);
    }
}

public class KTH_ParryModule : DLJ_Parry
{
    public event Action<KTH_DefenseResult> OnBundleResolved;
    public event Action<KTH_DefenseResult> OnParried;

    public KTH_DefenseBundle BeginBundle(KTH_AttackBundle data, int bundleCount, Vector3 hitFrom,
        Action<KTH_DefenseResult> onResolved = null, HealthModule attacker = null, int? contactCount = null)
    {
        TryGetComponent(out KTH_DefenseStats stats);
        DLJ_ParryProfile profile = data.Profile(stats != null ? stats.timingWindowScale : 1f);
        Prepare(profile);

        return new KTH_DefenseBundle(this, profile, Mathf.Max(1, contactCount ?? data.inputCount),
            (perfect, good, fail) =>
            {
                var result = KTH_DefenseCalculator.Calculate(data, perfect, good, fail, bundleCount, stats);
                Resolve(result, hitFrom, attacker);
                OnBundleResolved?.Invoke(result);
                onResolved?.Invoke(result);
            });
    }

    private void Resolve(KTH_DefenseResult result, Vector3 hitFrom, HealthModule attacker)
    {
        if (this == null || !gameObject.activeInHierarchy) return;
        TryGetComponent(out HealthModule health);
        if (health != null && health.IsDead) return;

        if (health != null && result.damage > 0)
            health.TakeDamage(result.damage, hitFrom);
        if (this == null || !isActiveAndEnabled) return;

        if (result.counterDamage > 0 && attacker != null && attacker != health && attacker.isActiveAndEnabled)
            attacker.TakeDamage(result.counterDamage, transform.position);

        if (result.succeeded) PlaySuccess();
        if (result.parried) OnParried?.Invoke(result);
    }
}
