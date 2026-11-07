using System;
using System.Collections.Generic;
using Agents;
using UnityEngine;
using UnityEngine.Events;

// 적 공격 묶음 하나의 진행 상황. 공격자가 BeginBundle로 받아서 접촉할 때마다 Hit()을 부른다
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

    // 공격이 실제로 닿은 순간(애니메이션 이벤트, 트윈 접촉 프레임 등)에 호출
    public void Hit() => owner.RegisterHit(this);
}

// 방어 타이밍 판정기. HealthModule과 같은 오브젝트(플레이어)에 붙인다
// 키 입력 시각과 공격 접촉 시각의 차이로 대성공 / 일반 성공 / 실패를 가르고
// 묶음 입력이 다 판정되면 피해를 계산해서 바로 체력에 반영한다
public class KTH_ParryModule : MonoBehaviour
{
    public UnityEvent OnParryEvent = new(); // 이펙트, 사운드 등 인스펙터 연결용

    public event Action<KTH_DefenseGrade> OnJudged;            // 입력 하나 판정될 때 (대성공! 같은 UI용)
    public event Action<KTH_DefenseResult> OnBundleResolved;   // 묶음 하나 정산될 때
    public event Action<KTH_DefenseResult> OnParried;

    [SerializeField] private KeyCode parryKey = KeyCode.F; // None이면 키 입력 없이 Press()로만 입력
    [Header("헛친 입력 잠금")]
    [Tooltip("헛친 입력(어떤 공격에도 안 쓰인 입력)이 나오면 이 시간 동안 입력을 받지 않는다")]
    [SerializeField] private float whiffLockout = 0.3f;
    [Tooltip("미리 누른 입력이 공격 접촉을 기다려 주는 최소 시간. 공격의 일반 성공 시간이 더 길면 그쪽을 따른다")]
    [SerializeField] private float whiffWindow = 0.2f;

    [Header("패링 성공 연출")]
    [SerializeField]private float cameraShakeForce = 0.5f;

    [SerializeField] private float hitStopTime;

    private struct PendingHit
    {
        public KTH_DefenseBundle bundle;
        public float contactTime;
    }

    private const float NoPress = float.NegativeInfinity;

    private float storedPress = NoPress;                // 접촉 전에 미리 누른, 아직 판정에 안 쓰인 입력 시각
    private readonly List<PendingHit> pending = new();  // 접촉은 했는데 늦은 입력을 기다리는 중
    private HealthModule health;
    private KTH_DefenseStats stats;
    private float lockedUntil;
    private float lastGoodWindow;                       // 마지막으로 시작된 묶음의 일반 성공 시간

    public bool IsLocked => Time.time < lockedUntil;
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

        // 늦은 입력 허용 시간까지 안 눌렀으면 실패
        for (int i = pending.Count - 1; i >= 0; i--)
        {
            PendingHit hit = pending[i];
            if (Time.time - hit.contactTime <= GoodWindow(hit.bundle)) continue;

            pending.RemoveAt(i);
            Judge(hit.bundle, KTH_DefenseGrade.Fail);
        }

        // 미리 눌렀는데 기다려도 공격이 안 닿았으면 헛친 입력
        if (HasStoredPress && Time.time - storedPress > StoredPressExpire)
            Whiff();
    }

    // 다단 공격을 빠르게 받을 수 있도록 공격에 쓰인 입력에는 쿨타임이 없다
    // 대신 헛친 입력(공격 없이 누름, 공격 하나에 두 번 누름)이 나오면 잠시 입력을 막아 연타를 막는다
    public void Press()
    {
        if (IsLocked) return;

        // 이미 닿은 공격이 기다리고 있으면 가장 먼저 닿은 것부터 늦은 입력으로 판정
        if (pending.Count > 0)
        {
            PendingHit hit = pending[0];
            pending.RemoveAt(0);
            Judge(hit.bundle, Grade(hit.bundle, Time.time - hit.contactTime));
            return;
        }

        // 아직 안 쓰인 입력이 남아 있는데 또 눌렀으면 연타
        if (HasStoredPress)
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

    // 공격자가 묶음 공격을 시작할 때 호출. bundleCount: 이 공격 전체의 정산 묶음 수 (방어력 1/N 분할용)
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

        // 접촉 전에 미리 눌렀으면 그 입력으로 판정한다
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

        if (bundle.Judged >= bundle.data.inputCount)
            Resolve(bundle);
    }

    // 묶음 정산: 패리면 피해 0, 아니면 공식대로 계산한 피해를 바로 체력에 반영
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
        if (KTH_DamageEffectManager.instance == null) return;
        KTH_DamageEffectManager.instance.CameraShake(cameraShakeForce);
        KTH_DamageEffectManager.instance.HitStop(hitStopTime);
    }
}
