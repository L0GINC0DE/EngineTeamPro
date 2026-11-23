using System;
using System.Collections;
using _01.Scripts.TAE.Agents;
using _01.Scripts.TAE.CoreLib;
using _01.Scripts.TAE.Data;
using UnityEngine;

namespace _01.Scripts.TAE.Combat
{
    public class InteractPoint : MonoBehaviour
    {
        public event Action<bool> OnReloadEvent;
        
        private UseBarUI useBar;
        
        public UIDataSO UIData { get; private set; }
        
        private SpriteRenderer _spriteRenderer;
        private Sprite _originalSprite;
        private Color _originalColor;

        public NotifyValue<float> UseTimer = new(0f);


        public void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            useBar = GetComponentInChildren<UseBarUI>();
            
            _originalSprite = _spriteRenderer.sprite;
            _originalColor = _spriteRenderer.color;
            
            OnReloadEvent += HandleUse;
            OnReloadEvent?.Invoke(false);

            UseTimer.OnValueChanged += HandleUseTimerChange;
        }

        private void OnEnable()
        {
            OnReloadEvent += HandleUse;
            UseTimer.OnValueChanged += HandleUseTimerChange;
        }

        private void OnDisable()
        {
            OnReloadEvent -= HandleUse;
            UseTimer.OnValueChanged -= HandleUseTimerChange;
        }

        public void SetSelected(bool selected)
        {
            _spriteRenderer.color = selected ? Color.yellow : _originalColor;
        }
        
        public void ResetPoint()
        {
            OnReloadEvent?.Invoke(false);
            _spriteRenderer.sprite = _originalSprite;
            SetSelected(false);
            StopAllCoroutines();
        }

        private void Use()
        {
            StartCoroutine(UseCoroutine());
        }

        private void HandleUse(bool isActive)
        {
            useBar.gameObject.SetActive(isActive);
        }
        
        private IEnumerator UseCoroutine()
        {
            UseTimer.Value = 0f;
            OnReloadEvent?.Invoke(true);

            while (UseTimer.Value < UIData.duration)
            {
                UseTimer.Value = Mathf.Min(UseTimer.Value + Time.deltaTime, UIData.duration);
                yield return null;
            }

            OnReloadEvent?.Invoke(false);
            ResetPoint();
        }

        private void HandleUseTimerChange(float prev, float next)
        {
            useBar.SetNormalizeValue(next / UIData.duration);
        }
        
        public void SetUp(UIDataSO data)
        {
            if (data == null || data.uiSprite == null)
                return;

            UIData = data;
            _spriteRenderer.sprite = data.uiSprite;
            Use();
        }
    }
}
