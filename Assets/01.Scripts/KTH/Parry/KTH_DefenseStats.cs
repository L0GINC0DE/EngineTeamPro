using UnityEngine;

// 플레이어 쪽 방어 수치. 몸 UI, 무기 UI, 행동 선택 시스템이 값을 채워 넣는다
public class KTH_DefenseStats : MonoBehaviour
{
    [Tooltip("판정 달성률 1일 때 줄어드는 피해 비율")]
    [Range(0, 1)] public float maxReduction = 0.5f;

    [Header("방어력")]
    public float bodyDefense;
    public float weaponDefense;
    public float defenseConversion = 1f;

    [Header("받는 피해 계수 (1에 합산)")]
    [Tooltip("이번 적 턴에 방어 행동을 선택했는지")]
    public bool isDefending;
    public float defendModifier = -0.5f;
    [Tooltip("그 밖의 받는 피해 증감 합 (예: 공격 선택 시 +0.1)")]
    public float takenModifier;

    [Header("몸 UI 유틸")]
    [Tooltip("대성공·일반 성공 허용 시간에 곱해지는 배율")]
    [Min(0)] public float timingWindowScale = 1f;

    [Tooltip("최소 피해 (미정, 0이면 방어력으로 완전히 막힐 수 있음)")]
    [Min(0)] public int minDamage;

    public float TakenCoefficient => Mathf.Max(0, 1 + takenModifier + (isDefending ? defendModifier : 0));
}
