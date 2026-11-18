using UnityEngine;
using UnityEngine.U2D.Animation;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteLibrary), typeof(SpriteRenderer))]
public class PlayerAppearance : MonoBehaviour
{
    [Header("Body")]
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private string bodyCategory = "Body";
    
    [Header("Weapon")]
    [SerializeField] private SpriteRenderer weaponRenderer;
    [SerializeField] private string weaponCategory = "Weapon";
    
    private SpriteLibrary spriteLibrary;

    public void HideSelection()
    {
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

        bodyRenderer.sprite = sprite;
    }
    
    public void ShowWeapon(UIType type)
    {
        if (spriteLibrary == null) spriteLibrary = GetComponent<SpriteLibrary>();
        if (weaponRenderer == null) weaponRenderer = GetComponent<SpriteRenderer>();
        if (spriteLibrary == null || weaponRenderer == null) return;

        Sprite sprite = spriteLibrary.GetSprite(weaponCategory, type.ToString());
        if (sprite == null) return;

        weaponRenderer.sprite = sprite;
    }
}
