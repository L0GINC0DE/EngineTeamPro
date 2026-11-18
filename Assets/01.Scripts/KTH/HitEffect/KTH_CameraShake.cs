using Unity.Cinemachine;
using UnityEngine;

public static class KTH_CameraShake
{
    
    public static void IgnoreTimeScale()
    {
        CinemachineImpulseManager.Instance.IgnoreTimeScale = true;

        if (Camera.main != null && Camera.main.TryGetComponent(out CinemachineBrain brain))
        {
            brain.IgnoreTimeScale = true;
        }
    }
    public static void Shake(CinemachineImpulseSource source, float force)
    {
        if (source == null) return;
  
        IgnoreTimeScale();
        source.GenerateImpulseWithVelocity(Vector3.down * Mathf.Abs(force));
    }
    public static void Shake(CinemachineImpulseSource source, Vector3 velocity)
    {
        Shake(source, velocity.magnitude);
    }
}
