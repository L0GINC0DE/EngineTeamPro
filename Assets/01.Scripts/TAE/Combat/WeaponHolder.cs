using System.Collections.Generic;
using System.Linq;
using _01.Scripts.KTH;
using _01.Scripts.TAE.Data;
using CoreLib;
using UnityEngine;
using UnityEngine.Events;

namespace _01.Scripts.TAE.Combat
{
    public class WeaponHolder : MonoBehaviour
    {
        public UnityEvent<UIWeapon> OnChangeWeaponEvent;
        
        [SerializeField] private UIWeapon weaponPrefab;
        [SerializeField] private UIDataListSO weaponDataList;

        public NotifyValue<UIWeapon> CurrentWeapon;

        private PlayerInputReader _playerInput;

        private List<UIWeapon> uiWeaponList = new();
        private Dictionary<UIDataSO, int> weaponDict = new();
        
        public IReadOnlyList<UIDataSO> WeaponDataList => weaponDataList.uiDataList;

        private void Awake()
        {
            CurrentWeapon = new NotifyValue<UIWeapon>();
            CurrentWeapon.OnValueChanged += HandleWeaponChange;
            
            MakeWeaponList();
            if(uiWeaponList.Count > 0)
                CurrentWeapon.Value = uiWeaponList.First();
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
            
            OnChangeWeaponEvent?.Invoke(CurrentWeapon.Value);
        }

        public void TryChangeWeapon(UIDataSO data)
        {
            ChangeWeapon(weaponDict[data]);
        }

        private void ChangeWeapon(int weaponIndex)
        {
            if (weaponIndex < uiWeaponList.Count && weaponIndex >= 0)
            {
                CurrentWeapon.Value = uiWeaponList[weaponIndex];
            }
        }

        public void MakeWeaponList()
        {
            int i = 0;
            foreach (var weaponData in weaponDataList.uiDataList)
            {
                UIWeapon ui = Instantiate(weaponPrefab, transform);
                ui.InitWeapon(weaponData);
                ui.gameObject.SetActive(false);
                uiWeaponList.Add(ui);
                weaponDict.Add(ui.UIData, i);
                i++;
            }
        }
        
    }
}