using System;
using UnityEngine;

// 스피커에서 퍼져나가는 원형 음파. 반지름이 플레이어까지 닿으면 판정
public class KTH_SonicRing : MonoBehaviour
{
    private const int Segments = 64;
    private static Material material;

    private LineRenderer line;
    private float speed;
    private float radius;
    private float hitRadius;
    private float maxRadius;
    private float width;
    private Color color;
    private Action onHit;

    private void Awake()
    {
        if (material == null)
            material = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));

        line = gameObject.AddComponent<LineRenderer>();
        line.sharedMaterial = material;
        line.useWorldSpace = false;
        line.loop = true;
        line.positionCount = Segments;
        line.sortingOrder = 10;
    }

    public void Launch(Vector3 target, KTH_ShoutStyle style, Action onHit)
    {
        target.z = transform.position.z;
        hitRadius = Vector3.Distance(transform.position, target);
        maxRadius = hitRadius + style.extraRadius;
        speed = Mathf.Max(hitRadius, 1) / style.travelTime; // 거리랑 상관없이 travelTime 뒤에 닿는다
        width = style.width;
        color = style.color;
        this.onHit = onHit;
        Draw();
    }

    private void Update()
    {
        radius += speed * Time.deltaTime;

        if (onHit != null && radius >= hitRadius)
            Hit();

        Draw();

        if (radius >= maxRadius)
            Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (onHit != null) Hit(); // 중간에 사라져도 판정은 한다
    }

    private void Hit()
    {
        Action callback = onHit;
        onHit = null;
        callback();
    }

    private void Draw()
    {
        // 플레이어를 지나면 점점 옅고 가늘어진다
        float fade = 1 - Mathf.InverseLerp(hitRadius, maxRadius, radius);
        Color c = color;
        c.a *= fade;
        line.startColor = line.endColor = c;
        line.startWidth = line.endWidth = width * Mathf.Lerp(0.3f, 1, fade);

        for (int i = 0; i < Segments; i++)
        {
            float a = i * Mathf.PI * 2 / Segments;
            line.SetPosition(i, new Vector3(Mathf.Cos(a), Mathf.Sin(a)) * radius);
        }
    }
}
