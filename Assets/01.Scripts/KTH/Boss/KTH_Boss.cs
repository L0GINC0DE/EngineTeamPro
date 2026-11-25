using System.Collections.Generic;
using UnityEngine;

public class KTH_Boss : KTH_Enemy
{
    [SerializeField]private List<KTH_BossAction> patterns = new();
    [SerializeField]private Transform _target;

    public Transform target => _target;
    int index;
    private KTH_BossAction reserved;
    private bool reservedEnhanced;

    private void Awake()
    {
        if (_target == null)
        {
            KTH_ParryModule player = FindAnyObjectByType<KTH_ParryModule>();
            if (player != null) _target = player.transform;
        }
    }

    public KTH_BossAction NextAction
    {
        get
        {
            if (reserved != null) return reserved;
            return patterns.Count == 0 ? null : patterns[index % patterns.Count];
        }
    }

    public void Reserve(KTH_BossAction action, bool enhanced = false)
    {
        reserved = action;
        reservedEnhanced = enhanced;
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
            bool enhanced = reservedEnhanced;
            reserved = null;
            reservedEnhanced = false;
            if (enhanced) action.ExecuteEnhanced(this);
            else action.Execute(this);
            return;
        }

        if (patterns.Count == 0) return;

        index %= patterns.Count;
        if (patterns[index] != null)
            patterns[index].Execute(this);
        index = (index + 1) % patterns.Count;
    }
}
