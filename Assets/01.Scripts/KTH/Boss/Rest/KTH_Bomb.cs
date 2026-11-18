using System;
using Agents;
using UnityEngine;
using DG.Tweening;


[Serializable]
public class KTH_BombStyle
{
    public float windup = 0.5f;
    public float hitInterval = 0.15f;
}

public static class KTH_Bomb
{
    public static void Explosion(KTH_Boss boss, KTH_AttackBundle bundle, KTH_BombStyle style,
        Action<KTH_DefenseResult> onDone = null)
    {
        Vector3 position = boss.transform.position;
        Transform target = boss.target;
        int count = bundle.inputCount;

        if (bundle.parryable && boss.TryGetComponent(out KTH_ParryTelegraph telegraph))
            telegraph.Play();

        KTH_ParryModule parry = null;
        if (target != null) target.TryGetComponent(out parry);

        Action hit;
        if (parry != null)
        {
            parry.BeginMash();
            hit = parry.BeginBundle(bundle, 1, position, result =>
            {
                parry.EndMash();
                onDone?.Invoke(result);
            }).Hit;
        }
        else
        {
            int damage = Mathf.RoundToInt(bundle.baseDamage * bundle.damageCoefficient / count);
            hit = () =>
            {
                if (target != null && target.TryGetComponent(out HealthModule hm))
                    hm.TakeDamage(damage, position);
            };
        }

        for (int i = 0; i < count; i++)
            DOVirtual.DelayedCall(style.windup + i * style.hitInterval, () => hit());
    }
}
