using System.Collections.Generic;
using UnityEngine;

namespace _01.Scripts.TAE.Combat
{
    public class GimmickList : MonoBehaviour
    {
        public List<Gimmick> gimmickList;
    }

    [System.Serializable]
    public class Gimmick
    {
        public bool canSelect;
        public InteractPoint point;

        public void CheckSelect(bool isCheck)
        {
            canSelect = isCheck;
        }
    }
}