using System;
using System.Collections.Generic;
using _01.Scripts.TAE.Agents;
using UnityEngine;

/// <summary>보스 피가 발악 기준 이하일 때 던지는 예외. catch에서 폭발(발악) 패턴으로 이어진다.</summary>
public class KTH_DesperateException : Exception
{
    public readonly int currentHp;

    public KTH_DesperateException(int currentHp)
        : base($"피가 발악 기준 이하 (현재 {currentHp})")
    {
        this.currentHp = currentHp;
    }
}

/// <summary>
/// 휴식 보스 3번 패턴.
/// try: 평소엔 강화 패턴 하나를 랜덤으로 골라 다음 턴에 예약한다.
///      피가 explosionHp 이하면 예외를 던진다.
/// catch: 태초의 폭발(발악)을 실행한다.
/// 피가 기준 이하로 떨어지는 순간 이 패턴을 다음 턴에 예약해서, 랜덤 순서와 상관없이 바로 발악이 나온다.
/// </summary>
public class KTH_RestBossPattern3 : KTH_BossAction
{
    [Tooltip("평소에 랜덤으로 예약할 강화 패턴들")]
    [SerializeField] private KTH_BossAction[] enhancedPatterns;
    [Tooltip("같은 강화 패턴이 연속으로 나오지 않게")]
    [SerializeField] private bool noRepeat = true;
    [Tooltip("피가 이 값 이하면 발악(폭발)")]
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

    /// <summary>피가 기준 이하로 떨어지면 다음 턴에 이 패턴(발악)을 예약한다.</summary>
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
            Debug.Log($"[휴식 보스] {e.Message} → 발악: 태초의 폭발");
            Desperate(boss, onComplete);
            return;
        }

        onComplete?.Invoke();
    }

    /// <summary>강화 패턴 예약. 피가 기준 이하면 예약하지 않고 예외를 던진다.</summary>
    private void ReserveEnhanced(KTH_Boss kthBoss)
    {
        if (hm.CurrentHealth <= explosionHp)
            throw new KTH_DesperateException(hm.CurrentHealth);

        KTH_BossAction pick = RandomPattern();
        if (pick == null)
        {
            Debug.Log("[휴식 보스] 강화 패턴이 비어 있음");
            return;
        }

        lastPattern = pick;
        kthBoss.Reserve(pick, true);
        Debug.Log($"[휴식 보스] 강화 패턴 예약: {pick.GetType().Name}");
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

        var candidates = new List<KTH_BossAction>();
        foreach (KTH_BossAction p in enhancedPatterns)
        {
            if (p == null || p == this) continue;
            if (noRepeat && p == lastPattern) continue;
            candidates.Add(p);
        }
        if (candidates.Count == 0 && lastPattern != null) candidates.Add(lastPattern);
        if (candidates.Count == 0) return null;

        return candidates[UnityEngine.Random.Range(0, candidates.Count)];
    }
}
