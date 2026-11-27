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

    private void RandomSprite(KTH_RandomSpriteListSo so)
    {
        var list = so.SpriteList;
        if (list.Count == 0) return;
        sr.sprite = list[Random.Range(0, list.Count)];
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
