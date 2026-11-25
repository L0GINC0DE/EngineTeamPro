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
        [Serializable]
        private struct GimmickOption
        {
            public UIDataSO card;
            public GimmickBase gimmick;
        }

        [SerializeField] private SpriteRenderer idleMarker;
        [SerializeField] private UseBarUI useBar;
        [SerializeField] private Color selectedColor = Color.yellow;
        [SerializeField] private GimmickOption[] options;

        private GimmickBase activeGimmick;
        private Coroutine useRoutine;
        private Color idleColor;

        public UIDataSO UIData { get; private set; }
        public NotifyValue<float> UseTimer = new(0f);
        public event Action<bool> OnReloadEvent;

        private void Awake()
        {
            idleColor = idleMarker.color;
            SetTimerVisible(false);
        }

        public void SetSelected(bool selected)
        {
            idleMarker.color = selected ? selectedColor : idleColor;
        }

        public bool TryApplyGimmick(UIDataSO data, bool launchRight)
        {
            if (data == null || options == null)
                return false;

            foreach (var option in options)
            {
                if (option.card != data || option.gimmick == null)
                    continue;

                ResetPoint();
                UIData = data;
                activeGimmick = option.gimmick;
                activeGimmick.SetDirection(launchRight);
                idleMarker.enabled = false;
                activeGimmick.gameObject.SetActive(true);
                useRoutine = StartCoroutine(UseForDuration(data.duration));
                return true;
            }

            return false;
        }

        public void ResetPoint()
        {
            if (useRoutine != null)
            {
                StopCoroutine(useRoutine);
                useRoutine = null;
            }

            if (activeGimmick != null)
            {
                activeGimmick.gameObject.SetActive(false);
                activeGimmick = null;
            }

            UIData = null;
            idleMarker.enabled = true;
            SetSelected(false);
            UseTimer.Value = 0f;
            SetTimerVisible(false);
        }

        private void SetTimerVisible(bool visible)
        {
            if (useBar != null)
                useBar.gameObject.SetActive(visible);

            OnReloadEvent?.Invoke(visible);
        }

        private IEnumerator UseForDuration(float duration)
        {
            if (useBar != null)
                useBar.SetNormalizeValue(1f);
            SetTimerVisible(true);

            while (UseTimer.Value < duration)
            {
                yield return null;
                UseTimer.Value = Mathf.Min(UseTimer.Value + Time.deltaTime, duration);
                if (useBar != null)
                    useBar.SetNormalizeValue(1f - UseTimer.Value / duration);
            }

            useRoutine = null;
            ResetPoint();
        }
    }
}
