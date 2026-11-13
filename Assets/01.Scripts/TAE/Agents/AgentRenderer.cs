using System;
using System.Collections.Generic;
using System.Linq;
using _01.Scripts.TAE.Data;
using CoreLib;
using UnityEngine;
using UnityEngine.Events;

namespace _01.Scripts.TAE.Agents
{
    public class AgentRenderer : MonoBehaviour
    {
        public UnityEvent<UIDataSO> OnUIBodyChanged;
        public SpriteRenderer _spriteRenderer;

        [field: SerializeField] public UIDataSO UIData { get; private set; }

        [SerializeField] private UIDataListSO uiList;
        public IReadOnlyList<UIDataSO> UIList => uiList.uiDataList;

        private void Start()
        {
            
        }

        public void FlipController(float value)
        {
            if (value != 0)
                transform.root.eulerAngles = value > 0f ? Vector3.zero : new Vector3(0, 180, 0);
        }
        
        public void Init()
        {
            if (UIList.Count == 0) return;
            UIData = UIList.First();
            SetUp();
        }
        
        private void OnValidate()
        {
            SetUp();
        }

        private void SetUp()
        {
            if (_spriteRenderer == null) return;
            Debug.Log("변경");
            _spriteRenderer.sprite = UIData.uiSprite;
            OnUIBodyChanged?.Invoke(UIData);
        }

        public void Change(UIDataSO data)
        {
            UIData = data;
            SetUp();
        }
    }
}