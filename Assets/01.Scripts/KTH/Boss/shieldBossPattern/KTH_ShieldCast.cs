using Agents;
using DG.Tweening;
using UnityEngine;

// 보스 자식으로 두는 방패. 날아가다 닿으면 판정하고 돌아온다
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
    
    // count번 던지고 돌아오기를 반복한다
    public void Throw(KTH_Boss boss, KTH_AttackBundle bundle, float flyDuration, float returnDuration, int count)
    {
        animator.ResetTrigger(HitEnd); // 지난 패턴에서 남은 트리거가 있으면 바로 Idle로 빠지지 않게
        animator.SetTrigger(ThrowHash); // 던지는 동안은 Throw_Loop에 머무니 시작할 때 한 번만
        ThrowOnce(boss, bundle, flyDuration, returnDuration, count, count);
    }

    private void ThrowOnce(KTH_Boss boss, KTH_AttackBundle bundle, float flyDuration, float returnDuration,
        int total, int remaining)
    {
        sr.flipX = false; // 날아갈 땐 정방향. 마지막에 돌아온 뒤에도 여기서 꺼진다
        if (remaining <= 0)
        {
            animator.SetTrigger(HitEnd); // 모든 던지기가 끝나면 Idle로
            return;
        }
        void Back() => Return(returnDuration, () =>
            ThrowOnce(boss, bundle, flyDuration, returnDuration, total, remaining - 1));

        Transform target = boss.target;
        Collider2D myCol = GetComponent<Collider2D>();
        Collider2D targetCol = target.GetComponent<Collider2D>();

        KTH_DefenseBundle defense = null;
        if (target.TryGetComponent(out KTH_ParryModule judge))
            defense = judge.BeginBundle(bundle, total, transform.position); // 묶음 수 = 던지는 횟수
        
        tween?.Kill();
        tween = transform.DOMove(target.position, flyDuration).SetEase(Ease.Linear)
            .OnUpdate(() =>
            {
                if (!myCol.bounds.Intersects(targetCol.bounds)) return;
                Hit(target, bundle, defense);
                Back();
            })
            .OnComplete(Back); // 빗나가도 돌아온다
    }

    // 돌아온 뒤 onArrived로 다음 던지기를 이어간다
    private void Return(float duration, TweenCallback onArrived)
    {
        tween?.Kill();
        tween = transform.DOLocalMove(restLocalPosition, duration).SetEase(Ease.OutQuad).OnComplete(onArrived);
        
        sr.flipX = true;
    }

    private void Hit(Transform target, KTH_AttackBundle bundle, KTH_DefenseBundle defense)
    {
        animator.SetTrigger(HitHash); // 판정기 유무와 상관없이 닿으면 재생

        if (defense != null) defense.Hit();
        else if (target.TryGetComponent(out HealthModule hm))
            hm.TakeDamage(Mathf.RoundToInt(bundle.baseDamage * bundle.damageCoefficient), transform.position);
    }
}
