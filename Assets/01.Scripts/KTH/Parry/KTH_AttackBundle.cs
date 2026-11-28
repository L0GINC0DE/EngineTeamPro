using System;
using UnityEngine;

[Serializable]
public class KTH_AttackBundle
{
    [Header("피해")]
    public float baseDamage = 10f;
    public float damageCoefficient = 1f;

    [Header("패링")]
    public DLJ_ParryMode parryMode;
    [HideInInspector] public bool parryable = true; // 예전 씬 값 (Legacy일 때만 씀)
    [Min(0)] public float counterMultiplier = 1f;

    [Header("타이밍")]
    [Min(1)] public int inputCount = 1;
    [Range(0, 1)] public float goodCoefficient = 0.5f;
    public float perfectWindow = 0.08f;
    public float goodWindow = 0.2f;

    public bool ShowsParryTelegraph => Profile().mode is DLJ_ParryMode.Block or DLJ_ParryMode.Counter;

    public DLJ_ParryProfile Profile(float windowScale = 1f)
    {
        DLJ_ParryMode mode = parryMode;
        if (mode == DLJ_ParryMode.Legacy)
            mode = parryable ? DLJ_ParryMode.Block : DLJ_ParryMode.Reduce;

        windowScale = Mathf.Max(0, windowScale);
        return new DLJ_ParryProfile(mode, perfectWindow * windowScale, goodWindow * windowScale, counterMultiplier);
    }
}
