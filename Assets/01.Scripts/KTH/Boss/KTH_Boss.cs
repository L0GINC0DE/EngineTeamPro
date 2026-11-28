using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class KTH_Boss : KTH_Enemy
{
    [SerializeField]private List<KTH_BossAction> patterns = new();
    [SerializeField]private Transform _target;
    [SerializeField] private bool useManagedTurns;

    [Header("패턴 순서")]
    [SerializeField] private bool randomOrder = true;
    [SerializeField] private bool noRepeat = true;

    public Transform target => _target;
    public bool UsesManagedTurns => useManagedTurns;
    int index = -1;
    private int lastIndex = -1;
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
            if (patterns.Count == 0) return null;
            if (randomOrder && index < 0) index = PickRandomIndex();
            return patterns[Mathf.Max(0, index) % patterns.Count];
        }
    }

    private int PickRandomIndex()
    {
        var candidates = new List<int>();
        for (int i = 0; i < patterns.Count; i++)
        {
            if (patterns[i] == null) continue;
            if (noRepeat && i == lastIndex && patterns.Count > 1) continue;
            candidates.Add(i);
        }
        if (candidates.Count == 0) return Mathf.Max(0, lastIndex);
        return candidates[Random.Range(0, candidates.Count)];
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
            index = Mathf.Max(0, index) % patterns.Count;
            lastIndex = index;
            // 랜덤이면 다음 턴에 다시 뽑음
            index = randomOrder ? -1 : (index + 1) % patterns.Count;
        }

        if (action != null) action.ExecuteTurn(this, enhanced, onComplete);
        else onComplete?.Invoke();
    }
}
