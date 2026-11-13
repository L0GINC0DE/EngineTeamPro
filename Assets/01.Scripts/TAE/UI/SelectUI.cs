using _01.Scripts.TAE.Data;
using UnityEngine;
using UnityEngine.UI;

namespace _01.Scripts.TAE.UI
{
    public class SelectUI : MonoBehaviour
    {
        public Image image;
        
        public virtual void SetUp(UIDataSO data)
        {
            image.sprite = data.uiSprite;
        }
    }
}