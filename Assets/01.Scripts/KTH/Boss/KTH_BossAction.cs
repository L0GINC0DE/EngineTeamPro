using UnityEngine;

public abstract class KTH_BossAction : MonoBehaviour
{
    public abstract void Execute(KTH_Boss kthBoss);
    public virtual void ExecuteEnhanced(KTH_Boss kthBoss) => Execute(kthBoss);
}
