using UnityEngine;

public class KTH_GhostRenderer : MonoBehaviour
{
    public SpriteRenderer ghostSpriteRenderer { get; private set; }
    private float _lifetime;
    private float _fadeDuration;
    private Color _startColor;
    void Awake()
    {
        ghostSpriteRenderer = GetComponent<SpriteRenderer>();
        if (ghostSpriteRenderer == null)
        {
            ghostSpriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
    }

    public void Initialize(Vector3 worldPosition, SpriteRenderer parentSpriteRenderer, Color color, float lifetime)
    {
        transform.position = worldPosition;
        ghostSpriteRenderer.sprite = parentSpriteRenderer.sprite;
        ghostSpriteRenderer.sortingLayerID = parentSpriteRenderer.sortingLayerID;
        ghostSpriteRenderer.sortingOrder = parentSpriteRenderer.sortingOrder;
        ghostSpriteRenderer.flipX = parentSpriteRenderer.flipX;
        ghostSpriteRenderer.flipY = parentSpriteRenderer.flipY;
        ghostSpriteRenderer.color = color;
        _lifetime = lifetime;
        _startColor = color;
        _fadeDuration = lifetime;
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (gameObject.activeSelf)
        {
            _fadeDuration -= Time.deltaTime;
            float alpha = Mathf.Clamp01(_fadeDuration / _lifetime);
            ghostSpriteRenderer.color = new Color(_startColor.r, _startColor.g, _startColor.b, alpha);

            if (_fadeDuration <= 0)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
