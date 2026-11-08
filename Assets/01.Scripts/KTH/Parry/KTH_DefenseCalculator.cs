using UnityEngine;

public struct KTH_DefenseResult
{
    public int perfect;
    public int good;
    public int fail;
    public float achievement;      // 판정 달성률
    public float timingMultiplier; // 방어 타이밍 배율
    public int damage;             // 최종 피격 피해
    public bool parried;
}

public static class KTH_DefenseCalculator
{
    // 판정 달성률(j) = [대성공 수 + 일반 성공 수 × 일반 성공 계수] / 묶음 입력 수
    // 방어 타이밍 배율(j) = 1 − 최대 피해 감소율 × 판정 달성률(j)
    // 피격 피해(j) = 기본 피해 × 적 피해 계수 × 받는 피해 계수 × 방어 타이밍 배율 − (몸 방어력 + 무기 방어력) × 변환 계수 / 묶음 수
    // stats가 없으면 감소율 0, 방어력 0, 받는 피해 계수 1로 계산
    public static KTH_DefenseResult Calculate(KTH_AttackBundle bundle, int perfect, int good, int fail,
        int bundleCount, KTH_DefenseStats stats)
    {
        var result = new KTH_DefenseResult { perfect = perfect, good = good, fail = fail };

        int inputs = Mathf.Max(1, perfect + good + fail);
        result.achievement = (perfect + good * bundle.goodCoefficient) / inputs;

        // 패리 가능한 묶음을 전부 대성공으로 받으면 피해 0
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
