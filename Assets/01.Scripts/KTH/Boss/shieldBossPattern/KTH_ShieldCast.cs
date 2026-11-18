using System;
using DG.Tweening;
using UnityEngine;

public class KTH_ShieldCast : MonoBehaviour
{
    private Vector3 restLocalPosition;
    private Tween tween;
    private Animator animator;

    private SpriteRenderer sr;
    
    private readonly int ThrowHash = Animator.StringToHash("IsThrow");
    private readonly int HitHash = Animator.StringToHash("IsThrowHit");
    private readonly int HitEnd = Animator.StringToHash("IsEnd");
    
    private void Awake()
    {
        sr=GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        restLocalPosition = transform.localPosition;
    }
    
    public void Throw(KTH_Boss boss, KTH_AttackBundle bundle, float flyDuration, float returnDuration, int count)
    {
        animator.ResetTrigger(HitEnd);
        animator.SetTrigger(ThrowHash);
        ThrowOnce(boss, bundle, flyDuration, returnDuration, count, count);
    }

    private void ThrowOnce(KTH_Boss boss, KTH_AttackBundle bundle, float flyDuration, float returnDuration,
        int total, int remaining)
    {
        sr.flipX = false;
        if (remaining <= 0)
        {
            animator.SetTrigger(HitEnd);
            return;
        }
        void Back() => Return(returnDuration, () =>
            ThrowOnce(boss, bundle, flyDuration, returnDuration, total, remaining - 1));

        Action hit = KTH_Throw.Judge(boss.target, bundle, transform.position, total);

        tween?.Kill();
        tween = KTH_Throw.Throw(transform, boss.target, flyDuration, () =>
        {
            animator.SetTrigger(HitHash);
            hit();
            Back();
        });
    }

    private void Return(float duration, TweenCallback onArrived)
    {
        tween?.Kill();
        tween = transform.DOLocalMove(restLocalPosition, duration).SetEase(Ease.OutQuad).OnComplete(onArrived);
        
        sr.flipX = true;
    }
}
