using System;
using UnityEngine;

public class KTH_RestBossPattern1 : KTH_BossAction
{
    [SerializeField]private KTH_Fire fireBallPrefab;
    [SerializeField]private Transform fireTransform;
    [SerializeField]private float windupTime = 0.4f;
    [SerializeField]private float flyDuration = 0.6f;
    [SerializeField]private KTH_AttackBundle bundle = new();

    [Header("강화: 큰 불꽃")]
    [SerializeField]private KTH_Fire enhancedPrefab;
    [SerializeField]private float enhancedWindupTime = 0.6f;
    [SerializeField]private float enhancedFlyDuration = 0.8f;
    [SerializeField]private KTH_AttackBundle enhancedBundle = new() { baseDamage = 20f };

    private Animator animator;
    
    private readonly int FireHash = Animator.StringToHash("IsFire");


    private void Awake()
    {
        animator = transform.GetChild(0).GetComponentInChildren<Animator>();
    }

    public override void Execute(KTH_Boss boss) => ExecuteTurn(boss, false, null);

    public override void ExecuteEnhanced(KTH_Boss boss) => ExecuteTurn(boss, true, null);

    public override void ExecuteTurn(KTH_Boss boss, bool enhanced, Action onComplete)
    {
        if (enhanced)
            Fire(boss, enhancedPrefab, enhancedWindupTime, enhancedFlyDuration, enhancedBundle, onComplete);
        else
            Fire(boss, fireBallPrefab, windupTime, flyDuration, bundle, onComplete);
        animator.SetTrigger(FireHash);
    }

    private void Fire(KTH_Boss boss, KTH_Fire prefab, float windup, float fly, KTH_AttackBundle data, Action onComplete)
    {
        KTH_Throw.Windup(boss, data, windup, () => FireBallSpawn(boss, prefab, fly, data, onComplete));
    }

    private void FireBallSpawn(KTH_Boss boss, KTH_Fire prefab, float fly, KTH_AttackBundle data, Action onComplete)
    {
        KTH_Fire fireBall = Instantiate(prefab, fireTransform.position, Quaternion.identity);
        Action hit = KTH_Throw.Judge(boss.target, data, fireTransform.position, 1, _ =>
        {
            fireBall.Explode();
            onComplete?.Invoke();
        });
        fireBall.Launch(boss.target, fly, hit);
    }
}
