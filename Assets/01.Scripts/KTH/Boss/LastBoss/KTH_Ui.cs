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

        // 콜라이더 크기를 스프라이트에 맞춘다 (스프라이트를 넣은 뒤에 해야 함)
        if (TryGetComponent(out BoxCollider2D col) && sr.sprite != null)
            col.size = sr.sprite.bounds.size;

        Go(boss, bundle, style);
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

    /// <summary>패링당하면 보스 쪽으로 회전하며 튕겨 나간 뒤 사라진다.</summary>
    private void FlyAway(KTH_Boss boss, KTH_UiThrowStyle style)
    {
        transform.SetParent(null, true); // 보스가 움직여도 따라가지 않게 떼어낸다

        float back = KTH_Throw.Direction(boss.target, boss.transform);

        if (style.spinDuration > 0f)
            transform.DORotate(new Vector3(0, 0, 360 * -back), style.spinDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear).SetLoops(-1).SetLink(gameObject);

        KTH_Throw.Return(transform, style.flyAwayAngle, back, style.flyAwayDistance, style.flyAwayDuration)
            .SetLink(gameObject)
            .OnComplete(() => Destroy(gameObject));
    }
}
