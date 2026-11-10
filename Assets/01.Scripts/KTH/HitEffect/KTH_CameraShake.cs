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

    // 모든 흔들림은 방패 보스 패링 때처럼 위아래로만 흔든다
    // (임펄스 소스의 DefaultVelocity와 상관없이 항상 아래 방향으로 시작)
    public static void Shake(CinemachineImpulseSource source, float force)
    {
        if (source == null) return;
        // 에디터에서는 씬 로드 후(Awake 이후) 임펄스 매니저가 새로 만들어져 설정이 초기화되므로 쏠 때마다 지정
        // 히트스탑(timeScale 0) 중에도 흔들려야 하므로 브레인도 같이 지정
        IgnoreTimeScale();
        source.GenerateImpulseWithVelocity(Vector3.down * Mathf.Abs(force));
    }

    // 방향을 넘겨도 크기만 세기로 쓰고 위아래로 흔든다
    public static void Shake(CinemachineImpulseSource source, Vector3 velocity)
    {
        Shake(source, velocity.magnitude);
    }
}
