using System;
using UnityEngine;

public abstract class KTH_BossAction : MonoBehaviour
{
    public abstract void Execute(KTH_Boss kthBoss);
    public virtual void ExecuteEnhanced(KTH_Boss kthBoss) => Execute(kthBoss);

    public virtual void ExecuteTurn(KTH_Boss boss, bool enhanced, Action onComplete)
    {
        if (enhanced) ExecuteEnhanced(boss);
        else Execute(boss);
        onComplete?.Invoke();
    }
}
