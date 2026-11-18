using DG.Tweening;
using UnityEngine;

public class KTH_ParryTelegraph : MonoBehaviour
{
    [SerializeField] private SpriteRenderer target;
    [SerializeField] private Color color = new(1f, 0.85f, 0.1f);
    [SerializeField] private float flashTime = 0.08f;
    [SerializeField] private int flashCount = 2;

    [SerializeField] private GameObject effectPrefab;
    [SerializeField] private Vector3 effectOffset = new(0, 1f, 0);
    [SerializeField] private float effectLifetime = 0.5f;

    private Color originColor;

    private void Awake()
    {
        if (target == null) TryGetComponent(out target);
        if (target != null) originColor = target.color;
    }

    public void Play()
    {
        if (target != null)
        {
            target.DOKill();
            target.color = originColor;
            target.DOColor(color, flashTime).SetLoops(flashCount * 2, LoopType.Yoyo)
                .OnKill(() => target.color = originColor);
        }

        if (effectPrefab != null)
        {
            GameObject fx = Instantiate(effectPrefab, transform.position + effectOffset, Quaternion.identity, transform);
            Destroy(fx, effectLifetime);
        }
    }
}
