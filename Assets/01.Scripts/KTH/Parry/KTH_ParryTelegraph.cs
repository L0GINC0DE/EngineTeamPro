using DG.Tweening;
using UnityEngine;

// 패리 가능한 공격의 노란 예고. 보스에 붙이고 패턴이 공격 모션 중에 Play()를 부른다
// 위치, 시간은 연출 조정 대상이라 전부 인스펙터에서 바꾼다
public class KTH_ParryTelegraph : MonoBehaviour
{
    [SerializeField] private SpriteRenderer target;         // 비워두면 자기 SpriteRenderer
    [SerializeField] private Color color = new(1f, 0.85f, 0.1f);
    [SerializeField] private float flashTime = 0.08f;       // 한 번 노랗게 됐다 돌아오는 시간의 절반
    [SerializeField] private int flashCount = 2;

    [SerializeField] private GameObject effectPrefab;      // 비워두면 색만 바뀜
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
