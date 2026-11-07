using System;
using System.Collections;
using Agents;
using DG.Tweening;
using DG.Tweening.Core.Easing;
using UnityEngine;

// 돌진 공격. 기본값은 예전 Boss1Pattern1 에셋에서 맞춰둔 값
[Serializable]
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
    [SerializeField] private KTH_AttackBundle bundle = new(); // 피해량, 패리 가능 여부, 방어 타이밍
    [SerializeField] private float parryOvershoot = 3f;      // 패링당하면 플레이어를 지나 더 가는 거리
    [SerializeField] private float overshootDuration = 0.3f;

    public override void Execute(KTH_Boss kthBoss)
    {
        DashAttack(kthBoss);
        
    }

    private void DashAttack(KTH_Boss boss)
    {
        Transform t = boss.transform;
        Vector3 origin = t.position;
        
        // 돌진이 끝난 뒤(플레이어 중심 도착) 때리면 이미 몇 프레임 겹친 뒤라 늦으므로
        // 돌진 중 매 프레임 가장자리가 닿았는지 보고 닿은 그 프레임에 바로 때린다
        float contactDist = HalfWidth(t) + HalfWidth(boss.target);
        bool dashing = false;
        bool hit = false;

        Sequence seq = DOTween.Sequence();
        // 돌진은 묶음 1개, 입력은 접촉 순간. 판정이 끝나면(늦은 입력 대기 포함) 결과가 온다
        KTH_DefenseBundle defense = null;
        if (boss.target.TryGetComponent(out KTH_ParryModule judge))
            defense = judge.BeginBundle(bundle, 1, origin, r => { if (r.parried) Overshoot(boss, seq, origin); });

        seq.Append(t.DOMoveY(boss.target.position.y, dashDuration).SetEase(yEasing));
        // 돌진 직전에 노란 예고를 띄운다 (위치와 시간은 연출 조정 대상)
        if (bundle.parryable && boss.TryGetComponent(out KTH_ParryTelegraph telegraph))
            seq.AppendCallback(telegraph.Play);
        seq.AppendInterval(0.2f);
        seq.AppendCallback(() => dashing = true);
        seq.Append(t.DOMoveX(boss.target.position.x, dashDuration).SetEase(xEasing));
        seq.AppendCallback(() =>
        {
            dashing = false;
            if (!hit) { hit = true; Contact(boss, defense, origin); } // 끝까지 안 닿았으면 도착 시점에 처리
        });
        seq.Append(t.DOMoveX(origin.x, returnDuration).SetEase(returnXEasing));
        seq.Append(t.DOMoveY(origin.y, returnDuration).SetEase(yEasing));

        // 트윈이 위치를 옮긴 직후에 호출되므로 닿은 프레임과 같은 프레임에 넉백이 들어간다
        seq.OnUpdate(() =>
        {
            if (!dashing || hit) return;
            if (Mathf.Abs(t.position.x - boss.target.position.x) > contactDist) return;
            hit = true;
            Contact(boss, defense, origin);
        });
    }

    // 판정기가 있으면 판정에 맡기고, 없으면(판정 안 받는 대상) 기본 피해만 준다
    private void Contact(KTH_Boss boss, KTH_DefenseBundle defense, Vector3 origin)
    {
        if (defense != null)
        {
            defense.Hit();
            return;
        }

        if (boss.target.TryGetComponent(out HealthModule hm))
            hm.TakeDamage(Mathf.RoundToInt(bundle.baseDamage * bundle.damageCoefficient), origin);
    }

    // 패링당하면 플레이어 위치에서 멈추지 않고 그대로 지나쳐 더 간 뒤 원래 자리로 돌아온다
    private void Overshoot(KTH_Boss boss, Sequence dash, Vector3 origin)
    {
        dash.Kill();

        Transform t = boss.transform;
        float dir = Mathf.Sign(boss.target.position.x - origin.x);
        float endX = boss.target.position.x + dir * parryOvershoot;

        // 돌진 속도가 이어지도록 빠르게 출발해서 감속하는 OutQuad를 쓴다
        Sequence seq = DOTween.Sequence();
        seq.Append(t.DOMoveX(endX, overshootDuration).SetEase(Ease.OutQuad));
        seq.Append(t.DOMoveX(origin.x, returnDuration).SetEase(returnXEasing));
        seq.Append(t.DOMoveY(origin.y, returnDuration).SetEase(yEasing));
    }

    // 콜라이더 → 스프라이트 순으로 가로 반폭을 구한다 (둘 다 없으면 중심끼리 닿아야 맞음)
    private static float HalfWidth(Transform tr)
    {
        if (tr.TryGetComponent(out Collider2D col)) return col.bounds.extents.x;
        if (tr.TryGetComponent(out SpriteRenderer sr)) return sr.bounds.extents.x;
        return 0;
    }
}
