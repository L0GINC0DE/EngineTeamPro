using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
[DefaultExecutionOrder(-100)]
public class DLJ_Parry : MonoBehaviour
{
    public UnityEvent OnParryEvent = new();
    public event Action OnPressed;

    [SerializeField] private KeyCode parryKey = KeyCode.F;
    [SerializeField, Min(0)] private float whiffLockout = 0.3f;
    [Tooltip("예고 없이 들어오는 공격에 대비해 입력을 보관하는 최대 시간")]
    [SerializeField, Min(0)] private float whiffWindow = 0.2f;
    [SerializeField] private KTH_ParryEffectSO parryEffect;

    private const float NoPress = float.NegativeInfinity;
    private float storedPress = NoPress;
    private float lockedUntil;
    private float bufferWindow;
    private int mashDepth;

    private readonly List<(float time, DLJ_ParryProfile profile, Action<DLJ_ParryGrade> judge)> pending = new();

    protected virtual void Update()
    {
        if (parryKey != KeyCode.None && Input.GetKeyDown(parryKey))
            Press();

        ExpireContacts(Time.time);
        ExpirePress(Time.time);
    }

    protected virtual void OnDisable()
    {
        storedPress = NoPress;
        lockedUntil = 0;
        bufferWindow = 0;
        mashDepth = 0;

        while (pending.Count > 0)
        {
            var hit = pending[0];
            pending.RemoveAt(0);
            hit.judge(DLJ_ParryGrade.Fail);
        }
    }

    public void Press()
    {
        if (!isActiveAndEnabled || Time.timeScale == 0) return;
        float now = Time.time;
        ExpireContacts(now);
        ExpirePress(now);
        if (!isActiveAndEnabled || now < lockedUntil) return;

        bool mashing = mashDepth > 0;
        OnPressed?.Invoke();
        if (mashing || !isActiveAndEnabled) return;

        if (pending.Count > 0)
        {
            var hit = pending[0];
            pending.RemoveAt(0);
            hit.judge(Grade(hit.profile, now - hit.time));
        }
        else if (storedPress != NoPress)
        {
            Whiff(now);
        }
        else
        {
            storedPress = now;
        }
    }

    // 예고 시점에 등록하면 기본 입력 보관 시간보다 긴 판정 창도 사용할 수 있다.
    public void Prepare(DLJ_ParryProfile profile)
    {
        if (profile.CanParry)
            bufferWindow = Mathf.Max(bufferWindow, profile.GoodWindow);
    }

    public void JudgeContact(DLJ_ParryProfile profile, bool allowLate, Action<DLJ_ParryGrade> judge)
    {
        if (this == null || !isActiveAndEnabled || !profile.CanParry)
        {
            judge(DLJ_ParryGrade.Fail);
            return;
        }

        float now = Time.time;
        float offset = now - storedPress;
        if (storedPress != NoPress && offset <= profile.GoodWindow)
        {
            storedPress = NoPress;
            judge(Grade(profile, offset));
        }
        else if (allowLate && profile.GoodWindow > 0)
        {
            pending.Add((now, profile, judge));
        }
        else
        {
            judge(DLJ_ParryGrade.Fail);
        }
    }

    public void BeginMash()
    {
        mashDepth++;
        storedPress = NoPress;
        lockedUntil = 0;
    }

    public void EndMash() => mashDepth = Mathf.Max(0, mashDepth - 1);

    public void PlaySuccess()
    {
        if (KTH_DamageEffectManager.instance != null && parryEffect != null)
        {
            KTH_DamageEffectManager.instance.CameraShake(parryEffect.shakeForce);
            KTH_DamageEffectManager.instance.HitStop(parryEffect.hitStopTime);
        }

        OnParryEvent?.Invoke();
    }

    private void ExpireContacts(float now)
    {
        for (int i = 0; i < pending.Count;)
        {
            var hit = pending[i];
            if (now - hit.time <= hit.profile.GoodWindow)
            {
                i++;
                continue;
            }

            pending.RemoveAt(i);
            hit.judge(DLJ_ParryGrade.Fail);
        }
    }

    private void ExpirePress(float now)
    {
        if (storedPress != NoPress && now - storedPress > Mathf.Max(whiffWindow, bufferWindow))
            Whiff(now);
    }

    private void Whiff(float now)
    {
        storedPress = NoPress;
        lockedUntil = now + whiffLockout;
    }

    private static DLJ_ParryGrade Grade(DLJ_ParryProfile profile, float offset)
    {
        if (offset <= profile.PerfectWindow) return DLJ_ParryGrade.Perfect;
        return offset <= profile.GoodWindow ? DLJ_ParryGrade.Good : DLJ_ParryGrade.Fail;
    }
}
