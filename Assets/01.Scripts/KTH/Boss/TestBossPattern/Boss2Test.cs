using UnityEngine;

[CreateAssetMenu(fileName = "Boss2Test", menuName = "KTH/BossTest/Boss2Test")]
public class Boss2Test : KTH_BossAction
{
    public override void Execute(KTH_Boss kthBoss)
    {
        Debug.Log("boss2Test");
    }
}
