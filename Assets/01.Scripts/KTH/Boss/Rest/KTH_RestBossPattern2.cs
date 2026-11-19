using System;
using DG.Tweening;
using UnityEngine;

public class KTH_RestBossPattern2 : KTH_BossAction
{
    [SerializeField]private Transform branch;
    [SerializeField]private float windupTime = 0.4f;
    [SerializeField]private float flyDuration = 0.7f;
    [SerializeField]private float spinDuration = 0.3f;
    [SerializeField]private KTH_AttackBundle bundle = new();
    [SerializeField, Min(1)]private int maxParry = 3;
    [SerializeField]private float bounceHeight = 3f;
    [SerializeField]private float bounceDuration = 0.4f;
    [SerializeField]private float fallDuration = 0.4f;
    [SerializeField]private float reflectAngle = 45f;
    [SerializeField]private float reflectDistance = 8f;
    [SerializeField]private float reflectDuration = 0.5f;

    private Vector3 restPosition;
    private Quaternion restRotation;
    private Tween spin;

    private void Awake()
    {
        restPosition = branch.localPosition;
        restRotation = branch.localRotation;
    }

    public override void Execute(KTH_Boss kthBoss)
    {
        KTH_Throw.Windup(kthBoss, bundle, windupTime, () => Fire(kthBoss));
    }

    private void Fire(KTH_Boss boss)
    {
        spin?.Kill();
        spin = branch.DORotate(new Vector3(0, 0, 360), spinDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear).SetLoops(-1);

        KTH_Throw.Throw(branch, boss.target, flyDuration, Hit(boss, 0));
    }

    private Action Hit(KTH_Boss boss, int parried)
    {
        return KTH_Throw.Judge(boss.target, bundle, branch.position, maxParry, r =>
        {
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
        spin?.Kill();
        branch.localPosition = restPosition;
        branch.localRotation = restRotation;
    }
}
