using UnityEngine;

// 레이저 프리팹에 붙음. 모양은 프리팹 파티클에서 바꾸고 여기선 위치, 방향, 길이, 굵기만 맞춤
// root(시작 쪽 가늘어지는 부분), body, tip(둥근 끝)은 파티클 1개씩이고 트랜스폼 스케일로 늘린다
public class KTH_LaserBeam : MonoBehaviour
{
    private const float BaseWidth = 1.2f; // 프리팹 파티클 크기를 맞춘 굵기

    [SerializeField] private Transform root;
    [SerializeField] private Transform body;
    [SerializeField] private Transform tip;
    [SerializeField] private ParticleSystem sparks;
    [SerializeField] private ParticleSystem impact;
    [SerializeField] private float rootLength = 2f;

    private ParticleSystem[] pieces;
    private float length = 1f;
    private float width;
    private float sparkRate;
    private ParticleSystem.MinMaxCurve sparkSize, sparkSpeed, impactSize, impactSpeed;
    private float impactRadius;

    private void Awake()
    {
        pieces = new[] { root.GetComponent<ParticleSystem>(), body.GetComponent<ParticleSystem>(), tip.GetComponent<ParticleSystem>() };
        sparkRate = sparks.emission.rateOverTimeMultiplier;
        sparkSize = sparks.main.startSize;
        sparkSpeed = sparks.main.startSpeed;
        impactSize = impact.main.startSize;
        impactSpeed = impact.main.startSpeed;
        impactRadius = impact.shape.radius;
        Stop();
    }

    public void Fire()
    {
        width = 0f;
        Refresh();
        foreach (ParticleSystem p in pieces) p.Play();
        sparks.Play();
    }

    public void SetWidth(float w)
    {
        width = Mathf.Max(0f, w);
        Refresh();
    }

    public void Aim(Vector2 origin, Vector2 dir, float len)
    {
        if (dir.sqrMagnitude < 0.000001f) dir = Vector2.right;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.SetPositionAndRotation(new Vector3(origin.x, origin.y, transform.position.z), Quaternion.Euler(0, 0, angle));
        length = Mathf.Max(0.01f, len);
        Refresh();
    }

    public void SetImpact(bool on)
    {
        if (on && !impact.isEmitting) impact.Play();
        else if (!on && impact.isEmitting) impact.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    public void Stop()
    {
        foreach (ParticleSystem p in pieces) p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        sparks.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        impact.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    private void Refresh()
    {
        float scale = width / BaseWidth;
        float tipLen = Mathf.Min(width * 0.5f, length * 0.3f);
        float rootLen = Mathf.Min(rootLength * scale, (length - tipLen) * 0.5f);
        float bodyLen = Mathf.Max(0f, length - tipLen - rootLen);

        Place(root, 0f, rootLen);
        Place(body, rootLen, bodyLen);
        Place(tip, rootLen + bodyLen, tipLen);

        // 굵기에 맞춰 도트 크기랑 속도도 같이 키움
        var sMain = sparks.main;
        sMain.startSize = Scaled(sparkSize, scale);
        sMain.startSpeed = Scaled(sparkSpeed, scale);
        var sShape = sparks.shape;
        sShape.radius = length * 0.5f;
        sShape.position = new Vector3(length * 0.5f, 0f, 0f);
        var sEmission = sparks.emission;
        sEmission.rateOverTimeMultiplier = width > 0.01f ? sparkRate * length : 0f;

        var iMain = impact.main;
        iMain.startSize = Scaled(impactSize, scale);
        iMain.startSpeed = Scaled(impactSpeed, scale);
        var iShape = impact.shape;
        iShape.radius = impactRadius * Mathf.Max(scale, 0.01f);
        impact.transform.localPosition = new Vector3(length, 0f, 0f);
    }

    private static ParticleSystem.MinMaxCurve Scaled(ParticleSystem.MinMaxCurve c, float s)
    {
        return new ParticleSystem.MinMaxCurve(c.constantMin * s, c.constantMax * s);
    }

    private void Place(Transform piece, float start, float len)
    {
        piece.localPosition = new Vector3(start + len * 0.5f, 0f, 0f);
        piece.localScale = new Vector3(Mathf.Max(len, 0.0001f), Mathf.Max(width, 0.0001f), 1f);
    }
}
