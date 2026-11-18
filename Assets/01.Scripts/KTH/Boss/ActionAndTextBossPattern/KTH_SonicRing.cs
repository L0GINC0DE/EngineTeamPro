using System;
using UnityEngine;

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
    private bool returning;

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
        speed = Mathf.Max(hitRadius, 1) / style.travelTime;
        width = style.width;
        color = style.color;
        this.onHit = onHit;
        Draw();
    }

    public void Return(Color color, float speedMultiplier = 1.5f)
    {
        onHit = null;
        returning = true;
        this.color = color;
        speed *= speedMultiplier;
        if (radius > hitRadius) radius = hitRadius;
        Draw();
    }

    private void Update()
    {
        if (returning)
        {
            radius -= speed * Time.deltaTime;
            if (radius <= 0)
            {
                Destroy(gameObject);
                return;
            }
            Draw();
            return;
        }

        radius += speed * Time.deltaTime;

        if (onHit != null && radius >= hitRadius)
            Hit();

        Draw();

        if (radius >= maxRadius)
            Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (onHit != null) Hit();
    }

    private void Hit()
    {
        Action callback = onHit;
        onHit = null;
        callback();
    }

    private void Draw()
    {
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
