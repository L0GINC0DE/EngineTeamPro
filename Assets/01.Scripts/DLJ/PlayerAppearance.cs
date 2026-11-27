using System.Collections;
using UnityEngine;
using UnityEngine.U2D.Animation;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteLibrary), typeof(SpriteRenderer))]
public class PlayerAppearance : MonoBehaviour
{
    private const float WeaponAttackDelay = 0.3f;
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
    private Coroutine pendingWeaponAttack;
    private Pose? weaponRestPose;

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

        if (weaponRestPose.HasValue && (weaponAnimator == null || !weaponAnimator.isActiveAndEnabled ||
            weaponAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash == ReadyHash))
            StopWeaponAttack();
    }

    private void OnDisable()
    {
        CancelPendingWeaponAttack();
        StopBodyAttack();
        StopWeaponAttack();
    }

    public void ShowBody(UIType type)
    {
        Sprite sprite = spriteLibrary.GetSprite("Body", type.ToString());
        if (sprite == null || bodyRenderer == null) return;

        selectedBody = type;
        if (!bodyAttacking && !PlayState(bodyAnimator, Animator.StringToHash($"Base Layer.{type}")))
            ShowStaticSprite(bodyRenderer, bodyAnimator, sprite);
    }

    public void ShowWeapon(UIType type)
    {
        Sprite sprite = spriteLibrary.GetSprite("Weapon", type.ToString());
        if (sprite == null || weaponRenderer == null) return;

        RestoreWeaponPose();
        selectedWeapon = type;
        selectedWeaponSprite = sprite;

        Vector2 offset = type switch
        {
            UIType.Attack => attackOffset,
            UIType.Defend => defendOffset,
            UIType.Act => actOffset,
            UIType.Heal => healOffset,
            UIType.HealthBar => healthbarOffset,
            UIType.Setting => settingOffset,
            UIType.TextBar => textbarOffset,
            _ => Vector2.zero
        };
        weaponRenderer.transform.localPosition += (Vector3)offset;
        ShowSelectedWeapon();
    }

    public void HideSelection()
    {
        selectedBody = null;
        if (!bodyAttacking) ShowStaticSprite(bodyRenderer, bodyAnimator, null);

        selectedWeapon = null;
        selectedWeaponSprite = null;
        if (!weaponRestPose.HasValue) ShowSelectedWeapon();
    }

    public void PlayAttack(UIType? weaponType)
    {
        if (!isActiveAndEnabled) return;

        CancelPendingWeaponAttack();
        bool startedBody = PlayState(bodyAnimator, CapeFlareHash);
        if (startedBody) bodyAttacking = true;

        if (weaponType is not UIType type) return;
        if (startedBody)
            pendingWeaponAttack = StartCoroutine(PlayWeaponAttackAfterDelay(type));
        else
            PlayWeaponAttack(type);
    }

    private IEnumerator PlayWeaponAttackAfterDelay(UIType type)
    {
        yield return new WaitForSeconds(WeaponAttackDelay);
        pendingWeaponAttack = null;
        PlayWeaponAttack(type);
    }

    private void CancelPendingWeaponAttack()
    {
        if (pendingWeaponAttack == null) return;
        StopCoroutine(pendingWeaponAttack);
        pendingWeaponAttack = null;
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

    private void PlayWeaponAttack(UIType type)
    {
        if (!isActiveAndEnabled || weaponRenderer == null) return;

        RestoreWeaponPose();
        Transform weapon = weaponRenderer.transform;
        var restPose = new Pose(weapon.localPosition, weapon.localRotation);
        if (!PlayState(weaponAnimator, Animator.StringToHash($"Base Layer.Attack_{type}"))) return;

        weaponRestPose = restPose;
        if (type == UIType.Attack)
            weapon.localPosition += (Vector3)swordAttackOffset;
    }

    private void StopWeaponAttack()
    {
        if (RestoreWeaponPose()) ShowSelectedWeapon();
    }

    private bool RestoreWeaponPose()
    {
        if (weaponRestPose is not Pose restPose) return false;
        weaponRestPose = null;
        if (weaponRenderer != null)
            weaponRenderer.transform.SetLocalPositionAndRotation(restPose.position, restPose.rotation);
        return true;
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
