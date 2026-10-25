using UnityEngine;

[CreateAssetMenu(fileName = "Boss1Test", menuName = "KTH/BossTest/Boss1Test")]
public class Boss1Test : KTH_BossAction
{
    public override void Execute(KTH_Boss kthBoss)
    {
        Debug.Log("boss1Test");
    }
}
