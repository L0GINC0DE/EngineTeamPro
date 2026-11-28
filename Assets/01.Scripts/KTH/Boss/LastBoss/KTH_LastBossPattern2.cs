using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class Pattern2Data
{
    [Tooltip("묶음별 UI 갯수 (순서대로 내리꽂음). 프리팹보다 많으면 한 바퀴 다 쓴 뒤 다시 섞어서 뽑는다")]
    public int[] groups = { 3, 4, 2, 2 };

    [Tooltip("패턴 시작 후 첫 묶음까지 대기")]
    public float showTime = 0.3f;
    [Tooltip("묶음과 묶음 사이 간격 (투척 시작 기준)")]
    public float groupInterval = 1f;
    [Tooltip("하늘에 나타난 뒤 내리꽂기까지 예고 시간")]
    public float groupWindup = 0.5f;

    [Header("하늘 생성 위치 (타겟 기준)")]
    [Tooltip("타겟 위로 얼마나 높이 생성할지")]
    public float skyHeight = 6f;
    [Tooltip("보스 쪽으로 얼마나 치우쳐 생성할지 (대각선으로 떨어지는 정도)")]
    public float skyBackOffset = 3f;
    [Tooltip("같은 묶음 UI끼리 가로 간격")]
    public float skySpacing = 1.4f;
    [Tooltip("같은 묶음 UI끼리 높이를 엇갈리게 하는 정도")]
    public float skyHeightJitter = 0.6f;
    [Tooltip("나타날 때 커지는 시간")]
    public float appearDuration = 0.2f;

    [Header("낙하")]
    public float fallDuration = 0.35f;
    [Tooltip("켜면 묶음 피해 = 기본 피해 × UI 갯수")]
    public bool damagePerUi = true;
    [Tooltip("떨어지는 방향으로 스프라이트를 기울일지")]
    public bool alignToFall = true;
    [Tooltip("스프라이트 기본 방향 보정 (스프라이트 위쪽이 앞이면 -90)")]
    public float spriteAngleOffset = -90f;

    [Header("착탄")]
    [Tooltip("착탄 시 생성할 이펙트 (비우면 없음)")]
    public GameObject impactEffect;
    public float impactEffectLife = 1f;
}

public class KTH_LastBossPattern2 : KTH_BossAction
{
    [Tooltip("내리꽂을 UI 프리팹들. 한 패턴 안에서는 같은 프리팹이 두 번 나오지 않는다 (패턴이 끝나면 다시 전부 사용)")]
    [SerializeField] private List<GameObject> uiPrefabs = new();
    [Tooltip("UI 크기: 가장 긴 변이 이 월드 길이(유닛)가 되도록 맞춘다. 프리팹 픽셀 크기와 상관없이 다 비슷한 크기로 나온다")]
    [SerializeField, Min(0.05f)] private float uiWorldSize = 1.2f;
    [SerializeField] private KTH_AttackBundle bundle = new();
    [SerializeField] private KTH_UiThrowStyle style = new();
    [SerializeField] private Pattern2Data p7d = new();

    private Sequence seq;

    private void OnDisable() => seq?.Kill();

    public override void Execute(KTH_Boss kthBoss)
    {
        List<List<GameObject>> groups = PickGroups();
        if (groups.Count == 0)
        {
            Debug.Log("[UI 방출] uiPrefabs 비어 있음");
            return;
        }

        int groupCount = groups.Count;

        seq?.Kill();
        seq = DOTween.Sequence().SetLink(gameObject);
        seq.AppendInterval(p7d.showTime);

        for (int i = 0; i < groups.Count; i++)
        {
            List<GameObject> group = groups[i];

            if (i > 0) seq.AppendInterval(p7d.groupInterval);
            seq.AppendCallback(() =>
            {
                // 하늘에 먼저 띄워서 보여주고, 예고 시간이 끝나면 대각선으로 내리꽂는다
                List<KTH_Ui> uis = SpawnInSky(kthBoss, group);
                KTH_Throw.Windup(kthBoss, bundle, p7d.groupWindup, () => FallGroup(kthBoss, uis, groupCount));
            });
        }
    }

