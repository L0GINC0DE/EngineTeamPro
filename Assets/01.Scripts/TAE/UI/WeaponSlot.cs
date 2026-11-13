using _01.Scripts.TAE.Combat;
using _01.Scripts.TAE.Data;
using UnityEngine;
using UnityEngine.UI;

namespace _01.Scripts.TAE.UI
{
    public class WeaponSlot : MonoBehaviour
    {
        public Transform content;
        public GameObject BtnTemplate;

        public void Initialize(WeaponHolder holder)
        {
            foreach (var data in holder.WeaponDataList)
            {
                var btnTemplate = Instantiate(BtnTemplate, content);
                btnTemplate.transform.GetChild(1).GetComponent<Image>().sprite = data.uiSprite;
                btnTemplate.GetComponent<Button>().onClick.AddListener(() =>
                {
                    holder.TryChangeWeapon(data);
                    UIManager.Instance.CloseClothUI();
                });
            }
        }
    }
}