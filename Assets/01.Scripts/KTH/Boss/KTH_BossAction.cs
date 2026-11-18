using UnityEngine;

public abstract class KTH_BossAction : MonoBehaviour
{
    public abstract void Execute(KTH_Boss kthBoss);

    /// <summary>강화 버전 실행. 강화 버전이 없는 패턴은 일반 실행</summary>
    public virtual void ExecuteEnhanced(KTH_Boss kthBoss) => Execute(kthBoss);
}
