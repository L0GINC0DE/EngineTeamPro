using System.Collections.Generic;
using System.Linq;
using _01.Scripts.KTH;
using _01.Scripts.TAE.Data;
using CoreLib;
using UnityEngine;

namespace _01.Scripts.TAE.Combat
{
    public class WeaponHolder : MonoBehaviour
    {
        [SerializeField] private UIWeapon weaponPrefab;
        [SerializeField] private UIDataListSO weaponDataList;

        public NotifyValue<UIWeapon> CurrentWeapon;

        private PlayerInputReader _playerInput;

        private Dictionary<UIDataSO, UIWeapon> weaponDict = new();
        
        public UIDataListSO WeaponDataList => weaponDataList;

        private void Awake()
        {
            CurrentWeapon = new NotifyValue<UIWeapon>();
            CurrentWeapon.OnValueChanged += HandleWeaponChange;
            
            MakeWeaponList();
            TryChangeWeapon(weaponDataList.uiDataList.First());
        }

        private void OnDisable()
        {
            if (CurrentWeapon != null)
                CurrentWeapon.OnValueChanged -= HandleWeaponChange;
        }

        public void InitializeWeapons(PlatformPlayer player)
        {
            _playerInput = player.InputCompo;
            _playerInput.OnAttackKeyPressed += HandleAttackKeyPressed;
        }

        private void HandleAttackKeyPressed()
        {
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

        public void MakeWeaponList()
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