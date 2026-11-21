using System;
using _01.Scripts.TAE.Agents;
using DG.Tweening;
using UnityEngine;

public static class KTH_Throw
{
    public static void Windup(KTH_Boss boss, KTH_AttackBundle bundle, float time, Action onFire)
    {
        if (bundle.parryable && boss.TryGetComponent(out KTH_ParryTelegraph telegraph))
            telegraph.Play();

        DOTween.Sequence().AppendInterval(time).AppendCallback(() => onFire());
    }

    public static Tween Throw(Transform obj, Transform target, float duration, Action onHit)
    {
        Tween tween = null;
        bool hit = false;

        void Hit()
        {
            if (hit) return;
            hit = true;
            tween.Kill();
            onHit?.Invoke();
        }

        tween = obj.DOMove(target.position, duration).SetEase(Ease.Linear)
            .OnUpdate(() =>
            {
                if (Touching(obj, target)) Hit();
            })
            .OnComplete(Hit);
        return tween;
    }

    public static Tween Return(Transform obj, float angle, float direction, float distance, float duration)
    {
        float rad = angle * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(Mathf.Cos(rad) * direction, Mathf.Sin(rad));
        return obj.DOMove(obj.position + dir * distance, duration).SetEase(Ease.OutQuad);
    }

    public static float Direction(Transform from, Transform to)
    {
        return to.position.x >= from.position.x ? 1 : -1;
    }

    public static Action Judge(Transform target, KTH_AttackBundle bundle, Vector3 from, int bundleCount = 1,
        Action<KTH_DefenseResult> onResolved = null)
    {
        if (target.TryGetComponent(out KTH_ParryModule judge))
            return judge.BeginBundle(bundle, bundleCount, from, onResolved).Hit;
        return () => NoJudge(target, bundle, from, onResolved);
    }

    private static void NoJudge(Transform target, KTH_AttackBundle bundle, Vector3 from,
        Action<KTH_DefenseResult> onResolved)
    {
        if (target.TryGetComponent(out HealthModule hm))
            hm.TakeDamage(Mathf.RoundToInt(bundle.baseDamage * bundle.damageCoefficient), from);
        onResolved?.Invoke(new KTH_DefenseResult());
    }

    private static bool Touching(Transform a, Transform b)
    {
        return GetBounds(a).Intersects(GetBounds(b));
    }

    private static Bounds GetBounds(Transform tr)
    {
        if (tr.TryGetComponent(out Collider2D col)) return col.bounds;
        if (tr.TryGetComponent(out SpriteRenderer sr)) return sr.bounds;
        return new Bounds(tr.position, Vector3.zero);
    }
}
