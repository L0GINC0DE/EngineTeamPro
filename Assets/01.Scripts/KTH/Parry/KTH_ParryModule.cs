using System;
using System.Collections.Generic;
using Agents;
using UnityEngine;
using UnityEngine.Events;

public class KTH_DefenseBundle
{
    public readonly KTH_AttackBundle data;
    public readonly int bundleCount;
    public readonly Vector3 hitFrom;
    public int perfect, good, fail;
    public bool resolved;

    internal readonly KTH_ParryModule owner;
    internal readonly Action<KTH_DefenseResult> onResolved;

    internal KTH_DefenseBundle(KTH_ParryModule owner, KTH_AttackBundle data, int bundleCount, Vector3 hitFrom,
        Action<KTH_DefenseResult> onResolved)
    {
        this.owner = owner;
        this.data = data;
        this.bundleCount = bundleCount;
        this.hitFrom = hitFrom;
        this.onResolved = onResolved;
    }

    public int Judged => perfect + good + fail;

    public event Action<KTH_DefenseGrade> OnJudged;
    internal void NotifyJudged(KTH_DefenseGrade grade) => OnJudged?.Invoke(grade);

    public void Hit() => owner.RegisterHit(this);
}

public class KTH_ParryModule : MonoBehaviour
{
    public UnityEvent OnParryEvent = new();

    public event Action<KTH_DefenseGrade> OnJudged;
    public event Action<KTH_DefenseResult> OnBundleResolved;
    public event Action<KTH_DefenseResult> OnParried;

    [SerializeField] private KeyCode parryKey = KeyCode.F;
    [Header("헛친 입력 잠금")]
    [Tooltip("헛친 입력(어떤 공격에도 안 쓰인 입력)이 나오면 이 시간 동안 입력을 받지 않는다")]
    [SerializeField] private float whiffLockout = 0.3f;
    [Tooltip("미리 누른 입력이 공격 접촉을 기다려 주는 최소 시간. 공격의 일반 성공 시간이 더 길면 그쪽을 따른다")]
    [SerializeField] private float whiffWindow = 0.2f;

    [Header("패링 성공 연출")]
    [SerializeField] private KTH_ParryEffectSO parryEffect;

    private struct PendingHit
    {
        public KTH_DefenseBundle bundle;
        public float contactTime;
    }

    private const float NoPress = float.NegativeInfinity;

    private float storedPress = NoPress;
    private readonly List<PendingHit> pending = new();
    private HealthModule health;
    private KTH_DefenseStats stats;
    private float lockedUntil;
    private float lastGoodWindow;

    private int mashDepth;

    public bool IsLocked => Time.time < lockedUntil;
    public bool IsMashing => mashDepth > 0;
    private bool HasStoredPress => !float.IsNegativeInfinity(storedPress);
    private float StoredPressExpire => Mathf.Max(whiffWindow, lastGoodWindow);

    private void Awake()
    {
        TryGetComponent(out health);
        TryGetComponent(out stats);
    }

    private void Update()
    {
        if (parryKey != KeyCode.None && Input.GetKeyDown(parryKey))
            Press();

        for (int i = pending.Count - 1; i >= 0; i--)
        {
            PendingHit hit = pending[i];
            if (Time.time - hit.contactTime <= GoodWindow(hit.bundle)) continue;

            pending.RemoveAt(i);
            Judge(hit.bundle, KTH_DefenseGrade.Fail);
        }

        if (HasStoredPress && Time.time - storedPress > StoredPressExpire)
        {
            if (IsMashing) storedPress = NoPress;
            else Whiff();
        }
    }

    public void Press()
    {
        if (IsLocked) return;

        if (pending.Count > 0)
        {
            PendingHit hit = pending[0];
            pending.RemoveAt(0);
            Judge(hit.bundle, Grade(hit.bundle, Time.time - hit.contactTime));
            return;
        }

        if (HasStoredPress && !IsMashing)
        {
            Whiff();
            return;
        }

        storedPress = Time.time;
    }

    private void Whiff()
    {
        storedPress = NoPress;
        lockedUntil = Time.time + whiffLockout;
    }

    public KTH_DefenseBundle BeginBundle(KTH_AttackBundle data, int bundleCount, Vector3 hitFrom,
        Action<KTH_DefenseResult> onResolved = null)
    {
        var defense = new KTH_DefenseBundle(this, data, bundleCount, hitFrom, onResolved);
        lastGoodWindow = GoodWindow(defense);
        return defense;
    }

    internal void RegisterHit(KTH_DefenseBundle bundle)
    {
        if (bundle.resolved) return;

        if (HasStoredPress)
        {
            float early = Time.time - storedPress;
            if (early <= GoodWindow(bundle))
            {
                storedPress = NoPress;
                Judge(bundle, Grade(bundle, early));
                return;
            }
        }

        pending.Add(new PendingHit { bundle = bundle, contactTime = Time.time });
    }

    public void BeginMash()
    {
        mashDepth++;
        lockedUntil = 0;
    }

    public void EndMash()
    {
        mashDepth = Mathf.Max(0, mashDepth - 1);
    }

    private KTH_DefenseGrade Grade(KTH_DefenseBundle bundle, float offset)
    {
        offset = Mathf.Abs(offset);
        if (offset <= PerfectWindow(bundle)) return KTH_DefenseGrade.Perfect;
        if (offset <= GoodWindow(bundle)) return KTH_DefenseGrade.Good;
        return KTH_DefenseGrade.Fail;
    }

    private float WindowScale => stats != null ? stats.timingWindowScale : 1f;
    private float PerfectWindow(KTH_DefenseBundle b) => b.data.perfectWindow * WindowScale;
    private float GoodWindow(KTH_DefenseBundle b) => b.data.goodWindow * WindowScale;

    private void Judge(KTH_DefenseBundle bundle, KTH_DefenseGrade grade)
    {
        switch (grade)
        {
            case KTH_DefenseGrade.Perfect: bundle.perfect++; break;
            case KTH_DefenseGrade.Good: bundle.good++; break;
            default: bundle.fail++; break;
        }
        OnJudged?.Invoke(grade);
        bundle.NotifyJudged(grade);

        if (bundle.Judged >= bundle.data.inputCount)
            Resolve(bundle);
    }

    private void Resolve(KTH_DefenseBundle bundle)
    {
        bundle.resolved = true;

        KTH_DefenseResult result = KTH_DefenseCalculator.Calculate(
            bundle.data, bundle.perfect, bundle.good, bundle.fail, bundle.bundleCount, stats);

        if (result.parried)
        {
            OnParrySuccess();
            OnParried?.Invoke(result);
            OnParryEvent?.Invoke();
        }
        else if (health != null)
        {
            health.TakeDamage(result.damage, bundle.hitFrom);
        }

        OnBundleResolved?.Invoke(result);
        bundle.onResolved?.Invoke(result);
    }

    private void OnParrySuccess()
    {
        if (KTH_DamageEffectManager.instance == null || parryEffect == null) return;
        KTH_DamageEffectManager.instance.CameraShake(parryEffect.shakeForce);
        KTH_DamageEffectManager.instance.HitStop(parryEffect.hitStopTime);
    }
}
