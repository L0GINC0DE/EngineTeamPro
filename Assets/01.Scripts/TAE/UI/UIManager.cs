using System;
using _01.Scripts.KTH;
using _01.Scripts.TAE.Agents;
using _01.Scripts.TAE.Combat;
using _01.Scripts.TAE.Data;
using _01.Scripts.TAE.Managers;
using CoreLib;
using Systems;
using UnityEngine;
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
            slotGroup.bodySlot.gameObject.SetActive(true);
            slotGroup.bodySlot.FocusFirstBtn();
        }
        public void OpenWeaponList()
        {
            selectUI.SetActive(false);
            slotGroup.weaponSlot.gameObject.SetActive(true);
            slotGroup.weaponSlot.FocusFirstBtn();
        }

        public void CloseClothUI()
        {
            clothUI.gameObject.SetActive(false);
            slotGroup.bodySlot.gameObject.SetActive(false);
            slotGroup.weaponSlot.gameObject.SetActive(false);
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
            slotGroup.weaponSlot.Initialize(holder.WeaponDataList, data =>
            {
                if (holder.TryChangeWeapon(data)) CloseClothUI();
                selectGroup.weaponSelectUI.SetUp(data);
            });
            
            slotGroup.bodySlot.Initialize(render.UIList, data =>
            {
                render.Change(data);
                _p.ApplyStats(data);
                selectGroup.bodySelectUI.SetUp(data);
                CloseClothUI();
            });
        }
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