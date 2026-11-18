using UnityEngine;

public class KTH_GhostRenderer : MonoBehaviour
{
    public SpriteRenderer ghostSpriteRenderer { get; private set; }
    private float _lifetime;
    private float _fadeDuration;
    private Color _startColor;

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
        float alpha = _startColor.a * Mathf.Clamp01(_fadeDuration / _lifetime);
        ghostSpriteRenderer.color = new Color(_startColor.r, _startColor.g, _startColor.b, alpha);

        if (_fadeDuration <= 0)
        {
            gameObject.SetActive(false);
        }
    }

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
