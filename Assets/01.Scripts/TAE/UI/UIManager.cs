using System;
using System.Collections.Generic;
using _01.Scripts.KTH;
using _01.Scripts.TAE.Agents;
using _01.Scripts.TAE.Combat;
using _01.Scripts.TAE.Data;
using _01.Scripts.TAE.Managers;
using CoreLib;
using Systems;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace _01.Scripts.TAE.UI
{
    public class UIManager : MonoSingleton<UIManager>
    {
        public Transform holderLayer;
        public DamageTMP damagePrefab;

        public ClothUI clothUI;
        public GameObject selectUI;

        public SelectGroup selectGroup;
        public SlotGroup slotGroup;

        private PlatformPlayer _p;

        [SerializeField] private Button bodyBtn;
        
        private void Update()
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
                OnEscape();
        }
        
        public void ShowDamageTMP(Transform trm, int damage)
        {
            var damageFont = PoolManager.Instance.Pop(damagePrefab.Item.ItemName) as DamageTMP;
            damageFont.Initialize(holderLayer, trm.position, damage.ToString(), Color.yellow);
        }

        public void ShowClothUI()
        {
            GameManager.Instance.TimeStop(true);
            clothUI.gameObject.SetActive(true);
            selectUI.SetActive(true);
            bodyBtn.Select();
            slotGroup.weaponSlot.gameObject.SetActive(false);
            slotGroup.bodySlot.gameObject.SetActive(false);
        }
        public void OpenBodyList()
        {
            selectUI.SetActive(false);
            clothUI.Push(slotGroup.bodySlot.gameObject);
            slotGroup.bodySlot.FocusFirstBtn();
        }
        public void OpenWeaponList()
        {
            selectUI.SetActive(false);
            clothUI.Push(slotGroup.weaponSlot.gameObject);
            slotGroup.weaponSlot.FocusFirstBtn();
        }

        public void CloseClothUI()
        {
            clothUI.gameObject.SetActive(false);
            clothUI.Push(slotGroup.weaponSlot.gameObject);
            GameManager.Instance.TimeStop(false);
        }

        public void Initialize(PlatformPlayer player)
        {
            _p = player;
            Initialize(_p.WeaponCompo, _p.RendererCompo);

            selectGroup.weaponSelectUI.SetUp(_p.WeaponCompo.CurrentWeapon.Value.UIData);
            selectGroup.bodySelectUI.SetUp(_p.RendererCompo.UIData);
        }

        private void Initialize(WeaponHolder holder, AgentRenderer render)
        {
            void RefreshSelection()
            {
                selectGroup.bodySelectUI.SetUp(render.UIData);
                selectGroup.weaponSelectUI.SetUp(holder.CurrentWeapon.Value.UIData);
            }
            
            slotGroup.weaponSlot.Initialize(holder.WeaponDataList, data =>
            {
                var prevWeaponData = holder.CurrentWeapon.Value.UIData;
                bool swap = data == render.UIData;
                if (!holder.TryChangeWeapon(data)) return;
                
                if (swap)
                {
                    render.Change(prevWeaponData);
                    _p.ApplyStats(prevWeaponData);
                }
                
                RefreshSelection();
                OnEscape();
            });
            
            slotGroup.bodySlot.Initialize(render.UIList, data =>
            {
                bool swap = data == holder.CurrentWeapon.Value.UIData;

                if (swap && !holder.TryChangeWeapon(render.UIData))
                    return;

                render.Change(data);
                _p.ApplyStats(data);

                RefreshSelection();
                OnEscape();
            });
        }

        #region Escape

        private GameObject previousFocus;

        public void OnEscape()
        {
            if (!clothUI.gameObject.activeInHierarchy) return;

            if (clothUI.panels.Count == 0)
            {
                CloseClothUI();
                return;
            }

            clothUI.Pop();
            selectUI.SetActive(true);
            bodyBtn.Select();
        }

        #endregion
    }   

    [Serializable]
    public struct SlotGroup
    {
        public Slot weaponSlot;
        public Slot bodySlot;
    }
    [Serializable]
    public struct SelectGroup
    {
        public SelectUI weaponSelectUI;
        public SelectUI bodySelectUI;
    }
}