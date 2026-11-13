using System;
using UnityEngine;

// 작은 외침. 링 여러 개가 연달아 나간다
[Serializable]
public class KTH_ATPattern2 : KTH_BossAction
{
    [SerializeField] private KTH_Speaker sp;
    [SerializeField] private KTH_AttackBundle bundle = new() { parryable = false, inputCount = 3 };
    [SerializeField, Min(1)] private int count = 1;
    [SerializeField] private KTH_ShoutStyle style = new();

    public override void Execute(KTH_Boss kthBoss)
    {
        sp.Use(kthBoss, bundle, count, style);
    }
}
