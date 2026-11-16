using System;
using Agents;
using UnityEngine;

public class KTH_RestBossPattern3 : KTH_BossAction
{
    [SerializeField] private KTH_BossAction nextPattern;
    [SerializeField]private int explosionHp;
    
    [SerializeField]private HealthModule hm;

    private void Awake()
    {
        hm.GetComponent<HealthModule>();
    }

    public override void Execute(KTH_Boss kthBoss)
    {
        CheckHp(kthBoss);
    }

    private void CheckHp(KTH_Boss kthBoss)
    {
        if (hm.CurrentHealth <= explosionHp) KTH_Bomb.Explosion();
        else kthBoss.Reserve(nextPattern);
    }
}