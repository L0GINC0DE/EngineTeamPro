using UnityEngine;
using UnityEngine.U2D.Animation;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteLibrary), typeof(SpriteRenderer))]
public class PlayerAppearance : MonoBehaviour
{
    private static readonly int ReadyHash = Animator.StringToHash("Base Layer.Ready");
    private static readonly int CapeFlareHash = Animator.StringToHash("Base Layer.CapeFlare");
    private static readonly int SwordIdleHash = Animator.StringToHash("Base Layer.SwordIdle");

    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private SpriteRenderer weaponRenderer;
    [SerializeField] private Vector2 swordAttackOffset;
    
    [SerializeField] private Vector2 attackOffset;
    [SerializeField] private Vector2 defendOffset;
    [SerializeField] private Vector2 actOffset;
    [SerializeField] private Vector2 healOffset;
    [SerializeField] private Vector2 healthbarOffset;
    [SerializeField] private Vector2 settingOffset;
    [SerializeField] private Vector2 textbarOffset;

    private SpriteLibrary spriteLibrary;
    private Animator bodyAnimator;
    private Animator weaponAnimator;
    private UIType? selectedBody;
    private bool bodyAttacking;
    private UIType? selectedWeapon;
    private Sprite selectedWeaponSprite;
    private bool attacking;
    private Vector3 weaponRestPosition;
    private Quaternion weaponRestRotation;

    private void Awake()
    {
        spriteLibrary = GetComponent<SpriteLibrary>();
        if (bodyRenderer == null) bodyRenderer = GetComponent<SpriteRenderer>();

        bodyAnimator = bodyRenderer.GetComponent<Animator>();
        if (weaponRenderer != null)
            weaponAnimator = weaponRenderer.GetComponent<Animator>();
    }

    private void LateUpdate()
    {
        if (bodyAttacking && (bodyAnimator == null || !bodyAnimator.isActiveAndEnabled ||
            bodyAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash != CapeFlareHash))
            StopBodyAttack();

        if (attacking && (weaponAnimator == null || !weaponAnimator.isActiveAndEnabled ||
            weaponAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash == ReadyHash))
            StopWeaponAttack();
    }

    private void OnDisable()
    {
        StopBodyAttack();
        StopWeaponAttack();
    }

    public void ShowBody(UIType type)
    {
        Sprite sprite = spriteLibrary.GetSprite("Body", type.ToString());
        if (sprite == null || bodyRenderer == null) return;

        selectedBody = type;
        if (bodyAttacking) return;

        if (!PlayState(bodyAnimator, Animator.StringToHash($"Base Layer.{type}")))
            ShowStaticSprite(bodyRenderer, bodyAnimator, sprite);
    }

    public void ShowWeapon(UIType type)
    {
        Sprite sprite = spriteLibrary.GetSprite("Weapon", type.ToString());
        if (sprite == null || weaponRenderer == null) return;

        RestoreWeaponPose();
        selectedWeapon = type;
        selectedWeaponSprite = sprite;
        
        Transform weapon = weaponRenderer.transform;
        weaponRestPosition = weapon.localPosition;
        weaponRestRotation = weapon.localRotation;
        
        if (type == UIType.Attack)
            weapon.localPosition += (Vector3)attackOffset;
        if (type == UIType.Defend)
            weapon.localPosition += (Vector3)defendOffset;
        if (type == UIType.Act)
            weapon.localPosition += (Vector3)actOffset;
        if (type == UIType.Heal)
            weapon.localPosition += (Vector3)healOffset;
        if (type == UIType.HealthBar)
            weapon.localPosition += (Vector3)healthbarOffset;
        if (type == UIType.Setting)
            weapon.localPosition += (Vector3)settingOffset;
        if (type == UIType.TextBar)
            weapon.localPosition += (Vector3)textbarOffset;
        
        ShowSelectedWeapon();
    }

    public void HideSelection()
    {
        selectedBody = null;
        if (!bodyAttacking) ShowStaticSprite(bodyRenderer, bodyAnimator, null);

        selectedWeapon = null;
        selectedWeaponSprite = null;
        if (!attacking) ShowSelectedWeapon();
    }

    public void PlayBodyAttack()
    {
        if (isActiveAndEnabled && PlayState(bodyAnimator, CapeFlareHash))
            bodyAttacking = true;
    }

    private void StopBodyAttack()
    {
        if (!bodyAttacking) return;
        bodyAttacking = false;
        if (selectedBody is UIType type)
            ShowBody(type);
        else
            ShowStaticSprite(bodyRenderer, bodyAnimator, null);
    }

    public void PlayWeaponAttack(UIType type)
    {
        int attackHash = Animator.StringToHash($"Base Layer.Attack_{type}");
        if (!isActiveAndEnabled || weaponRenderer == null) return;

        RestoreWeaponPose();
        Transform weapon = weaponRenderer.transform;
        weaponRestPosition = weapon.localPosition;
        weaponRestRotation = weapon.localRotation;
        if (!PlayState(weaponAnimator, attackHash)) return;

        if (type == UIType.Attack)
            weapon.localPosition += (Vector3)swordAttackOffset;

        attacking = true;
    }

    private void StopWeaponAttack()
    {
        if (!attacking) return;

        RestoreWeaponPose();
        ShowSelectedWeapon();
    }

    private void RestoreWeaponPose()
    {
        if (!attacking) return;

        attacking = false;
        if (weaponRenderer != null)
            weaponRenderer.transform.SetLocalPositionAndRotation(weaponRestPosition, weaponRestRotation);
    }

    private void ShowSelectedWeapon()
    {
        if (selectedWeapon != UIType.Attack || !PlayState(weaponAnimator, SwordIdleHash))
            ShowStaticSprite(weaponRenderer, weaponAnimator, selectedWeaponSprite);
    }

    private static void ShowStaticSprite(SpriteRenderer renderer, Animator animator, Sprite sprite)
    {
        if (animator != null) animator.enabled = false;
        if (renderer != null) renderer.sprite = sprite;
    }

    private static bool PlayState(Animator animator, int state)
    {
        if (animator == null || !animator.gameObject.activeInHierarchy || animator.runtimeAnimatorController == null)
            return false;

        bool wasEnabled = animator.enabled;
        animator.enabled = true;
        if (!animator.HasState(0, state))
        {
            animator.enabled = wasEnabled;
            return false;
        }

        animator.Play(state, 0, 0f);
        animator.Update(0f);
        return true;
    }
}
