using DG.Tweening;
using UnityEngine;

public class KTH_Ui : MonoBehaviour
{
    private SpriteRenderer sr;

    public void Init(KTH_RandomSpriteListSo so, KTH_Boss boss, KTH_AttackBundle bundle, KTH_UiThrowStyle style)
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sortingOrder = 10;
        RandomSprite(so);

        if (TryGetComponent(out BoxCollider2D col) && sr.sprite != null)
            col.size = sr.sprite.bounds.size;

        Go(boss, bundle, style);
    }

    /// <summary>던지기 없이 스프라이트만 세팅 (묶음 투척 패턴에서 사용)</summary>
    public void Setup(Sprite sprite)
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sortingOrder = 10;
        sr.sprite = sprite;

        if (TryGetComponent(out BoxCollider2D col) && sr.sprite != null)
            col.size = sr.sprite.bounds.size;
    }

    /// <summary>
    /// 프리팹을 새로 생성해서 자식으로 붙인다.
    /// 캔버스 UI 프리팹(RectTransform)이면 월드 스페이스 캔버스로 감싸고, 가장 긴 변이 worldSize(유닛)가 되도록 크기를 맞춘다.
    /// </summary>
    public void SetupPrefab(GameObject prefab, float worldSize, int sortingOrder = 10)
    {
        bool isCanvasUi = prefab.GetComponent<RectTransform>() != null;

        if (isCanvasUi)
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.overrideSorting = true;
            canvas.sortingOrder = sortingOrder;

            RectTransform root = GetComponent<RectTransform>();
            GameObject inst = Instantiate(prefab, root, false);
            var rt = (RectTransform)inst.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;

            Vector2 size = rt.rect.size;
            root.sizeDelta = size;
            root.localScale = Vector3.one * (worldSize / Mathf.Max(1f, Mathf.Max(size.x, size.y)));

            // 날아오는 UI라 클릭/조작은 막는다
            foreach (var sel in inst.GetComponentsInChildren<UnityEngine.UI.Selectable>()) sel.interactable = false;

            if (!TryGetComponent(out BoxCollider2D col)) col = gameObject.AddComponent<BoxCollider2D>();
            col.size = size;
        }
        else
        {
            GameObject inst = Instantiate(prefab, transform, false);
            inst.transform.localPosition = Vector3.zero;
            foreach (var r in inst.GetComponentsInChildren<SpriteRenderer>()) r.sortingOrder = sortingOrder;

            if (GetComponent<BoxCollider2D>() == null)
            {
                Bounds b = new Bounds(transform.position, Vector3.zero);
                foreach (var r in inst.GetComponentsInChildren<Renderer>()) b.Encapsulate(r.bounds);
                BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
                col.size = b.size;
                col.offset = transform.InverseTransformPoint(b.center);
            }
        }
    }

    private void RandomSprite(KTH_RandomSpriteListSo so)
    {
        var list = so.SpriteList;
        if (list.Count == 0) return;
        sr.sprite = list[Random.Range(0, list.Count)];
    }

    private void Go(KTH_Boss boss, KTH_AttackBundle bundle, KTH_UiThrowStyle style)
    {
        var hit = KTH_Throw.Judge(boss.target, bundle, transform.position, 1, result =>
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
