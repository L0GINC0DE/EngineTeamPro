using System;
using System.Collections.Generic;
using _01.Scripts.TAE.Agents;
using UnityEngine;

public class KTH_DesperateException : Exception
{
    public readonly int currentHp;

    public KTH_DesperateException(int currentHp) : base($"피가 발악 기준 이하 (현재 {currentHp})")
    {
        this.currentHp = currentHp;
    }
}

// 평소엔 강화 패턴 하나 골라서 다음 턴에 예약
// 피가 explosionHp 이하면 예외 던지고 catch에서 태초의 폭발
public class KTH_RestBossPattern3 : KTH_BossAction
{
    [SerializeField] private KTH_BossAction[] enhancedPatterns;
    [SerializeField] private bool noRepeat = true;
    [SerializeField] private int explosionHp;
    [SerializeField] private HealthModule hm;

    [Header("폭발 연타")]
    [SerializeField] private KTH_AttackBundle bundle = new()
    {
        baseDamage = 30, inputCount = 10, perfectWindow = 0.15f, goodWindow = 0.25f
    };
    [SerializeField] private KTH_BombStyle style = new();

    private KTH_Boss boss;
    private KTH_BossAction lastPattern;
    private bool exploded;

    private void Awake()
    {
        if (hm == null) hm = GetComponent<HealthModule>();
        if (hm == null) hm = GetComponentInParent<HealthModule>();
        boss = GetComponentInParent<KTH_Boss>();
    }

    private void OnEnable()
    {
        if (hm != null) hm.OnHealthChanged += OnHealthChanged;
    }

    private void OnDisable()
    {
        if (hm != null) hm.OnHealthChanged -= OnHealthChanged;
    }

    // 피가 기준 밑으로 떨어지면 바로 다음 턴에 발악 나오게 예약
    private void OnHealthChanged(int prev, int next)
    {
        if (exploded || hm.IsDead || next > explosionHp) return;
        if (boss != null) boss.Reserve(this);
    }

    public override void Execute(KTH_Boss boss) => ExecuteTurn(boss, false, null);

    public override void ExecuteTurn(KTH_Boss boss, bool enhanced, Action onComplete)
    {
        try
        {
            ReserveEnhanced(boss);
        }
        catch (KTH_DesperateException e)
        {
            Debug.Log(e.Message);
            Desperate(boss, onComplete);
            return;
        }

        onComplete?.Invoke();
    }

    private void ReserveEnhanced(KTH_Boss kthBoss)
    {
        if (hm.CurrentHealth <= explosionHp)
            throw new KTH_DesperateException(hm.CurrentHealth);

        KTH_BossAction pick = RandomPattern();
        if (pick == null) return;

        lastPattern = pick;
        kthBoss.Reserve(pick, true);
    }

    private void Desperate(KTH_Boss kthBoss, Action onComplete)
    {
        if (exploded)
        {
            onComplete?.Invoke();
            return;
        }
        exploded = true;
        KTH_Bomb.Explosion(kthBoss, bundle, style, _ => onComplete?.Invoke());
    }

    private KTH_BossAction RandomPattern()
    {
        if (enhancedPatterns == null || enhancedPatterns.Length == 0) return null;

        List<KTH_BossAction> list = new();
        foreach (KTH_BossAction p in enhancedPatterns)
        {
            if (p == null || p == this) continue;
            if (noRepeat && p == lastPattern) continue;
            list.Add(p);
        }
        if (list.Count == 0 && lastPattern != null) list.Add(lastPattern);
        if (list.Count == 0) return null;

        return list[UnityEngine.Random.Range(0, list.Count)];
    }
}
