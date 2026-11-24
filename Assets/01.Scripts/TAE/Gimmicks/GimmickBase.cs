using _01.Scripts.TAE.Players;
using UnityEngine;

namespace _01.Scripts.TAE
{
    public abstract class GimmickBase : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float flashDuration = 0.18f;
        [SerializeField] private Color flashTint = new(0.35f, 1f, 0.92f);

        private SpriteRenderer spriteRenderer;
        private Color originalColor;
        private float flashEndsAt;

        public abstract void Activate(PlatformPlayer player);
        public virtual void SetDirection(bool right) { }

        protected void Awake()
        {
            if (TryGetComponent(out spriteRenderer))
                originalColor = spriteRenderer.color;
        }

        protected void OnEnable() => Flash();

        protected void LateUpdate()
        {
            if (spriteRenderer == null || flashDuration <= 0f)
                return;

            float strength = Mathf.Clamp01((flashEndsAt - Time.time) / flashDuration) * 0.65f;
            Color color = Color.Lerp(originalColor, flashTint, strength);
            color.a = originalColor.a;
            spriteRenderer.color = color;
        }

        protected void OnTriggerEnter2D(Collider2D other)
        {
            if (!isActiveAndEnabled)
                return;

            var player = other.GetComponentInParent<PlatformPlayer>();
            if (player == null || !player.isActiveAndEnabled || player.HealthCompo.IsDead)
                return;

            Activate(player);
            Flash();
        }

        private void Flash() => flashEndsAt = Time.time + flashDuration;
    }
}
