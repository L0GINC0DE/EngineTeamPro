using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

public class KTH_DamageEffectManager : MonoBehaviour
{
    public static  KTH_DamageEffectManager instance;

    [SerializeField] private CinemachineImpulseSource impulseSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (impulseSource == null)
        {
            impulseSource = GetComponent<CinemachineImpulseSource>();
        }
        KTH_CameraShake.IgnoreTimeScale();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            KTH_HitStop.Cancel();
            instance = null;
        }
    }

    public void PlayHit(KTH_HitEffectSO data, Transform target, Vector3 hitFrom)
    {
        if (data == null) { Debug.LogError("HitEffectSO가 비어 있음", this); return; }

        float dir = target.position.y >= hitFrom.y ? 1 : -1;

        HitStop(data.hitStopTime);
        CameraShake(data.shakeForce);
        KTH_Knockback.Knockback(target, dir, data);
    }

    public void HitStop(float delay)
    {
        StartCoroutine(KTH_HitStop.HitStop(delay));
    }

    public void CameraShake(float force)
    {
        KTH_CameraShake.Shake(impulseSource, force);
    }

    public void CameraShake(Vector3 velocity)
    {
        KTH_CameraShake.Shake(impulseSource, velocity);
    }
}
