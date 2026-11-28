using UnityEngine;

public struct KTH_DefenseResult
{
    public int perfect;
    public int good;
    public int fail;
    public float achievement;
    public float timingMultiplier;
    public int damage;
    public bool parried;
    public bool succeeded;
    public int counterDamage;
}

public static class KTH_DefenseCalculator
{
    public static KTH_DefenseResult Calculate(KTH_AttackBundle bundle, int perfect, int good, int fail,
        int bundleCount, KTH_DefenseStats stats)
    {
        var result = new KTH_DefenseResult { perfect = perfect, good = good, fail = fail };

        int inputs = Mathf.Max(1, perfect + good + fail);
        DLJ_ParryProfile profile = bundle.Profile();
        result.achievement = profile.CanParry ? (perfect + good * Mathf.Clamp01(bundle.goodCoefficient)) / inputs : 0;
        float baseDamage = bundle.baseDamage * bundle.damageCoefficient;
        float maxReduction = stats != null ? stats.maxReduction : DLJ_ParryProfile.DefaultReduction;
        (result.timingMultiplier, result.counterDamage, result.parried, result.succeeded) =
            profile.Resolve(baseDamage, result.achievement, perfect == inputs, maxReduction);
        if (result.parried) return result;

        float takenCoef = stats != null ? stats.TakenCoefficient : 1;
        float defense = stats != null ? (stats.bodyDefense + stats.weaponDefense) * stats.defenseConversion : 0;
        int minDamage = stats != null ? stats.minDamage : 0;

        float damage = baseDamage * takenCoef * result.timingMultiplier
                       - defense / Mathf.Max(1, bundleCount);

        result.damage = Mathf.Max(minDamage, Mathf.RoundToInt(damage));
        return result;
    }
}
