using System;
using _01.Scripts.TAE.Agents;
using UnityEngine;

public class KTH_RestBossPattern3 : KTH_BossAction
{
    [SerializeField] private KTH_BossAction[] enhancedPatterns;
    [SerializeField]private int explosionHp;

    [SerializeField]private HealthModule hm;

    [Header("폭발 연타")]
    [SerializeField] private KTH_AttackBundle bundle = new()
    {
        baseDamage = 30, inputCount = 10, perfectWindow = 0.15f, goodWindow = 0.25f
    };
    [SerializeField] private KTH_BombStyle style = new();

    private void Awake()
    {
        if (hm == null) hm = GetComponent<HealthModule>();
    }

    public override void Execute(KTH_Boss boss) => ExecuteTurn(boss, false, null);

    public override void ExecuteTurn(KTH_Boss boss, bool enhanced, Action onComplete)
    {
        if (hm.CurrentHealth <= explosionHp)
        {
            KTH_Bomb.Explosion(boss, bundle, style, _ => onComplete?.Invoke());
            return;
        }

        boss.Reserve(RandomPattern(), true);
        onComplete?.Invoke();
    }

    private KTH_BossAction RandomPattern()
    {
        if (enhancedPatterns == null || enhancedPatterns.Length == 0) return null;
        return enhancedPatterns[UnityEngine.Random.Range(0, enhancedPatterns.Length)];
    }
}
