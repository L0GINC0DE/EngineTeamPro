using CoreLib;
using _01.Scripts.KTH;
using _01.Scripts.TAE.Test;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.UI;

namespace _01.Scripts.TAE
{
    public class UIManager : MonoSingleton<UIManager>
    {
        public Image bodyImg;
        public Image weaponImg;

        public Button bodyBtn;
        public Button weaponBtn;

        public GameObject infoUI;

        private PlatformPlayer player;

        public void ShowUI(PlatformPlayer target, InteractTest source)
        {
            HideUI();
            if (infoUI == null || bodyBtn == null || weaponBtn == null
                || target == null) return;
            infoUI.SetActive(true);
            if (!target.BeginWardrobe(source, infoUI))
            {
                infoUI.SetActive(false);
                return;
            }
            player = target;
            bodyBtn.interactable = source.Data != player.data.CurrentWeaponData;
            weaponBtn.interactable = source.Data != player.data.CurrentBodyData;
            bodyBtn.onClick.AddListener(SelectBody);
            weaponBtn.onClick.AddListener(SelectWeapon);
        }

        private void SelectBody()
        {
            if (player != null && player.ChangeBody()) HideUI();
        }

        private void SelectWeapon()
        {
            if (player != null && player.ChangeWeapon()) HideUI();
        }


        public void HideUI()
        {
            if (bodyBtn != null) bodyBtn.onClick.RemoveListener(SelectBody);
            if (weaponBtn != null) weaponBtn.onClick.RemoveListener(SelectWeapon);
            player = null;
            if (infoUI != null) infoUI.SetActive(false);
        }

        private void Update()
        {
            if (infoUI == null) return;
            if (player == null && !infoUI.activeSelf) return;
            if (player == null || !player.CanUseWardrobe || !infoUI.activeInHierarchy
                || (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)) HideUI();
        }

        private void OnDisable() => HideUI();
    }
}
