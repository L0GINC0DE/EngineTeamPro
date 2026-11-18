using System.Collections;
using UnityEngine;

public static class KTH_HitStop
{
    private static float _endTime;  
    private static bool _isRunning;

    private static float _prevScale = 1;

    public static bool IsRunning => _isRunning;

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
