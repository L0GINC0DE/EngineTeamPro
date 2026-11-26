using System;
using System.Collections.Generic;
using UnityEngine;

public class KTH_Boss : KTH_Enemy
{
    [SerializeField]private List<KTH_BossAction> patterns = new();
    [SerializeField]private Transform _target;
    [SerializeField] private bool useManagedTurns;

    public Transform target => _target;
    public bool UsesManagedTurns => useManagedTurns;
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
        if (!useManagedTurns && Input.GetKeyDown(KeyCode.S))
        {
            TakeTurn();
        }
    }

    public void TakeTurn() => TakeTurn(null);

    public void TakeTurn(Action onComplete)
    {
        KTH_BossAction action = NextAction;
        bool enhanced = false;
        if (reserved != null)
        {
            enhanced = reservedEnhanced;
            reserved = null;
            reservedEnhanced = false;
        }
        else if (patterns.Count > 0)
        {
            index = (index + 1) % patterns.Count;
        }

        if (action != null) action.ExecuteTurn(this, enhanced, onComplete);
        else onComplete?.Invoke();
    }
}
