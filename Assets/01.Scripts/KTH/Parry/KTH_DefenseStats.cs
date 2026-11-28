using UnityEngine;

public class KTH_DefenseStats : MonoBehaviour
{
    [Range(0, 1)] public float maxReduction = 0.5f;

    [Header("방어력")]
    public float bodyDefense;
    public float weaponDefense;
    public float defenseConversion = 1f;

    [Header("받는 피해")]
    public bool isDefending;
    public float defendModifier = -0.5f;
    public float takenModifier;

    [Header("타이밍")]
    [Min(0)] public float timingWindowScale = 1f;
    [Min(0)] public int minDamage;

    public float TakenCoefficient => Mathf.Max(0, 1 + takenModifier + (isDefending ? defendModifier : 0));
}
