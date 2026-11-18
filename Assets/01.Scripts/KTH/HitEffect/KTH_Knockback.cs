using DG.Tweening;
using UnityEngine;

public static class KTH_Knockback
{
    public static Sequence Knockback(Transform target, float dir, KTH_HitEffectSO data)
    {
        target.DOKill(true);
        float originX = target.position.x;

        Sequence seq = DOTween.Sequence().SetTarget(target);
        seq.Append(target.DOMoveX(originX + dir * data.knockbackDistance, data.pushTime).SetEase(data.pushEase));

        if (data.returnToOrigin)
        {
            seq.AppendInterval(data.holdTime);
            seq.Append(target.DOMoveX(originX, data.returnTime).SetEase(data.returnEase));
        }

        seq.Goto(Time.deltaTime, true);
        return seq;
    }
}
