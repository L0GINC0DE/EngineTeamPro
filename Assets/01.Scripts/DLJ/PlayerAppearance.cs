using UnityEngine;
using UnityEngine.U2D.Animation;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteLibrary), typeof(SpriteRenderer))]
public class PlayerAppearance : MonoBehaviour
{
    [Header("Body")]
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private string bodyCategory = "Body";

    [Header("Body idle")]
    [SerializeField] private Sprite[] bodyIdleFrames;
    [SerializeField, Min(1f)] private float bodyIdleFramesPerSecond = 6.25f;
    
    [Header("Weapon")]
    [SerializeField] private SpriteRenderer weaponRenderer;
    [SerializeField] private string weaponCategory = "Weapon";

    [Header("Sword idle")]
    [SerializeField] private Sprite[] swordIdleFrames;
    [SerializeField, Min(1f)] private float swordIdleFramesPerSecond = 30f;
    
    private SpriteLibrary spriteLibrary;
    private int bodyIdleStart = -1;
    private int bodyIdleCount;
    private float bodyIdleTime;
    private Sprite displayedBodyFrame;
    private bool playingSwordIdle;
    private float swordIdleTime;
    private Sprite displayedSwordFrame;

    private void LateUpdate()
    {
        AdvanceBodyIdle(Time.deltaTime);
        AdvanceWeaponIdle(Time.deltaTime);
    }

    private void AdvanceBodyIdle(float deltaTime)
    {
        if (bodyIdleStart < 0) return;
        if (bodyRenderer == null || bodyRenderer.sprite != displayedBodyFrame
            || bodyIdleFrames == null || bodyIdleStart + bodyIdleCount > bodyIdleFrames.Length)
        {
            bodyIdleStart = -1;
            return;
        }

        float fps = Mathf.Max(1f, bodyIdleFramesPerSecond);
        bodyIdleTime = Mathf.Repeat(bodyIdleTime + deltaTime, bodyIdleCount / fps);
        int index = Mathf.Min(Mathf.FloorToInt(bodyIdleTime * fps), bodyIdleCount - 1);
        displayedBodyFrame = bodyIdleFrames[bodyIdleStart + index];
        bodyRenderer.sprite = displayedBodyFrame;
    }

    private void AdvanceWeaponIdle(float deltaTime)
    {
        if (!playingSwordIdle) return;
        if (weaponRenderer == null || weaponRenderer.sprite != displayedSwordFrame
            || swordIdleFrames == null || swordIdleFrames.Length == 0)
        {
            playingSwordIdle = false;
            return;
        }

        float fps = Mathf.Max(1f, swordIdleFramesPerSecond);
        swordIdleTime = Mathf.Repeat(swordIdleTime + deltaTime, swordIdleFrames.Length / fps);
        int index = Mathf.Min(Mathf.FloorToInt(swordIdleTime * fps), swordIdleFrames.Length - 1);
        displayedSwordFrame = swordIdleFrames[index];
        weaponRenderer.sprite = displayedSwordFrame;
    }

    public void HideSelection()
    {
        bodyIdleStart = -1;
        bodyIdleTime = 0f;
        playingSwordIdle = false;
        swordIdleTime = 0f;

        if (bodyRenderer != null)
        {
            bodyRenderer.sprite = null;
        }

        if (weaponRenderer != null)
        {
            weaponRenderer.sprite = null;
        }
    }

    public void ShowBody(UIType type)
    {
        if (spriteLibrary == null) spriteLibrary = GetComponent<SpriteLibrary>();
        if (bodyRenderer == null) bodyRenderer = GetComponent<SpriteRenderer>();
        if (spriteLibrary == null || bodyRenderer == null) return;

        Sprite sprite = spriteLibrary.GetSprite(bodyCategory, type.ToString());
        if (sprite == null) return;

        bodyIdleStart = -1;
        bodyIdleCount = 0;
        bodyIdleTime = 0f;
        if (bodyIdleFrames != null)
        {
            for (int i = 0; i < bodyIdleFrames.Length; i++)
            {
                if (bodyIdleFrames[i] != sprite) continue;
                bodyIdleStart = i;
                while (i + bodyIdleCount < bodyIdleFrames.Length
                    && bodyIdleFrames[i + bodyIdleCount] != null
                    && bodyIdleFrames[i + bodyIdleCount].texture == sprite.texture)
                    bodyIdleCount++;
                break;
            }
        }
        displayedBodyFrame = sprite;
        bodyRenderer.sprite = sprite;
    }
    
    public void ShowWeapon(UIType type)
    {
        if (spriteLibrary == null) spriteLibrary = GetComponent<SpriteLibrary>();
        if (weaponRenderer == null) weaponRenderer = GetComponent<SpriteRenderer>();
        if (spriteLibrary == null || weaponRenderer == null) return;

        Sprite sprite = spriteLibrary.GetSprite(weaponCategory, type.ToString());
        if (sprite == null) return;

        playingSwordIdle = type == UIType.Attack && swordIdleFrames != null
            && swordIdleFrames.Length > 0 && swordIdleFrames[0] != null;
        swordIdleTime = 0f;
        displayedSwordFrame = playingSwordIdle ? swordIdleFrames[0] : sprite;
        weaponRenderer.sprite = displayedSwordFrame;
    }
}
