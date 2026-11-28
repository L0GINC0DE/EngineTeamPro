using DG.Tweening;
using UnityEngine;

public static class KTH_Knockback
{
    // 보스는 위, 플레이어는 아래라서 넉백은 위아래로
    public static Sequence Knockback(Transform target, float dir, KTH_HitEffectSO data)
    {
        target.DOKill(true);
        float originY = target.position.y;

        Sequence seq = DOTween.Sequence().SetTarget(target);
        seq.Append(target.DOMoveY(originY + dir * data.knockbackDistance, data.pushTime).SetEase(data.pushEase));

        if (data.returnToOrigin)
        {
            seq.AppendInterval(data.holdTime);
            seq.Append(target.DOMoveY(originY, data.returnTime).SetEase(data.returnEase));
        }

        seq.Goto(Time.deltaTime, true);
        return seq;
    }
}
