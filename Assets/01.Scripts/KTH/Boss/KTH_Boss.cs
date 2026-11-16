using System.Collections.Generic;
using UnityEngine;

public class KTH_Boss : KTH_Enemy
{
    [SerializeField]private List<KTH_BossAction> patterns = new();
    [SerializeField]private Transform _target;

    public Transform target => _target;
    int index;
    private KTH_BossAction reserved;

    public KTH_BossAction NextAction
    {
        get
        {
            if (reserved != null) return reserved;
            return patterns.Count == 0 ? null : patterns[index % patterns.Count];
        }
    }

    public void Reserve(KTH_BossAction action)
    {
        reserved = action;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            TakeTurn();
        }
    }

    public void TakeTurn()
    {
        if (reserved != null)
        {
            KTH_BossAction action = reserved;
            reserved = null;
            action.Execute(this);
            return;
        }

        if (patterns.Count == 0) return;

        index %= patterns.Count;
        if (patterns[index] != null)
            patterns[index].Execute(this);
        index = (index + 1) % patterns.Count;
    }
}
