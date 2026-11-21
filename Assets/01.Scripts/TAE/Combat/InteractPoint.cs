using _01.Scripts.TAE.Managers;
using UnityEngine;

namespace _01.Scripts.TAE.Combat
{
    public enum InteractPointType
    {
        None = -1,
        AttackUI,
        SettingUI,
        HealthBarUI
    }
    
    public class InteractPoint : MonoBehaviour
    {
        [SerializeField] private InteractPointType type;
        public InteractPointType PointType => type;
        
        private SpriteRenderer _spriteRenderer;
        private Sprite _originalSprite;
        private Color _originalColor;

        public void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _originalSprite = _spriteRenderer.sprite;
            _originalColor = _spriteRenderer.color;
        }
        public void SetSelected(bool selected)
        {
            _spriteRenderer.color = selected ? Color.yellow : _originalColor;
        }
        
        public void ResetPoint()
        {
            type = InteractPointType.None;
            _spriteRenderer.sprite = _originalSprite;
            SetSelected(false);
        }

        public void ApplyGimmick(InteractPointType selectedType, Sprite sprite)
        {
            if (selectedType == InteractPointType.None)
            {
                ResetPoint();
                return;
            }
            
            type = selectedType;
            _spriteRenderer.sprite = sprite;
        }
    }
}