using System;
using UnityEngine;

public enum KTH_DefenseGrade
{
    Perfect,
    Good,
    Fail,
}

[Serializable]
public class KTH_AttackBundle
{
    [Tooltip("적 기본 공격 피해량")]
    public float baseDamage = 10f;
    [Tooltip("적이 가하는 피해 계수")]
    public float damageCoefficient = 1f;
    [Tooltip("노란 예고가 뜨는 공격. 입력이 전부 대성공이면 피해 0")]
    public bool parryable = true;

    [Header("방어 타이밍")]
    [Tooltip("이 묶음에서 받아야 하는 입력 수 (접촉 횟수)")]
    [Min(1)] public int inputCount = 1;
    [Tooltip("일반 성공 한 번이 판정 달성률에 기여하는 정도")]
    [Range(0, 1)] public float goodCoefficient = 0.5f;
    [Tooltip("접촉 시점 앞뒤로 대성공이 되는 시간(초)")]
    public float perfectWindow = 0.08f;
    [Tooltip("접촉 시점 앞뒤로 일반 성공이 되는 시간(초). 이보다 벗어나면 실패")]
    public float goodWindow = 0.2f;
}
