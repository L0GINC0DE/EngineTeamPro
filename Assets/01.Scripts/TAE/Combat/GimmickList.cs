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
        public bool CanSelect { get; private set; }
        public InteractPoint point;

        public void CheckSelect(bool isCheck) => CanSelect = isCheck;
        public void ResetPoint() => point.ResetPoint();
    }
}