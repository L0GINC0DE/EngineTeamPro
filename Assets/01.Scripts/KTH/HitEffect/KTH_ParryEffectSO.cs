using UnityEngine;

[CreateAssetMenu(fileName = "ParryEffect", menuName = "KTH/ParryEffect")]
public class KTH_ParryEffectSO : ScriptableObject
{
    [Header("히트스탑")]
    public float hitStopTime = 0.08f;

    [Header("카메라 쉐이크")]
    public float shakeForce = 0.5f;
}
