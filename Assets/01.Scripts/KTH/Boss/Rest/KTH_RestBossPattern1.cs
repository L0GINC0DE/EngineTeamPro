using System;
using DG.Tweening;
using UnityEngine;

public class KTH_RestBossPattern1 : KTH_BossAction
{
    [SerializeField]private GameObject fireBallPrefab;
    [SerializeField]private Transform fireTransform;
    [SerializeField]private float windupTime = 0.4f;
    [SerializeField]private float flyDuration = 0.6f;
    [SerializeField]private KTH_AttackBundle bundle = new();
    [SerializeField]private float reflectAngle = 30f;
    [SerializeField]private float reflectDistance = 6f;
    [SerializeField]private float reflectDuration = 0.4f;

    public override void Execute(KTH_Boss kthBoss)
    {
        KTH_Throw.Windup(kthBoss, bundle, windupTime, () => FireBallSpawn(kthBoss));
    }

    private void FireBallSpawn(KTH_Boss boss)
    {
        Transform fireBall = GameObject.Instantiate(fireBallPrefab, fireTransform.position, Quaternion.identity).transform;

        Action hit = KTH_Throw.Judge(boss.target, bundle, fireTransform.position, 1, r =>
        {
            if (!r.parried)
            {
                GameObject.Destroy(fireBall.gameObject);
                return;
            }

            float back = KTH_Throw.Direction(boss.target, boss.transform);
            KTH_Throw.Return(fireBall, reflectAngle, back, reflectDistance, reflectDuration)
                .OnComplete(() => GameObject.Destroy(fireBall.gameObject));
        });

        KTH_Throw.Throw(fireBall, boss.target, flyDuration, hit);
    }
}
