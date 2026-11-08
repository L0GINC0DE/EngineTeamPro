using System;
using UnityEngine;
using UnityEngine.Serialization;

// 방패 던지기
[Serializable]
public class KTH_ShieldBossPattern2 : KTH_BossAction
{
    [SerializeField, FormerlySerializedAs("cast")] private KTH_ShieldCast shield;
    [SerializeField] private float flyDuration = 0.6f;
    [SerializeField] private float returnDuration = 0.5f;
    [SerializeField] private KTH_AttackBundle bundle = new();
    [SerializeField, Min(1)] private int count = 1;

    public override void Execute(KTH_Boss kthBoss)
    {
        shield.Throw(kthBoss, bundle, flyDuration, returnDuration, count);
    }
}
