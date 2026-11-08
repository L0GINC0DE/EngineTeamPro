using UnityEngine;

public class KTH_GhostRenderer : MonoBehaviour
{
    public SpriteRenderer ghostSpriteRenderer { get; private set; }
    private float _lifetime;
    private float _fadeDuration;
    private Color _startColor;

    // 부모(보스)가 움직여도 잔상은 생성된 월드 위치에 남아 있어야 한다
    private Vector3 _worldPosition;
    private Quaternion _worldRotation;
    private Vector3 _worldScale;

    void Awake()
    {
        ghostSpriteRenderer = GetComponent<SpriteRenderer>();
        if (ghostSpriteRenderer == null)
        {
            ghostSpriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
    }

    public void Initialize(Vector3 worldPosition, Transform source, SpriteRenderer parentSpriteRenderer, Color color, float lifetime)
    {
        _worldPosition = worldPosition;
        _worldRotation = source.rotation;
        _worldScale = source.lossyScale;
        ApplyWorldTransform();
        ghostSpriteRenderer.sprite = parentSpriteRenderer.sprite;
        ghostSpriteRenderer.sortingLayerID = parentSpriteRenderer.sortingLayerID;
        // 본체와 같은 order면 겹칠 때 그리기 순서가 뒤바뀌며 깜빡이므로 한 칸 뒤로
        ghostSpriteRenderer.sortingOrder = parentSpriteRenderer.sortingOrder - 1;
        ghostSpriteRenderer.flipX = parentSpriteRenderer.flipX;
        ghostSpriteRenderer.flipY = parentSpriteRenderer.flipY;
        ghostSpriteRenderer.color = color;
        _lifetime = Mathf.Max(lifetime, 0.0001f);
        _startColor = color;
        _fadeDuration = _lifetime;
        gameObject.SetActive(true);
    }

    void Update()
    {
        _fadeDuration -= Time.deltaTime;
        // 시작 알파(_startColor.a)에서 0으로 줄어들게 한다
        float alpha = _startColor.a * Mathf.Clamp01(_fadeDuration / _lifetime);
        ghostSpriteRenderer.color = new Color(_startColor.r, _startColor.g, _startColor.b, alpha);

        if (_fadeDuration <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    // 보스는 Update(DOTween 등)에서 움직이므로 그 뒤에 월드 위치로 되돌린다
    void LateUpdate()
    {
        ApplyWorldTransform();
    }

    private void ApplyWorldTransform()
    {
        transform.SetPositionAndRotation(_worldPosition, _worldRotation);

        Transform parent = transform.parent;
        if (parent == null)
        {
            transform.localScale = _worldScale;
            return;
        }
        Vector3 parentScale = parent.lossyScale;
        transform.localScale = new Vector3(
            parentScale.x != 0f ? _worldScale.x / parentScale.x : 0f,
            parentScale.y != 0f ? _worldScale.y / parentScale.y : 0f,
            parentScale.z != 0f ? _worldScale.z / parentScale.z : 0f
        );
    }
}
