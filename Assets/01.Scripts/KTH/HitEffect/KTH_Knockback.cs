using DG.Tweening;
using UnityEngine;

public static class KTH_Knockback
{
    // 튕겨 나감 → 대기 → 복귀
    // 히트스탑 중에는 timeScale이 0이라 멈춰 있다가 풀리는 순간 튕겨 나간다
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

        // 새로 만든 트윈은 다음 프레임부터 움직이므로(한 틱 늦음) 이번 프레임 분량을 바로 적용
        seq.Goto(Time.deltaTime, true);
        return seq;
    }
}
