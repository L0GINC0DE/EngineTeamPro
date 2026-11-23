using UnityEngine;
using UnityEngine.U2D.Animation;

namespace _01.Scripts.TAE.Data
{
    [CreateAssetMenu(fileName = "UIWeaponData", menuName = "SO/UI/Weapon", order = 0)]
    public class UIDataSO : ScriptableObject
    {
        [Header("Common Settings")]
        public string uiName;
        public Sprite uiSprite;
        
        [Header("Body Settings")]
        public float moveSpeed;
        public int health;
        public SpriteLibraryAsset spriteLibrary;
        
        [Header("Weapon Settings")]
        public Vector3 weaponSpritePos;
        public int damage;
        public float swingCoolTime;
        public float knockbackPower;
        public float attackRange;
        
        [Header("Gimmick Settings")]
        public float duration;
        
    }
}