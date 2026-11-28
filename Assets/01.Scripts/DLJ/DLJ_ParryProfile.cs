using System;
using UnityEngine;

public enum DLJ_ParryMode
{
    Legacy,
    Unparryable,
    Reduce,
    Block,
    Counter
}

public enum DLJ_ParryGrade
{
    Perfect,
    Good,
    Fail
}

[Serializable]
public struct DLJ_ParryProfile
{
    public static readonly DLJ_ParryProfile Default = new(DLJ_ParryMode.Block);
    public const float DefaultReduction = 0.5f;
    public const float GoodWeight = 0.5f;

    public DLJ_ParryMode mode;
    [Min(0)] public float perfectWindow;
    [Min(0)] public float goodWindow;
    [Min(0)] public float counterMultiplier;

    public DLJ_ParryProfile(DLJ_ParryMode mode, float perfectWindow = 0.08f,
        float goodWindow = 0.2f, float counterMultiplier = 1f)
    {
        this.mode = mode;
        this.perfectWindow = perfectWindow;
        this.goodWindow = goodWindow;
        this.counterMultiplier = counterMultiplier;
    }

    public bool CanParry => mode is DLJ_ParryMode.Reduce or DLJ_ParryMode.Block or DLJ_ParryMode.Counter;
    public float PerfectWindow => Mathf.Max(0, perfectWindow);
    public float GoodWindow => Mathf.Max(PerfectWindow, goodWindow);

    public (float damageMultiplier, int counterDamage, bool blocked, bool succeeded) Resolve(
        float damage, float achievement, bool allPerfect, float maxReduction = DefaultReduction)
    {
        if (!CanParry || achievement <= 0)
            return (1f, 0, false, false);

        if (allPerfect && mode == DLJ_ParryMode.Counter)
            return (0f, Mathf.RoundToInt(Mathf.Max(0, damage) * Mathf.Max(0, counterMultiplier)), true, true);

        if (allPerfect && mode == DLJ_ParryMode.Block)
            return (0f, 0, true, true);

        return (1f - Mathf.Clamp01(maxReduction) * Mathf.Clamp01(achievement), 0, false, true);
    }
}
