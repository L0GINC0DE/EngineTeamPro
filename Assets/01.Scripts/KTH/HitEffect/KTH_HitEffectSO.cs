using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "HitEffect", menuName = "KTH/HitEffect")]
public class KTH_HitEffectSO : ScriptableObject
{
    [Header("히트스탑")]
    public float hitStopTime = 0.08f;

    [Header("카메라 쉐이크 (넉백 방향으로 흔들림)")]
    public float shakeForce = 1f;

    [Header("넉백")]
    public float knockbackDistance = 1.5f;
    public float pushTime = 0.12f;
    public Ease pushEase = Ease.OutExpo;

    [Header("복귀")]
    public bool returnToOrigin = true;
    public float holdTime = 0.3f;
    public float returnTime = 0.15f;
    public Ease returnEase = Ease.InOutQuad;
}
