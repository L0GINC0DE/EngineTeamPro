using UnityEngine;

namespace _01.Scripts.TAE.Agents
{
    public class UseBarUI : MonoBehaviour
    {
        [SerializeField] private Transform barTrm;
        
        public void SetNormalizeValue(float totalTime)
        {
            barTrm.localScale = new Vector3(totalTime, 1, 1);
        }
    }
}