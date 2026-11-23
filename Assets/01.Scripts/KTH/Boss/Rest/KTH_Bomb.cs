using System;
using Agents;
using UnityEngine;
using DG.Tweening;


[Serializable]
public class KTH_BombStyle
{
    [Tooltip("차지 이펙트 유지 시간")]
    public float windup = 0.5f;
    [Tooltip("플레이어 앞까지 날아가는 시간")]
    public float flyDuration = 1f;
    [Tooltip("플레이어와 이 거리만큼 떨어진 곳에서 멈춘다")]
    public float stopDistance = 1.5f;
    [Tooltip("멈춘 상태에서 연타를 받는 제한 시간")]
    public float mashTime = 2f;
    public GameObject BombCharging;
    public GameObject BombFly;
    public GameObject Bomb;
}

/// <summary>
/// 태초의 폭발: 차지 → 플레이어 바로 앞까지 날아가서 멈춤 → 제한 시간 안에 bundle.inputCount번 연타.
/// 성공하면 터지고 보스 사망, 실패하면 터지고 플레이어 사망.
/// </summary>
public static class KTH_Bomb
{
    public static void Explosion(KTH_Boss boss, KTH_AttackBundle bundle, KTH_BombStyle style,
        Action<bool> onDone = null)
    {
        Vector3 position = boss.transform.position;
        Transform target = boss.target;
        int required = Mathf.Max(1, bundle.inputCount);

        if (bundle.parryable && boss.TryGetComponent(out KTH_ParryTelegraph telegraph))
            telegraph.Play();

        KTH_ParryModule parry = null;
        if (target != null) target.TryGetComponent(out parry);

        // 1. 차지
        SetActive(style.BombCharging, true);

        DOVirtual.DelayedCall(style.windup, () =>
        {
            // 2. 차지 끄고 플라이 켜서 플레이어 바로 앞까지
            Vector3 startPos = style.BombCharging != null ? style.BombCharging.transform.position : position;
            SetActive(style.BombCharging, false);

            Transform fly = style.BombFly != null ? style.BombFly.transform : null;
            if (fly != null)
            {
                fly.position = startPos;
                SetActive(style.BombFly, true);
            }

            Vector3 stopPos = startPos;
            if (target != null)
            {
                Vector3 toTarget = target.position - startPos;
                stopPos = target.position - toTarget.normalized * Mathf.Min(style.stopDistance, toTarget.magnitude);
            }

            Tween move = fly != null
                ? fly.DOMove(stopPos, style.flyDuration).SetEase(Ease.OutQuad)
                : DOVirtual.DelayedCall(style.flyDuration, () => { });

            // 3. 멈춘 상태에서 연타
            move.OnComplete(() => Mash(parry, required, style.mashTime, success =>
            {
                // 4. 플라이 끄고 터짐 → 성공이면 보스, 실패면 플레이어 사망
                SetActive(style.BombFly, false);
                PlayBomb(style, fly != null ? fly.position : stopPos);

                Transform victim = success ? boss.transform : target;
                Vector3 from = success ? stopPos : position;
                if (victim != null && victim.TryGetComponent(out HealthModule hm))
                    hm.TakeDamage(hm.CurrentHealth, from);

                onDone?.Invoke(success);
            }));
        });
    }

    /// <summary>제한 시간 안에 required번 누르면 즉시 성공, 시간이 다 되면 실패.</summary>
    private static void Mash(KTH_ParryModule parry, int required, float time, Action<bool> onEnd)
    {
        if (parry == null)
        {
            onEnd(false);
            return;
        }

        int presses = 0;
        bool ended = false;
        Tween timeout = null;

        void End(bool success)
        {
            if (ended) return;
            ended = true;
            parry.OnPressed -= OnPress;
            parry.EndMash();
            timeout?.Kill();
            onEnd(success);
        }

        void OnPress()
        {
            if (++presses >= required) End(true);
        }

        parry.BeginMash();
        parry.OnPressed += OnPress;
        timeout = DOVirtual.DelayedCall(time, () => End(false));
    }

    private static void PlayBomb(KTH_BombStyle style, Vector3 pos)
    {
        if (style.Bomb == null) return;
        style.Bomb.transform.position = pos;
        style.Bomb.SetActive(false); // 파티클 재생을 위해 재활성화
        style.Bomb.SetActive(true);
    }

    private static void SetActive(GameObject go, bool active)
    {
        if (go != null) go.SetActive(active);
    }
}
