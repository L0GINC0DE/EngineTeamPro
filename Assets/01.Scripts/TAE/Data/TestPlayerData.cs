using UnityEngine;

namespace _01.Scripts.TAE.Data
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "Data/Player", order = 0)]
    public class TestPlayerData : ScriptableObject
    {
        public string uiName;
        public Sprite uiSprite;
        public float attackRange;
        public float attackInterval;
        public float damage;
        public float rate;
        public float defendPower;
        public float moveSpeed;
    }
}
