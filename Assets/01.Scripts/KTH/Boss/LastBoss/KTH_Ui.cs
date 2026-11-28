using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class KTH_Ui : MonoBehaviour
{
    private SpriteRenderer sr;

    public void Init(KTH_RandomSpriteListSo so, KTH_Boss boss, KTH_AttackBundle bundle, KTH_UiThrowStyle style)
    {
        Setup(RandomSprite(so));
        Go(boss, bundle, style);
    }

    public void Setup(Sprite sprite)
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sortingOrder = 10;
        sr.sprite = sprite;

        if (TryGetComponent(out BoxCollider2D col) && sr.sprite != null)
            col.size = sr.sprite.bounds.size;
    }

    // 프리팹을 자식으로 생성. 캔버스 UI면 월드 캔버스로 감싸고 긴 변이 worldSize가 되게 맞춤
    public void SetupPrefab(GameObject prefab, float worldSize, int sortingOrder = 10)
    {
        if (prefab.GetComponent<RectTransform>() != null)
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.overrideSorting = true;
            canvas.sortingOrder = sortingOrder;

            RectTransform root = GetComponent<RectTransform>();
            GameObject inst = Instantiate(prefab, root, false);
            RectTransform rt = (RectTransform)inst.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;

            Vector2 size = rt.rect.size;
            root.sizeDelta = size;
            root.localScale = Vector3.one * (worldSize / Mathf.Max(1f, Mathf.Max(size.x, size.y)));

            foreach (Selectable s in inst.GetComponentsInChildren<Selectable>()) s.interactable = false;

            if (!TryGetComponent(out BoxCollider2D col)) col = gameObject.AddComponent<BoxCollider2D>();
            col.size = size;
        }
        else
        {
            GameObject inst = Instantiate(prefab, transform, false);
            inst.transform.localPosition = Vector3.zero;
            foreach (SpriteRenderer r in inst.GetComponentsInChildren<SpriteRenderer>()) r.sortingOrder = sortingOrder;

            if (GetComponent<BoxCollider2D>() == null)
            {
                Bounds b = new Bounds(transform.position, Vector3.zero);
                foreach (Renderer r in inst.GetComponentsInChildren<Renderer>()) b.Encapsulate(r.bounds);
                BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
                col.size = b.size;
                col.offset = transform.InverseTransformPoint(b.center);
            }
        }
    }

    private Sprite RandomSprite(KTH_RandomSpriteListSo so)
    {
        var list = so.SpriteList;
        if (list.Count == 0) return null;
        return list[Random.Range(0, list.Count)];
    }

    private void Go(KTH_Boss boss, KTH_AttackBundle bundle, KTH_UiThrowStyle style)
    {
        var hit = KTH_Throw.Judge(boss, bundle, transform.position, 1, result =>
        {
            if (result.parried) FlyAway(boss, style);
            else Destroy(gameObject);
        });

        KTH_Throw.Throw(transform, boss.target, style.flyDuration, hit).SetLink(gameObject);
    }

    public void FlyAway(KTH_Boss boss, KTH_UiThrowStyle style)
    {
        transform.SetParent(null, true);

        float back = KTH_Throw.Direction(boss.target, boss.transform);

        if (style.spinDuration > 0f)
            transform.DORotate(new Vector3(0, 0, 360 * -back), style.spinDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear).SetLoops(-1).SetLink(gameObject);

        KTH_Throw.Return(transform, style.flyAwayAngle, back, style.flyAwayDistance, style.flyAwayDuration)
            .SetLink(gameObject)
            .OnComplete(() => Destroy(gameObject));
    }
}
