using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class Pattern2Data
{
    [Tooltip("묶음별 UI 갯수 (순서대로 던짐). 남는 UI는 마지막 묶음에 합쳐진다")]
    public int[] groups = { 3, 2, 2 };

    [Tooltip("UI들을 보스 위에 띄운 뒤 첫 투척까지 대기")]
    public float showTime = 0.6f;
    [Tooltip("묶음과 묶음 사이 간격 (이전 묶음 판정 후가 아니라 투척 시작 기준)")]
    public float groupInterval = 1f;
    [Tooltip("각 묶음 던지기 전 예고 시간")]
    public float groupWindup = 0.3f;

    [Header("대기 배치")]
    public float hoverHeight = 2.5f;
    public float hoverSpacing = 0.8f;
    public float hoverMoveDuration = 0.3f;

    [Tooltip("켜면 묶음 피해 = 기본 피해 × UI 갯수")]
    public bool damagePerUi = true;
}

public class KTH_LastBossPattern2 : KTH_BossAction
{
    [SerializeField] private KTH_UiStorage storage;
    [Tooltip("흡수한 UI가 없을 때 대신 쓸 랜덤 스프라이트 (비우면 흡수한 게 없을 때 패턴 생략)")]
    [SerializeField] private KTH_RandomSpriteListSo fallbackSprites;
    [SerializeField] private KTH_AttackBundle bundle = new();
    [SerializeField] private KTH_UiThrowStyle style = new();
    [SerializeField] private Pattern2Data p7d = new();

    private Sequence seq;

    private void Awake()
    {
        if (storage == null) storage = GetComponentInParent<KTH_UiStorage>();
    }

    private void OnDisable() => seq?.Kill();

    public override void Execute(KTH_Boss kthBoss)
    {
        List<Sprite> sprites = CollectSprites();
        if (sprites.Count == 0)
        {
            Debug.Log("[UI 방출] 흡수한 UI 없음");
            return;
        }

        List<List<KTH_Ui>> groups = Split(Spawn(kthBoss, sprites));

        seq?.Kill();
        seq = DOTween.Sequence().SetLink(gameObject);
        seq.AppendInterval(p7d.showTime);

        for (int i = 0; i < groups.Count; i++)
        {
            List<KTH_Ui> group = groups[i];
            int groupCount = groups.Count;

            if (i > 0) seq.AppendInterval(p7d.groupInterval);
            seq.AppendCallback(() =>
                KTH_Throw.Windup(kthBoss, bundle, p7d.groupWindup, () => ThrowGroup(kthBoss, group, groupCount)));
        }
    }

    private List<Sprite> CollectSprites()
    {
        if (storage != null && storage.Count > 0) return storage.TakeAll();

        var list = new List<Sprite>();
        if (fallbackSprites == null || fallbackSprites.SpriteList.Count == 0) return list;

        int total = 0;
        foreach (int n in p7d.groups) total += Mathf.Max(0, n);
        for (int i = 0; i < total; i++)
            list.Add(fallbackSprites.SpriteList[UnityEngine.Random.Range(0, fallbackSprites.SpriteList.Count)]);
        return list;
    }

    /// <summary>보스 위에 일렬로 띄운다.</summary>
    private List<KTH_Ui> Spawn(KTH_Boss kthBoss, List<Sprite> sprites)
    {
        var uis = new List<KTH_Ui>();
        Vector3 center = kthBoss.transform.position + Vector3.up * p7d.hoverHeight;
        float startX = -(sprites.Count - 1) * p7d.hoverSpacing * 0.5f;

        for (int i = 0; i < sprites.Count; i++)
        {
            GameObject go = new GameObject($"UI_{i}");
            go.transform.SetParent(transform, false);
            go.transform.position = kthBoss.transform.position;
            go.AddComponent<SpriteRenderer>();
            go.AddComponent<BoxCollider2D>();

            KTH_Ui ui = go.AddComponent<KTH_Ui>();
            ui.Setup(sprites[i]);
            uis.Add(ui);

            Vector3 hover = center + Vector3.right * (startX + i * p7d.hoverSpacing);
            go.transform.DOMove(hover, p7d.hoverMoveDuration).SetEase(Ease.OutBack).SetLink(go);
        }
        return uis;
    }

    /// <summary>groups 순서대로 나누고, 남는 건 마지막 묶음에 붙인다.</summary>
    private List<List<KTH_Ui>> Split(List<KTH_Ui> uis)
    {
        var result = new List<List<KTH_Ui>>();
        int idx = 0;

        foreach (int size in p7d.groups)
        {
            if (idx >= uis.Count) break;
            int take = Mathf.Min(Mathf.Max(1, size), uis.Count - idx);
            result.Add(uis.GetRange(idx, take));
            idx += take;
        }

        if (idx < uis.Count)
        {
            if (result.Count == 0) result.Add(new List<KTH_Ui>());
            result[result.Count - 1].AddRange(uis.GetRange(idx, uis.Count - idx));
        }
        return result;
    }

    private void ThrowGroup(KTH_Boss kthBoss, List<KTH_Ui> group, int groupCount)
    {
        group.RemoveAll(u => u == null);
        if (group.Count == 0) return;

        KTH_AttackBundle groupBundle = CloneBundle(group.Count);
        var tweens = new List<Tween>();
        bool resolved = false;

        Action judge = KTH_Throw.Judge(kthBoss.target, groupBundle, group[0].transform.position, groupCount, r =>
        {
            foreach (KTH_Ui ui in group)
            {
                if (ui == null) continue;
                if (r.parried) ui.FlyAway(kthBoss, style);
                else Destroy(ui.gameObject);
            }
        });

        // 묶음 중 하나라도 닿으면 묶음 전체가 한 번에 판정 (패리 입력 1번)
        void OnHit()
        {
            if (resolved) return;
            resolved = true;
            foreach (Tween t in tweens) t?.Kill();
            judge();
        }

        foreach (KTH_Ui ui in group)
        {
            ui.transform.DOKill();
            tweens.Add(KTH_Throw.Throw(ui.transform, kthBoss.target, style.flyDuration, OnHit).SetLink(ui.gameObject));
        }
    }

    private KTH_AttackBundle CloneBundle(int uiCount)
    {
        var b = JsonUtility.FromJson<KTH_AttackBundle>(JsonUtility.ToJson(bundle));
        b.inputCount = 1;
        if (p7d.damagePerUi) b.baseDamage *= uiCount;
        return b;
    }
}
