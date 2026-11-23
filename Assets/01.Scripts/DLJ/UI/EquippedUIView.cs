using DG.Tweening;
using UnityEngine;

[DefaultExecutionOrder(200)]
public sealed class EquippedUIView : MonoBehaviour
{
    [SerializeField] private Transform worldAnchor;

    private RectTransform slot;
    private Canvas canvas;
    private CanvasGroup input;
    private PlayableUI source;
    private RectTransform button;
    private Transform originalParent;
    private int originalIndex;
    private Vector2 originalAnchorMin, originalAnchorMax, originalSize;
    private Vector3 originalPosition, originalScale;
    private bool originalActive;

    private void Awake()
    {
        slot = (RectTransform)transform;
        canvas = GetComponentInParent<Canvas>();
        input = GetComponent<CanvasGroup>();
    }

    public bool Attach(PlayableUI ui)
    {
        if (ui == null || ui.transform is not RectTransform rect) return false;

        Restore();
        rect.DOKill(true);
        source = ui;
        button = rect;
        originalParent = rect.parent;
        originalIndex = rect.GetSiblingIndex();
        originalAnchorMin = rect.anchorMin;
        originalAnchorMax = rect.anchorMax;
        originalSize = rect.sizeDelta;
        originalPosition = rect.anchoredPosition3D;
        originalScale = rect.localScale;
        originalActive = rect.gameObject.activeSelf;

        Vector2 size = rect.rect.size;
        float scale = slot.rect.width / Mathf.Max(1f, size.x);
        rect.SetParent(slot, false);
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
        rect.sizeDelta = size;
        rect.localScale = new Vector3(scale, scale, 1f);
        rect.anchoredPosition3D = -rect.rect.center * scale;
        rect.gameObject.SetActive(true);
        LateUpdate();
        return true;
    }

    private void LateUpdate()
    {
        if (source == null) return;
        input.interactable = input.blocksRaycasts = source.CanEquip;
        if (worldAnchor == null || canvas == null) return;

        Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Camera worldCamera = uiCamera != null ? uiCamera : Camera.main;
        if (worldCamera == null) return;

        Vector2 screenPoint = worldCamera.WorldToScreenPoint(worldAnchor.position);
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
            (RectTransform)slot.parent, screenPoint, uiCamera, out Vector3 point))
            slot.position = point;
    }

    public void Restore()
    {
        source = null;
        if (button == null) return;

        button.SetParent(originalParent, false);
        button.SetSiblingIndex(originalIndex);
        button.anchorMin = originalAnchorMin;
        button.anchorMax = originalAnchorMax;
        button.sizeDelta = originalSize;
        button.anchoredPosition3D = originalPosition;
        button.localScale = originalScale;
        button.gameObject.SetActive(originalActive);
        button = null;
    }
}