    /// <summary>
    /// groups 크기대로 묶음을 뽑는다. 전체 목록을 한 바퀴 다 쓸 때까지는 중복 없이 뽑고,
    /// 다 쓰면 목록을 다시 채운다 (같은 묶음 안에서는 겹치지 않게). 매 실행마다 새로 뽑으므로 패턴이 끝나면 다시 전부 쓸 수 있다.
    /// </summary>
    private List<List<GameObject>> PickGroups()
    {
        var all = new List<GameObject>();
        foreach (GameObject p in uiPrefabs)
            if (p != null && !all.Contains(p)) all.Add(p);

        var result = new List<List<GameObject>>();
        if (all.Count == 0) return result;

        var pool = new List<GameObject>(all);
        foreach (int n in p7d.groups)
        {
            int size = Mathf.Min(Mathf.Max(1, n), all.Count);
            var group = new List<GameObject>();

            while (group.Count < size)
            {
                if (pool.Count == 0)
                    foreach (GameObject p in all)
                        if (!group.Contains(p)) pool.Add(p);

                int idx = UnityEngine.Random.Range(0, pool.Count);
                group.Add(pool[idx]);
                pool.RemoveAt(idx);
            }
            result.Add(group);
        }
        return result;
    }

    /// <summary>타겟 위 하늘(보스 쪽으로 치우친 곳)에 묶음을 새로 생성한다.</summary>
    private List<KTH_Ui> SpawnInSky(KTH_Boss kthBoss, List<GameObject> prefabs)
    {
        var uis = new List<KTH_Ui>();
        Transform target = kthBoss.target;
        float toBoss = KTH_Throw.Direction(target, kthBoss.transform);

        Vector3 center = target.position + new Vector3(toBoss * p7d.skyBackOffset, p7d.skyHeight);
        float startX = -(prefabs.Count - 1) * p7d.skySpacing * 0.5f;

        for (int i = 0; i < prefabs.Count; i++)
        {
            GameObject prefab = prefabs[i];
            bool isCanvasUi = prefab.GetComponent<RectTransform>() != null;
            GameObject go = isCanvasUi
                ? new GameObject($"UI_{prefab.name}", typeof(RectTransform))
                : new GameObject($"UI_{prefab.name}");
            go.transform.SetParent(transform, false);

            float jitter = (i % 2 == 0 ? 1f : -1f) * p7d.skyHeightJitter * 0.5f;
            go.transform.position = center + new Vector3(startX + i * p7d.skySpacing, jitter);

            KTH_Ui ui = go.AddComponent<KTH_Ui>();
            ui.SetupPrefab(prefab, uiWorldSize);
            uis.Add(ui);

            AimAt(go.transform, target.position);

            Vector3 scale = go.transform.localScale;
            go.transform.localScale = Vector3.zero;
            go.transform.DOScale(scale, p7d.appearDuration).SetEase(Ease.OutBack).SetLink(go);
        }
        return uis;
    }

    private void FallGroup(KTH_Boss kthBoss, List<KTH_Ui> group, int groupCount)
    {
        group.RemoveAll(u => u == null);
        if (group.Count == 0) return;

        var tweens = new List<Tween>();
        bool resolved = false;

        Action judge = KTH_Throw.Judge(kthBoss, CloneBundle(group.Count), group[0].transform.position, groupCount, r =>
        {
            foreach (KTH_Ui ui in group)
            {
                if (ui == null) continue;
                if (r.parried) ui.FlyAway(kthBoss, style);
                else
                {
                    SpawnImpact(ui.transform.position);
                    Destroy(ui.gameObject);
                }
            }
        });

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
            AimAt(ui.transform, kthBoss.target.position);
            tweens.Add(KTH_Throw.Throw(ui.transform, kthBoss.target, p7d.fallDuration, OnHit)
                .SetEase(Ease.InQuad)
                .SetLink(ui.gameObject));
        }
    }

    private void AimAt(Transform tr, Vector3 to)
    {
        if (!p7d.alignToFall) return;
        Vector2 dir = to - tr.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + p7d.spriteAngleOffset;
        tr.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void SpawnImpact(Vector3 pos)
    {
        if (p7d.impactEffect == null) return;
        GameObject fx = Instantiate(p7d.impactEffect, pos, Quaternion.identity);
        if (p7d.impactEffectLife > 0f) Destroy(fx, p7d.impactEffectLife);
    }

    private KTH_AttackBundle CloneBundle(int uiCount)
    {
        var b = JsonUtility.FromJson<KTH_AttackBundle>(JsonUtility.ToJson(bundle));
        b.inputCount = 1;
        if (p7d.damagePerUi) b.baseDamage *= uiCount;
        return b;
    }
}
