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
}

public static class KTH_DefenseCalculator
{
    public static KTH_DefenseResult Calculate(KTH_AttackBundle bundle, int perfect, int good, int fail,
        int bundleCount, KTH_DefenseStats stats)
    {
        var result = new KTH_DefenseResult { perfect = perfect, good = good, fail = fail };

        int inputs = Mathf.Max(1, perfect + good + fail);
        result.achievement = (perfect + good * bundle.goodCoefficient) / inputs;

        if (bundle.parryable && perfect == inputs)
        {
            result.parried = true;
            result.timingMultiplier = 0;
            result.damage = 0;
            return result;
        }

        float maxReduction = stats != null ? stats.maxReduction : 0;
        float takenCoef = stats != null ? stats.TakenCoefficient : 1;
        float defense = stats != null ? (stats.bodyDefense + stats.weaponDefense) * stats.defenseConversion : 0;
        int minDamage = stats != null ? stats.minDamage : 0;

        result.timingMultiplier = 1 - maxReduction * result.achievement;

        float damage = bundle.baseDamage * bundle.damageCoefficient * takenCoef * result.timingMultiplier
                       - defense / Mathf.Max(1, bundleCount);

        result.damage = Mathf.Max(minDamage, Mathf.RoundToInt(damage));
        return result;
    }
}
