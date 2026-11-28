using System;
using _01.Scripts.TAE.Agents;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class Pattern6Data
{
    public float chargeTime = 1.2f;
    public float beamDuration = 2.5f;
    public float beamWidth = 6f;
    [Range(0, 1)] public float successRatio = 0.8f;
    public float tickInterval = 0.25f;
    public int tickDamage = 5;
    public LayerMask hitMask = ~0;
}

// 레이저 : 차지 후 발사, 나가는 동안 패리키를 꾹 누르고 있어야 함
// 매 프레임 레이캐스트로 맞은 걸 보고 막는 중이면 플레이어 앞에서 끊김
public class KTH_LastBossPattern6 : KTH_BossAction
{
    private const float Overreach = 6f;
    private const float GrowTime = 0.15f;
    private const float StartGrace = 0.15f;
    private const float HoldShake = 0.15f;
    private const float HitShake = 0.35f;
    private const string ChargeTrigger = "IsLaserCharge";
    private const string FireBool = "IsLaser";
    [SerializeField] private Transform muzzle;
    [SerializeField] private KTH_LaserBeam laserPrefab;
    [SerializeField] private GameObject chargePrefab;
    [SerializeField] private KTH_AttackBundle bundle = new() { baseDamage = 0f };
    [SerializeField] private Pattern6Data p6d = new();

    private KTH_LaserBeam beam;
    private GameObject charge;
    private Sequence seq;
    private Action endBeam;

    private void Awake()
    {
        // 보스가 뒤집혀도 빔은 안 뒤집히게 부모 없이 생성
        if (laserPrefab != null) beam = Instantiate(laserPrefab);
    }

    private void OnDestroy()
    {
        if (beam != null) Destroy(beam.gameObject);
    }

    private void OnDisable()
    {
        seq?.Kill();
        endBeam?.Invoke();
        StopCharge();
        if (beam != null) beam.Stop();
    }

    public override void Execute(KTH_Boss kthBoss)
    {
        seq?.Kill();

        Animator animator = kthBoss.GetComponentInChildren<Animator>();
        SetTrigger(animator, ChargeTrigger);
        StartCharge(kthBoss);

        KTH_Throw.Windup(kthBoss, bundle, p6d.chargeTime, () =>
        {
            StopCharge();
            SetBool(animator, FireBool, true);
            Fire(kthBoss, _ => SetBool(animator, FireBool, false));
        });
    }

    // 차지 이펙트가 1초짜리라 chargeTime에 맞춰 속도 조절
    private void StartCharge(KTH_Boss kthBoss)
    {
        StopCharge();
        if (chargePrefab == null) return;

        Transform at = muzzle != null ? muzzle : kthBoss.transform;
        charge = Instantiate(chargePrefab, at.position, Quaternion.identity, at);
        float speed = p6d.chargeTime > 0f ? 1f / p6d.chargeTime : 1f;
        foreach (ParticleSystem ps in charge.GetComponentsInChildren<ParticleSystem>())
        {
            var main = ps.main;
            main.simulationSpeed = speed;
        }
    }

    private void StopCharge()
    {
        if (charge != null) Destroy(charge);
        charge = null;
    }

