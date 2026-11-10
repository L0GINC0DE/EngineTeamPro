using System;
using Agents;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

[Serializable]
public class KTH_ShoutStyle
{
    public float travelTime = 0.4f; 
    public float ringInterval = 0.15f;
    public float extraRadius = 4f;   
    public float width = 0.12f;
    public Color color = Color.white;
    public float shakeForce;
}

public class KTH_Speaker : MonoBehaviour
{
    [SerializeField] private float rotateTime = 0.25f;
    [SerializeField] private float shoutDelay = 0.45f;
    [SerializeField] private Vector3 mouthOffset;
    [SerializeField] private CinemachineImpulseSource impulse;

    private Vector3 original;
    private Vector3 originalScale;
    private Sequence seq;

    private void Awake()
    {
        original = transform.localEulerAngles;
        originalScale = transform.localScale;
    }
    
    public void Use(KTH_Boss boss, KTH_AttackBundle bundle, int count, KTH_ShoutStyle style)
    {
        float interval = shoutDelay + (bundle.inputCount - 1) * style.ringInterval;

        seq?.Kill();
        transform.localScale = originalScale;
        seq = DOTween.Sequence();
        seq.Append(transform.DOLocalRotate(new Vector3(original.x, original.y, 0), rotateTime));
        for (int i = 0; i < count; i++)
        {
            seq.AppendInterval(i == 0 ? shoutDelay : interval);
            seq.AppendCallback(() => Shout(boss, bundle, count, style));
        }
        seq.AppendInterval(interval);
        seq.Append(transform.DOLocalRotate(original, rotateTime));
    }

    private void Shout(KTH_Boss boss, KTH_AttackBundle bundle, int count, KTH_ShoutStyle style)
    {
        Transform target = boss.target;
        Vector3 mouth = transform.TransformPoint(mouthOffset);

        KTH_DefenseBundle defense = null;
        if (target.TryGetComponent(out KTH_ParryModule parry))
            defense = parry.BeginBundle(bundle, count, mouth);

        Action hit = () =>
        {
            if (defense != null) defense.Hit();
            else if (target.TryGetComponent(out HealthModule hm))
                hm.TakeDamage((int)(bundle.baseDamage * bundle.damageCoefficient));
        };

        transform.localScale = originalScale;
        transform.DOPunchScale(originalScale * 0.2f, 0.25f, 6);
        if (style.shakeForce > 0)
            KTH_CameraShake.Shake(impulse, style.shakeForce);

        for (int i = 0; i < bundle.inputCount; i++)
        {
            DOVirtual.DelayedCall(i * style.ringInterval, () =>
            {
                KTH_SonicRing ring = new GameObject("SonicRing").AddComponent<KTH_SonicRing>();
                ring.transform.position = mouth;
                ring.Launch(target.position, style, hit);
            });
        }
    }
}
