using System;
using UnityEngine;

[Serializable]
public class KTH_UiThrowStyle
{
    [Tooltip("던지기 전 예고 시간")]
    public float windup = 0.4f;
    [Tooltip("플레이어까지 날아가는 시간")]
    public float flyDuration = 0.6f;

    [Header("패링 시 날아가기")]
    [Tooltip("튕겨 나가는 각도 (0 = 수평, 90 = 위)")]
    public float flyAwayAngle = 45f;
    public float flyAwayDistance = 12f;
    public float flyAwayDuration = 0.6f;
    [Tooltip("날아가는 동안 한 바퀴 도는 시간 (0이면 회전 안 함)")]
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
