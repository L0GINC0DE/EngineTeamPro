using System;
using DG.Tweening;
using UnityEngine;

public class KTH_RestBossPattern2 : KTH_BossAction
{
    [SerializeField]private Transform branch;
    [SerializeField]private float windupTime = 0.4f;
    [SerializeField]private float flyDuration = 0.7f;
    [Tooltip("날아가는 동안 회전")]
    [SerializeField]private bool spin = true;
    [SerializeField]private float spinDuration = 0.3f;
    [SerializeField]private KTH_AttackBundle bundle = new();
    [SerializeField, Min(1)]private int maxParry = 3;
    [SerializeField]private float bounceHeight = 3f;
    [SerializeField]private float bounceDuration = 0.4f;
    [SerializeField]private float fallDuration = 0.4f;
    [SerializeField]private float reflectAngle = 45f;
    [SerializeField]private float reflectDistance = 8f;
    [SerializeField]private float reflectDuration = 0.5f;

    [Header("강화: 화차")]
    [Tooltip("발사 전 보스 앞에서 제자리 회전하는 시간 (이 동안 화차 애니메이션 재생)")]
    [SerializeField]private float enhancedWindupTime = 0.5f;
    [SerializeField]private float enhancedFlyDuration = 0.7f;
    [SerializeField]private bool enhancedSpin = false;
    [SerializeField]private KTH_AttackBundle enhancedBundle = new() { baseDamage = 20f };

    private Animator animator;
    
    private readonly int SpinHash=Animator.StringToHash("IsFireCar");
    
    private Vector3 restPosition;
    private Quaternion restRotation;
    private Tween spinTween;

    private KTH_AttackBundle current;

    private void Awake()
    {
        restPosition = branch.localPosition;
        restRotation = branch.localRotation;
        animator = branch.GetComponentInChildren<Animator>();
    }

    public override void Execute(KTH_Boss kthBoss)
    {
        Begin(kthBoss, false, bundle, windupTime, flyDuration, spin);
    }

    public override void ExecuteEnhanced(KTH_Boss kthBoss)
    {
        Begin2(kthBoss, true, enhancedBundle, enhancedWindupTime, enhancedFlyDuration, enhancedSpin);

    }

    private void Begin(KTH_Boss boss, bool isEnhanced, KTH_AttackBundle data, float windup, float fly, bool doSpin)
    {
        current = data;
        KTH_Throw.Windup(boss, current, windup, () => Fire(boss, fly, doSpin));
        
    }
    // 화차: 보스 앞 제자리에서 회전 + 애니메이션 → 예고 시간이 끝나면 애니메이션 종료 → 회전 정지 → 플레이어에게 발사
    private void Begin2(KTH_Boss boss, bool isEnhanced, KTH_AttackBundle data, float windup, float fly, bool doSpin)
    {
        current = data;

        spinTween?.Kill();
        spinTween = branch.DOLocalRotate(new Vector3(0, 0, 360), spinDuration, RotateMode.FastBeyond360)
            .SetRelative().SetEase(Ease.Linear).SetLoops(-1);
        if (animator != null) animator.SetBool(SpinHash, true);

        KTH_Throw.Windup(boss, current, windup, () =>
        {
            if (animator != null) animator.SetBool(SpinHash, false);
            spinTween?.Kill();
            branch.localRotation = restRotation;
            Fire(boss, fly, doSpin);
        });
    }

    private void Fire(KTH_Boss boss, float fly, bool doSpin)
    {
        spinTween?.Kill();
        if (doSpin)
            spinTween = branch.DORotate(new Vector3(0, 0, 360), spinDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear).SetLoops(-1);

        KTH_Throw.Throw(branch, boss.target, fly, Hit(boss, 0));
    }

    private Action Hit(KTH_Boss boss, int parried)
    {
        return KTH_Throw.Judge(boss.target, current, branch.position, maxParry, r =>
        {
            Debug.Log($"[화차] {parried + 1}번째 판정 - perfect:{r.perfect} good:{r.good} fail:{r.fail} parried:{r.parried}");
            if (!r.parried)
            {
                End();
                return;
            }

            float back = KTH_Throw.Direction(boss.target, boss.transform);
            int count = parried + 1;

            if (count >= maxParry)
            {
                KTH_Throw.Return(branch, reflectAngle, back, reflectDistance, reflectDuration).OnComplete(End);
                return;
            }

            Action next = Hit(boss, count);
            KTH_Throw.Return(branch, 90, back, bounceHeight, bounceDuration)
                .OnComplete(() => KTH_Throw.Throw(branch, boss.target, fallDuration, next));
        });
    }

    private void End()
    {
        spinTween?.Kill();
        branch.localPosition = restPosition;
        branch.localRotation = restRotation;
        if (animator != null) animator.SetBool(SpinHash, false);
    }

    
}
