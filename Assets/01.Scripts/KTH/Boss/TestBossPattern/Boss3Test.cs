using UnityEngine;

[CreateAssetMenu(fileName = "Boss3Test", menuName = "KTH/BossTest/Boss3Test")]
public class Boss3Test : KTH_BossAction
{
    public override void Execute(KTH_Boss kthBoss)
    {
        Debug.Log("boss3Test");
    }
}
