using System;
using DG.Tweening;
using UnityEngine;

public class KTH_Fire : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;
    
    [SerializeField] private float fadeTime = 0.25f;
    [SerializeField] private float fadeScale = 1.3f;
    [SerializeField] private Ease fadeEase = Ease.OutQuad;

    private Tween move;
    private bool exploding;
    private Vector3 lastPosition;

    private void Awake()
    {
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        lastPosition = transform.position;
    }

    public void Launch(Transform target, float duration, Action onHit)
    {
        Face(target.position - transform.position);
        move?.Kill();
        move = KTH_Throw.Throw(transform, target, duration, onHit);
    }

    public void Explode()
    {
        if (exploding) return;
        exploding = true;
        move?.Kill();

        if (sr == null)
        {
            Destroy(gameObject);
            return;
        }

        move = DOTween.Sequence()
            .Append(sr.DOFade(0, fadeTime).SetEase(fadeEase))
            .Join(transform.DOScale(transform.localScale * fadeScale, fadeTime).SetEase(fadeEase))
            .OnComplete(() => Destroy(gameObject));
    }

    private void LateUpdate()
    {
        if (exploding) return;
        Vector3 delta = transform.position - lastPosition;
        lastPosition = transform.position;
        Face(delta);
    }

    private void Face(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.000001f) return;

        // 스프라이트가 왼쪽(-X)을 향하므로 -X축이 진행 방향을 보도록 회전
        float angle = Mathf.Atan2(-dir.y, -dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void OnDestroy()
    {
        move?.Kill();
    }
}
