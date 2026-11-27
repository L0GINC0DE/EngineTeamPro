using System;
using System.Collections.Generic;
using _01.Scripts.TAE.Agents;
using UnityEngine;

/// <summary>휴식이 플레이어에게 끊겼을 때 던지는 예외. catch에서 발악 패턴으로 이어진다.</summary>
public class KTH_RestInterruptedException : Exception
{
    public readonly float achievement;

    public KTH_RestInterruptedException(float achievement)
        : base($"휴식 방해됨 (달성률 {achievement:0.##})")
    {
        this.achievement = achievement;
    }
}

/// <summary>
/// 휴식: 타이밍 판정 결과에 따라 회복한다.
/// try 안에서 휴식을 시도하고, 플레이어가 휴식을 끊으면(달성률이 기준 이상) 예외를 던져
/// catch에서 발악 패턴을 하나 골라 다음 턴에 강화 버전으로 예약한다.
/// </summary>
public class KTH_LastBossPattern3 : KTH_BossAction
{
    [SerializeField]private HealthModule hm;
    [SerializeField]private int healAmount = 20;

    [SerializeField]private float windupTime = 0.6f;

    [Range(0, 1)]
    [SerializeField]private float maxHealReduction = 0.7f;

    [SerializeField]private KTH_AttackBundle bundle = new() { baseDamage = 0f };

    [Header("발악 (휴식이 끊기면)")]
    [Tooltip("달성률이 이 값 이상이면 휴식이 끊긴 걸로 보고 발악 패턴으로 넘어간다")]
    [Range(0, 1)]
    [SerializeField] private float interruptThreshold = 0.5f;
    [Tooltip("휴식이 끊겼을 때 랜덤으로 나올 발악 패턴들 (다음 턴에 강화 버전으로 실행)")]
    [SerializeField] private List<KTH_BossAction> desperatePatterns = new();
    [Tooltip("같은 발악 패턴이 연속으로 나오지 않게")]
    [SerializeField] private bool noRepeatDesperate = true;

    private KTH_BossAction lastDesperate;

    private void Awake()
    {
        if (hm == null) hm = GetComponentInParent<HealthModule>();
    }

    public override void Execute(KTH_Boss kthBoss)
    {
        KTH_Throw.Windup(kthBoss, bundle, windupTime, () => HealTiming(kthBoss));
    }

    private void HealTiming(KTH_Boss kthBoss)
    {
        KTH_Throw.Judge(kthBoss.target, bundle, kthBoss.transform.position, 1, result =>
        {
            try
            {
                Rest(result);
            }
            catch (KTH_RestInterruptedException e)
            {
                Debug.Log($"[휴식] {e.Message} → 발악");
                Desperate(kthBoss);
            }
        })();
    }

    /// <summary>휴식 시도. 플레이어가 끊었으면 회복하지 않고 예외를 던진다.</summary>
    private void Rest(KTH_DefenseResult result)
    {
        if (result.achievement >= interruptThreshold)
            throw new KTH_RestInterruptedException(result.achievement);

        float ratio = 1f - maxHealReduction * result.achievement;
        int amount = Mathf.RoundToInt(healAmount * ratio);
        hm.Heal(amount);
        Debug.Log($"[휴식] 달성률 {result.achievement:0.##} → 회복 {amount}");
    }

    /// <summary>발악 패턴을 하나 골라 다음 턴에 강화 버전으로 예약한다.</summary>
    private void Desperate(KTH_Boss kthBoss)
    {
        var candidates = new List<KTH_BossAction>();
        foreach (KTH_BossAction p in desperatePatterns)
        {
            if (p == null || p == this) continue;
            if (noRepeatDesperate && p == lastDesperate) continue;
            candidates.Add(p);
        }
        // 후보가 직전 것 하나뿐이면 그거라도 쓴다
        if (candidates.Count == 0 && lastDesperate != null) candidates.Add(lastDesperate);
        if (candidates.Count == 0)
        {
            Debug.Log("[휴식] 발악 패턴이 비어 있음");
            return;
        }

        KTH_BossAction pick = candidates[UnityEngine.Random.Range(0, candidates.Count)];
        lastDesperate = pick;
        kthBoss.Reserve(pick, true);
        Debug.Log($"[휴식] 발악 패턴 예약: {pick.GetType().Name}");
    }
}
