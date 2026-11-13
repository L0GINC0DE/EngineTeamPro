using _01.Scripts.TAE.Agents;
using UnityEngine;
using UnityEngine.UI;

namespace _01.Scripts.TAE.UI
{
    public class BodySlot : MonoBehaviour
    {
        public Transform content;
        public GameObject BtnTemplate;

        public void Initialize(AgentRenderer render)
        {
            foreach (var data in render.UIList)
            {
                var btnTemplate = Instantiate(BtnTemplate, content);
                btnTemplate.transform.GetChild(1).GetComponent<Image>().sprite = data.uiSprite;
                btnTemplate.GetComponent<Button>().onClick.AddListener(() =>
                {
                    render.Change(data);
                    UIManager.Instance.CloseClothUI();
                });
            }
        }
    }
}