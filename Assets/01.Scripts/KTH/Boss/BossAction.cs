using UnityEngine;

public abstract class BossAction
{
    public string name;
    public abstract void Execute(Boss boss);
}
