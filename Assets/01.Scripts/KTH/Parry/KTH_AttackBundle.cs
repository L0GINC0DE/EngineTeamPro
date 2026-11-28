using System;
using UnityEngine;

[Serializable]
public class KTH_AttackBundle
{
    [Tooltip("적 기본 공격 피해량")]
    public float baseDamage = 10f;
    [Tooltip("적이 가하는 피해 계수")]
    public float damageCoefficient = 1f;
    [Tooltip("Legacy는 기존 씬의 패링 설정을 사용. Block은 완전 차단, Counter는 차단과 반격")]
    public DLJ_ParryMode parryMode;
    [HideInInspector] public bool parryable = true;
    [Tooltip("Counter 성공 시 적에게 들어가는 원래 공격 피해의 배율")]
    [Min(0)] public float counterMultiplier = 1f;

    [Header("방어 타이밍")]
    [Tooltip("이 묶음에서 받아야 하는 입력 수 (접촉 횟수)")]
    [Min(1)] public int inputCount = 1;
    [Tooltip("일반 성공 한 번이 판정 달성률에 기여하는 정도")]
    [Range(0, 1)] public float goodCoefficient = 0.5f;
    [Tooltip("접촉 시점 앞뒤로 대성공이 되는 시간(초)")]
    public float perfectWindow = 0.08f;
    [Tooltip("접촉 시점 앞뒤로 일반 성공이 되는 시간(초). 이보다 벗어나면 실패")]
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
