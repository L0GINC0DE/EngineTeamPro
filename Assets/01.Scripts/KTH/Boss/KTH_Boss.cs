using System;
using System.Collections.Generic;
using UnityEngine;

public class KTH_Boss : KTH_Enemy
{
    [SerializeField]private List<KTH_BossAction> patterns = new();
    [SerializeField]private Transform _target;
    [SerializeField] private bool useManagedTurns;

    [Header("패턴 순서")]
    [Tooltip("켜면 patterns 중에서 랜덤으로 고른다. 끄면 순서대로")]
    [SerializeField] private bool randomOrder = true;
    [Tooltip("랜덤일 때 같은 패턴이 연속으로 나오지 않게")]
    [SerializeField] private bool noRepeat = true;

    public Transform target => _target;
    public bool UsesManagedTurns => useManagedTurns;
    int index = -1; // -1 = 아직 안 뽑음
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
        // NextAction으로 미리 본 패턴을 그대로 쓴다 (예고와 실제가 다르지 않게)
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
            // 랜덤이면 다음 턴에 새로 뽑고, 아니면 순서대로 다음 칸
            index = randomOrder ? -1 : (index + 1) % patterns.Count;
        }

        if (action != null) action.ExecuteTurn(this, enhanced, onComplete);
        else onComplete?.Invoke();
    }
}
