using System;
using _01.Scripts.TAE.Combat;
using _01.Scripts.TAE.Data;
using CoreLib;
using Systems;
using UnityEngine;

namespace _01.Scripts.TAE.UI
{
    public class UIManager : MonoSingleton<UIManager>
    {
        public Transform holderLayer;
        public DamageTMP damagePrefab;

        public ClothUI clothUI;

        public WeaponSelectUI weaponSelectUI;
        public BodySelectUI bodySelectUI;
        
        public WeaponSlot weaponSlotUI;
        public BodySlot bodySlotUI;

        public void ShowDamageTMP(Transform trm, int damage)
        {
            var damageFont = PoolManager.Instance.Pop(damagePrefab.Item.ItemName) as DamageTMP;
            damageFont.Initialize(holderLayer, trm.position, damage.ToString(), Color.yellow);
        }

        public void Set(UIWeapon weapon)
        {
            weaponSelectUI.SetUp(weapon.UIData);
        }

        public void SetBody(UIDataSO data)
        {
            bodySelectUI.SetUp(data);
        }

        public void ShowClothUI()
        {
            clothUI.gameObject.SetActive(true);
            weaponSelectUI.gameObject.SetActive(true);
            weaponSlotUI.gameObject.SetActive(false);
        }

        public void CloseClothUI()
        {
            clothUI.gameObject.SetActive(false);
        }
    }
}