using System.Collections;
using UnityEngine;

public static class KTH_HitStop
{
    private static float _endTime;  
    private static bool _isRunning;

    private static float _prevScale = 1;

    public static bool IsRunning => _isRunning;

    // 프로젝트가 Domain Reload를 끈 상태라 static 값이 이전 플레이에서 그대로 남는다
    // 이전 판의 _endTime(큰 값)이 남아 있으면 다음 판 첫 히트스탑이 몇십 초씩 멈추므로 플레이 시작 때 초기화
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _endTime = 0;
        _isRunning = false;
        _prevScale = 1;
    }

    public static void Cancel()
    {
        _endTime = 0;
        if (!_isRunning) return;
        Time.timeScale = _prevScale;
        _isRunning = false;
    }
    
    public static IEnumerator HitStop(float delay)
    {
        _endTime = Mathf.Max(_endTime, Time.unscaledTime + delay);
        if (_isRunning) yield break;

        _isRunning = true;
        _prevScale = Time.timeScale;
        Time.timeScale = 0;
        
        while (Time.unscaledTime < _endTime)
        {
            yield return null;
        }

        Time.timeScale = _prevScale;
        _isRunning = false;
    }
}
