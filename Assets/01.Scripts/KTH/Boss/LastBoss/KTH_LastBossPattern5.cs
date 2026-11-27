using System;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class Pattern5Data
{
    [field: SerializeField]public float WindupTime{get; private set;}        // 이동 시작 전 대기
    [field: SerializeField]public float ApproachDuration{get; private set;}  // 플레이어 앞까지 이동 시간
    [field: SerializeField]public float Reach{get; private set;}             // 플레이어와 떨어져 멈출 거리
    [field: SerializeField]public float ReturnDuration{get; private set;}    // 원위치 복귀 시간
    [field: SerializeField]public float AttackDelayTime{get; private set;}   // 애니 시작 → 타격 프레임까지
    [field: SerializeField]public float RecoverTime{get; private set;}       // 타격 후 복귀 전 멈춤
}

public class KTH_LastBossPattern5 : KTH_BossAction
{
    [SerializeField]private KTH_AttackBundle bundle = new() { baseDamage = 10f };
    [SerializeField] private Pattern5Data p5d;

    [Header("애니메이션")]
    [SerializeField] private string scratchTrigger = "IsScratch";

    [Header("패리 당했을 때 튕김")]
    [SerializeField] private float knockbackAngle = 30f;
    [SerializeField] private float knockbackDistance = 2f;
    [SerializeField] private float knockbackDuration = 0.3f;

    private Tween tween;

    private void OnDisable() => tween?.Kill();

    public override void Execute(KTH_Boss kthBoss)
    {
        Scratch(kthBoss);
    }

    private void Scratch(KTH_Boss kthBoss)
    {
        tween?.Kill();

        Transform t = kthBoss.transform;
        Transform target = kthBoss.target;
        Vector3 origin = t.position;
        float dir = KTH_Throw.Direction(t, target);
        float stopX = target.position.x - dir * p5d.Reach;

        Animator animator = kthBoss.GetComponentInChildren<Animator>();
        kthBoss.TryGetComponent(out KTH_ParryTelegraph telegraph);

        Sequence seq = DOTween.Sequence().SetLink(gameObject);

        // 패리 결과는 타격 직후 또는 입력 시점에 늦게 올 수 있어서 콜백에서 처리
        Action hit = KTH_Throw.Judge(kthBoss, bundle, origin, 1, r =>
        {
            if (r.parried) Knockback(t, origin, dir);
        });

        seq.AppendInterval(p5d.WindupTime);
        seq.Append(t.DOMoveX(stopX, p5d.ApproachDuration).SetEase(Ease.OutQuad));
        seq.AppendCallback(() =>
        {
            if (animator != null && !string.IsNullOrEmpty(scratchTrigger))
                animator.SetTrigger(scratchTrigger);
            if (bundle.ShowsParryTelegraph && telegraph != null)
                telegraph.Play();
        });
        seq.AppendInterval(p5d.AttackDelayTime);
        seq.AppendCallback(() => hit());
        seq.AppendInterval(p5d.RecoverTime);
        seq.Append(t.DOMoveX(origin.x, p5d.ReturnDuration).SetEase(Ease.OutQuad));

        tween = seq;
    }

    private void Knockback(Transform t, Vector3 origin, float dir)
    {
        tween?.Kill();

        Sequence seq = DOTween.Sequence().SetLink(gameObject);
        seq.Append(KTH_Throw.Return(t, knockbackAngle, -dir, knockbackDistance, knockbackDuration));
        seq.Append(t.DOMove(origin, p5d.ReturnDuration).SetEase(Ease.OutQuad));
        tween = seq;
    }
}
