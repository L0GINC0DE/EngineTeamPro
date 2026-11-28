using System;
using DG.Tweening;
using DG.Tweening.Core.Easing;
using UnityEngine;

public class KTH_ShieldBossPattern1 : KTH_BossAction
{
    [SerializeField]private float windupTime = 0.5f;
    [SerializeField]private float dashDuration = 1f;
    [SerializeField]private float returnDuration = 0.5f;
    [SerializeField] private Ease yEasing = Ease.OutQuad;
    [SerializeField] private AnimationCurve xEasing = new(
        new Keyframe(0f, 0f, 0f, 0f),
        new Keyframe(0.58922744f, 0.18454696f, 1.1424452f, 1.1424452f, 0.33333334f, 0.516581f),
        new Keyframe(0.7445119f, 0.9758533f, 0.19637091f, 0.19637091f, 0.33333334f, 0.3087318f),
        new Keyframe(1f, 1f, 0f, 0f));
    [SerializeField] private Ease returnXEasing = Ease.OutQuad;
    [SerializeField] private KTH_AttackBundle bundle = new();
    [SerializeField] private float parryOvershoot = 3f;
    [SerializeField] private float overshootDuration = 0.3f;

    public override void Execute(KTH_Boss kthBoss)
    {
        DashAttack(kthBoss);
        
    }

    private void DashAttack(KTH_Boss boss)
    {
        Transform t = boss.transform;
        Vector3 origin = t.position;
        
        float contactDist = HalfWidth(t) + HalfWidth(boss.target);
        bool dashing = false;
        bool hit = false;

        Sequence seq = DOTween.Sequence();
        Action contact = KTH_Throw.Judge(boss, bundle, origin, 1,
            r => { if (r.parried) Overshoot(boss, seq, origin); });

        seq.Append(t.DOMoveY(boss.target.position.y, dashDuration).SetEase(yEasing));
        if (bundle.ShowsParryTelegraph && boss.TryGetComponent(out KTH_ParryTelegraph telegraph))
            seq.AppendCallback(telegraph.Play);
        seq.AppendInterval(0.2f);
        seq.AppendCallback(() => dashing = true);
        seq.Append(t.DOMoveX(boss.target.position.x, dashDuration).SetEase(xEasing));
        seq.AppendCallback(() =>
        {
            dashing = false;
            if (!hit) { hit = true; contact(); }
        });
        seq.Append(t.DOMoveX(origin.x, returnDuration).SetEase(returnXEasing));
        seq.Append(t.DOMoveY(origin.y, returnDuration).SetEase(yEasing));

        seq.OnUpdate(() =>
        {
            if (!dashing || hit) return;
            if (Mathf.Abs(t.position.x - boss.target.position.x) > contactDist) return;
            hit = true;
            contact();
        });
    }

    private void Overshoot(KTH_Boss boss, Sequence dash, Vector3 origin)
    {
        dash.Kill();

        Transform t = boss.transform;
        float dir = Mathf.Sign(boss.target.position.x - origin.x);
        float endX = boss.target.position.x + dir * parryOvershoot;

        Sequence seq = DOTween.Sequence();
        seq.Append(t.DOMoveX(endX, overshootDuration).SetEase(Ease.OutQuad));
        seq.Append(t.DOMoveX(origin.x, returnDuration).SetEase(returnXEasing));
        seq.Append(t.DOMoveY(origin.y, returnDuration).SetEase(yEasing));
    }

    private static float HalfWidth(Transform tr)
    {
        if (tr.TryGetComponent(out Collider2D col)) return col.bounds.extents.x;
        if (tr.TryGetComponent(out SpriteRenderer sr)) return sr.bounds.extents.x;
        return 0;
    }
}
