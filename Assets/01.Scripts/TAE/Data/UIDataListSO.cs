using System.Collections.Generic;
using UnityEngine;

namespace _01.Scripts.TAE.Data
{
    [CreateAssetMenu(fileName = "UIDataListSO", menuName = "UI/List", order = 0)]
    public class UIDataListSO : ScriptableObject
    {
        public List<UIDataSO> uiDataList;
    }
}