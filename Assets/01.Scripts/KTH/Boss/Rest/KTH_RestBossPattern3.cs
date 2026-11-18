using System;
using Agents;
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

    public override void Execute(KTH_Boss kthBoss)
    {
        CheckHp(kthBoss);
    }

    private void CheckHp(KTH_Boss kthBoss)
    {
        if (hm.CurrentHealth <= explosionHp) KTH_Bomb.Explosion(kthBoss, bundle, style);
        else kthBoss.Reserve(RandomPattern(), true);
    }

    private KTH_BossAction RandomPattern()
    {
        if (enhancedPatterns == null || enhancedPatterns.Length == 0) return null;
        return enhancedPatterns[UnityEngine.Random.Range(0, enhancedPatterns.Length)];
    }
}
