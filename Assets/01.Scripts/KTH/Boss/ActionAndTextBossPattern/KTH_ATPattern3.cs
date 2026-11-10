using System;
using UnityEngine;

// 큰 외침. 두꺼운 링 하나가 화면 끝까지 퍼진다
[Serializable]
public class KTH_ATPattern3 : KTH_BossAction
{
    [SerializeField] private KTH_Speaker sp;
    [SerializeField] private KTH_AttackBundle bundle = new() { baseDamage = 25, parryable = false };
    [SerializeField, Min(1)] private int count = 1;
    [SerializeField] private KTH_ShoutStyle style = new()
    {
        travelTime = 0.6f,
        extraRadius = 12f,
        width = 0.5f,
        color = new Color(1f, 0.9f, 0.4f),
        shakeForce = 1f,
    };

    public override void Execute(KTH_Boss kthBoss)
    {
        sp.Use(kthBoss, bundle, count, style);
    }
}