    private void Fire(KTH_Boss kthBoss, Action<bool> onEnd)
    {
        Transform target = kthBoss.target;
        target.TryGetComponent(out KTH_ParryModule parry);
        target.TryGetComponent(out HealthModule targetHm);

        // 꾹 누르는 동안 헛친 입력으로 안 잠기게
        parry?.BeginMash();

        float elapsed = 0f, safe = 0f, tickTimer = 0f;
        bool ended = false;

        if (beam != null)
        {
            Shot first = Cast(kthBoss, target);
            beam.Aim(first.from, first.dir, first.maxDistance);
            beam.Fire();
        }

        void End()
        {
            if (ended) return;
            ended = true;
            endBeam = null;
            parry?.EndMash();

            float ratio = p6d.beamDuration > 0f ? safe / p6d.beamDuration : 1f;
            bool success = ratio >= p6d.successRatio;
            Debug.Log($"레이저 {ratio:P0} {success}");

            if (success) parry?.OnParryEvent?.Invoke();

            if (beam != null) beam.Stop();
            onEnd?.Invoke(success);
        }
        endBeam = End;

        seq = DOTween.Sequence().SetLink(gameObject);
        seq.AppendInterval(p6d.beamDuration);
        seq.OnUpdate(() =>
        {
            if (ended || target == null) return;
            float dt = Time.deltaTime;
            elapsed += dt;

            Shot shot = Cast(kthBoss, target);
            bool holding = parry != null && parry.IsHolding;
            bool hitPlayer = shot.hit && shot.hitTarget;

            if (!hitPlayer || holding || elapsed <= StartGrace) safe += dt;
            DrawBeam(shot, hitPlayer, holding, elapsed);

            tickTimer += dt;
            if (tickTimer < p6d.tickInterval) return;
            tickTimer -= p6d.tickInterval;

            if (!hitPlayer) return;
            if (holding)
            {
                Shake(HoldShake);
            }
            else if (elapsed > StartGrace)
            {
                Shake(HitShake);
                if (targetHm != null) targetHm.TakeDamage(p6d.tickDamage, shot.from);
            }
        });
        seq.OnComplete(End);
        seq.OnKill(End);
    }

    private void DrawBeam(Shot shot, bool hitPlayer, bool holding, float elapsed)
    {
        if (beam == null) return;

        // 막히면 거기서 끊고 아니면 끝까지
        bool blocked = hitPlayer ? holding : shot.hit;
        beam.Aim(shot.from, shot.dir, blocked ? shot.distance : shot.maxDistance);
        beam.SetImpact(blocked);

        float grow = Mathf.Clamp01(Mathf.Min(elapsed, p6d.beamDuration - elapsed) / GrowTime);
        beam.SetWidth(p6d.beamWidth * grow);
    }

    private struct Shot
    {
        public Vector2 from;
        public Vector2 dir;
        public float maxDistance;
        public bool hit;
        public bool hitTarget;
        public float distance;
    }

    // 보스 자기 콜라이더는 건너뜀
    private Shot Cast(KTH_Boss kthBoss, Transform target)
    {
        Shot shot = new Shot { from = muzzle != null ? muzzle.position : kthBoss.transform.position };
        Vector2 toTarget = (Vector2)target.position - shot.from;
        float dist = toTarget.magnitude;
        shot.dir = dist > 0.001f ? toTarget / dist : Vector2.left;
        shot.maxDistance = dist + Overreach;

        RaycastHit2D[] hits = Physics2D.RaycastAll(shot.from, shot.dir, shot.maxDistance, p6d.hitMask);
        foreach (RaycastHit2D h in hits)
        {
            if (h.collider == null || h.collider.transform.IsChildOf(kthBoss.transform)) continue;

            shot.hit = true;
            shot.distance = h.distance;
            Transform t = h.collider.attachedRigidbody != null ? h.collider.attachedRigidbody.transform : h.collider.transform;
            shot.hitTarget = t == target || h.collider.transform.IsChildOf(target);
            break;
        }

        // 플레이어 콜라이더 없으면 그냥 플레이어 위치에서 맞은 걸로
        if (target.GetComponentInChildren<Collider2D>() == null && (!shot.hit || shot.distance > dist))
        {
            shot.hit = true;
            shot.hitTarget = true;
            shot.distance = dist;
        }
        return shot;
    }

    private static void Shake(float force)
    {
        if (force > 0f && KTH_DamageEffectManager.instance != null)
            KTH_DamageEffectManager.instance.CameraShake(force);
    }

    private static void SetTrigger(Animator animator, string name)
    {
        if (animator == null) return;
        foreach (AnimatorControllerParameter p in animator.parameters)
            if (p.name == name && p.type == AnimatorControllerParameterType.Trigger)
            {
                animator.SetTrigger(name);
                return;
            }
    }

    private static void SetBool(Animator animator, string name, bool on)
    {
        if (animator == null) return;
        foreach (AnimatorControllerParameter p in animator.parameters)
            if (p.name == name && p.type == AnimatorControllerParameterType.Bool)
            {
                animator.SetBool(name, on);
                return;
            }
    }
}
