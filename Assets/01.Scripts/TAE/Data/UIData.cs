using UnityEngine;

namespace _01.Scripts.TAE.Data
{
    [CreateAssetMenu(fileName = "UIData", menuName = "SO/UIData", order = 0)]
    public class UIData : ScriptableObject
    {
        public string uiName;
        public Sprite icon;

        [Header("BodyUI Info")]
        public float moveSpeed;
        public float defendPower;

        [Header("WeaponUI Info")]
        public float attackRange;
        public float attackInterval;
        public float damage;
    }
}
