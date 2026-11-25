using System.Collections.Generic;
using System.Linq;
using _01.Scripts.TAE.CoreLib;
using _01.Scripts.TAE.Data;
using _01.Scripts.TAE.Players;
using UnityEngine;

namespace _01.Scripts.TAE.Combat
{
    public class WeaponHolder : MonoBehaviour
    {
        [SerializeField] private UIWeapon weaponPrefab;
        [SerializeField] private UIDataListSO weaponDataList;

        public NotifyValue<UIWeapon> CurrentWeapon;

        private PlayerInputReader _playerInput;
        private PlatformPlayer owner;

        private Dictionary<UIDataSO, UIWeapon> weaponDict = new();
        
        public UIDataListSO WeaponDataList => weaponDataList;
        
        [SerializeField] private UIDataSO shieldData;

        private void Awake()
        {
            CurrentWeapon = new NotifyValue<UIWeapon>();
            CurrentWeapon.OnValueChanged += HandleWeaponChange;
            
            MakeWeaponList();
            TryChangeWeapon(weaponDataList.uiDataList[1]);
        }

        private void OnDisable()
        {
            if (CurrentWeapon != null)
                CurrentWeapon.OnValueChanged -= HandleWeaponChange;
        }

        public void InitializeWeapons(PlatformPlayer player)
        {
            owner = player;
            _playerInput = player.InputCompo;
            _playerInput.OnAttackKeyPressed += HandleAttackKeyPressed;
        }

        private void HandleAttackKeyPressed()
        {
            if (Time.timeScale > 0.1f && !owner.HasGimmickShield)
                CurrentWeapon.Value.Attack();
        }

        private void HandleWeaponChange(UIWeapon prev, UIWeapon next)
        {
            prev?.gameObject.SetActive(false);
            next?.gameObject.SetActive(true);
        }

        public bool TryChangeWeapon(UIDataSO data)
        {
            if (data == null) return false;
            CurrentWeapon.Value = weaponDict[data];
            return true;
        }

        private void MakeWeaponList()
        {
            foreach (var weaponData in weaponDataList.uiDataList)
            {
                UIWeapon ui = Instantiate(weaponPrefab, transform);
                ui.InitWeapon(weaponData);
                ui.gameObject.SetActive(false);
                weaponDict.Add(ui.UIData, ui);
            }
        }
        
    }
}