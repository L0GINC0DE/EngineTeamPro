using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class Pattern2Data
{
    public int[] groups = { 3, 4, 2, 2 };
    public float showTime = 0.3f;
    public float groupInterval = 1f;
    public float groupWindup = 0.5f;

    [Header("하늘 위치")]
    public float skyHeight = 6f;
    public float skyBackOffset = 3f;
    public float skySpacing = 1.4f;
    public float skyHeightJitter = 0.6f;
    public float appearDuration = 0.2f;

    [Header("낙하")]
    public float fallDuration = 0.35f;
    public bool damagePerUi = true;
    public bool alignToFall = true;
    public float spriteAngleOffset = -90f;

    [Header("착탄")]
    public GameObject impactEffect;
    public float impactEffectLife = 1f;
}

// UI 방출 : 하늘에 UI 묶음 띄우고 플레이어한테 내리꽂음
public class KTH_LastBossPattern2 : KTH_BossAction
{
    [SerializeField] private List<GameObject> uiPrefabs = new();
    [SerializeField, Min(0.05f)] private float uiWorldSize = 1.2f;
    [SerializeField] private KTH_AttackBundle bundle = new();
    [SerializeField] private KTH_UiThrowStyle style = new();
    [SerializeField] private Pattern2Data p7d = new();

    private Sequence seq;

    private void OnDisable() => seq?.Kill();

    public override void Execute(KTH_Boss kthBoss)
    {
        List<List<GameObject>> groups = PickGroups();
        if (groups.Count == 0) return;

        seq?.Kill();
        seq = DOTween.Sequence().SetLink(gameObject);
        seq.AppendInterval(p7d.showTime);

        for (int i = 0; i < groups.Count; i++)
        {
            List<GameObject> group = groups[i];
            if (i > 0) seq.AppendInterval(p7d.groupInterval);
            seq.AppendCallback(() =>
            {
                List<KTH_Ui> uis = SpawnInSky(kthBoss, group);
                KTH_Throw.Windup(kthBoss, bundle, p7d.groupWindup, () => Fall(kthBoss, uis, groups.Count));
            });
        }
    }

    // 전부 한 번씩 쓰기 전까지는 안 겹치게 뽑음
    private List<List<GameObject>> PickGroups()
    {
        List<GameObject> all = new();
        foreach (GameObject p in uiPrefabs)
            if (p != null && !all.Contains(p)) all.Add(p);

        List<List<GameObject>> result = new();
        if (all.Count == 0) return result;

        List<GameObject> pool = new(all);
        foreach (int n in p7d.groups)
        {
            int size = Mathf.Min(Mathf.Max(1, n), all.Count);
            List<GameObject> group = new();

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

    private List<KTH_Ui> SpawnInSky(KTH_Boss kthBoss, List<GameObject> prefabs)
    {
        List<KTH_Ui> uis = new();
        Transform target = kthBoss.target;
        float toBoss = KTH_Throw.Direction(target, kthBoss.transform);

        Vector3 center = target.position + new Vector3(toBoss * p7d.skyBackOffset, p7d.skyHeight);
        float startX = -(prefabs.Count - 1) * p7d.skySpacing * 0.5f;

        for (int i = 0; i < prefabs.Count; i++)
        {
            GameObject prefab = prefabs[i];
            GameObject go = prefab.GetComponent<RectTransform>() != null
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

    private void Fall(KTH_Boss kthBoss, List<KTH_Ui> group, int groupCount)
    {
        group.RemoveAll(u => u == null);
        if (group.Count == 0) return;

        List<Tween> tweens = new();
        bool hitDone = false;

        Action judge = KTH_Throw.Judge(kthBoss, MakeBundle(group.Count), group[0].transform.position, groupCount, r =>
        {
            foreach (KTH_Ui ui in group)
            {
                if (ui == null) continue;
                if (r.parried)
                {
                    ui.FlyAway(kthBoss, style);
                }
                else
                {
                    SpawnImpact(ui.transform.position);
                    Destroy(ui.gameObject);
                }
            }
        });

        void OnHit()
        {
            if (hitDone) return;
            hitDone = true;
            foreach (Tween t in tweens) t?.Kill();
            judge();
        }

        foreach (KTH_Ui ui in group)
        {
            ui.transform.DOKill();
            AimAt(ui.transform, kthBoss.target.position);
            tweens.Add(KTH_Throw.Throw(ui.transform, kthBoss.target, p7d.fallDuration, OnHit).SetEase(Ease.InQuad).SetLink(ui.gameObject));
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

    private KTH_AttackBundle MakeBundle(int uiCount)
    {
        KTH_AttackBundle b = JsonUtility.FromJson<KTH_AttackBundle>(JsonUtility.ToJson(bundle));
        b.inputCount = 1;
        if (p7d.damagePerUi) b.baseDamage *= uiCount;
        return b;
    }
}
