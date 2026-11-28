using System;
using System.Collections.Generic;
using _01.Scripts.TAE.Agents;
using UnityEngine;

public class KTH_RestInterruptedException : Exception
{
    public readonly float achievement;

    public KTH_RestInterruptedException(float achievement) : base($"휴식 방해됨 (달성률 {achievement:0.##})")
    {
        this.achievement = achievement;
    }
}

// 휴식 : 판정 결과만큼 회복
// 플레이어가 휴식을 끊으면 예외 던지고 catch에서 발악 패턴을 다음 턴에 강화로 예약
public class KTH_LastBossPattern3 : KTH_BossAction
{
    [SerializeField]private HealthModule hm;
    [SerializeField]private int healAmount = 20;
    [SerializeField]private float windupTime = 0.6f;
    [Range(0, 1)]
    [SerializeField]private float maxHealReduction = 0.7f;
    [SerializeField]private KTH_AttackBundle bundle = new() { baseDamage = 0f };

    [Header("발악")]
    [Range(0, 1)]
    [SerializeField] private float interruptThreshold = 0.5f;
    [SerializeField] private List<KTH_BossAction> desperatePatterns = new();
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
        KTH_Throw.Judge(kthBoss, bundle, kthBoss.transform.position, 1, result =>
        {
            try
            {
                Rest(result);
            }
            catch (KTH_RestInterruptedException e)
            {
                Debug.Log(e.Message);
                Desperate(kthBoss);
            }
        })();
    }

    private void Rest(KTH_DefenseResult result)
    {
        if (result.achievement >= interruptThreshold)
            throw new KTH_RestInterruptedException(result.achievement);

        float ratio = 1f - maxHealReduction * result.achievement;
        hm.Heal(Mathf.RoundToInt(healAmount * ratio));
    }

    private void Desperate(KTH_Boss kthBoss)
    {
        List<KTH_BossAction> list = new();
        foreach (KTH_BossAction p in desperatePatterns)
        {
            if (p == null || p == this) continue;
            if (noRepeatDesperate && p == lastDesperate) continue;
            list.Add(p);
        }
        if (list.Count == 0 && lastDesperate != null) list.Add(lastDesperate);
        if (list.Count == 0) return;

        KTH_BossAction pick = list[UnityEngine.Random.Range(0, list.Count)];
        lastDesperate = pick;
        kthBoss.Reserve(pick, true);
    }
}
