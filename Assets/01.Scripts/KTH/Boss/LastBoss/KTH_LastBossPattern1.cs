using System;
using UnityEngine;

[Serializable]
public class KTH_UiThrowStyle
{
    public float windup = 0.4f;
    public float flyDuration = 0.6f;

    [Header("패링 시 날아가기")]
    public float flyAwayAngle = 45f;
    public float flyAwayDistance = 12f;
    public float flyAwayDuration = 0.6f;
    public float spinDuration = 0.15f;
}

public class KTH_LastBossPattern1 : KTH_BossAction
{
    [SerializeField]private KTH_RandomSpriteListSo spriteListSo;
    [SerializeField]private KTH_AttackBundle bundle = new();
    [SerializeField]private KTH_UiThrowStyle style = new();

    public override void Execute(KTH_Boss kthBoss)
    {
        KTH_Throw.Windup(kthBoss, bundle, style.windup, () => Spawn(kthBoss));
    }

    private void Spawn(KTH_Boss kthBoss)
    {
        GameObject ui = new GameObject("UI");
        ui.transform.SetParent(transform, false);
        ui.AddComponent<SpriteRenderer>();
        ui.AddComponent<BoxCollider2D>();

        KTH_Ui kthUi = ui.AddComponent<KTH_Ui>();
        kthUi.Init(spriteListSo, kthBoss, bundle, style);
    }
}
