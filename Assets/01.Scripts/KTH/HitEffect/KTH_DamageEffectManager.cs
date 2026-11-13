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

    // 맞는 순간 멈춤 + 흔들림 → 멈춤이 풀리면서 튕겨 나감
    // hitFrom: 공격이 날아온 위치 (돌진 시작 위치 등)
    // 공격자 현재 위치를 쓰면 돌진이 플레이어와 겹친 순간 방향이 항상 오른쪽이 되므로 따로 받는다
    public void PlayHit(KTH_HitEffectSO data, Transform target, Vector3 hitFrom)
    {
        if (data == null) { Debug.LogError("HitEffectSO가 비어 있음", this); return; }

        float dir = target.position.x >= hitFrom.x ? 1 : -1;

        HitStop(data.hitStopTime);
        CameraShake(data.shakeForce); // 위아래 흔들림 (KTH_CameraShake)
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
