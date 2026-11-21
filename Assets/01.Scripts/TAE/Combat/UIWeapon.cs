using System;
using _01.Scripts.TAE.Data;
using UnityEngine;

namespace _01.Scripts.TAE.Combat
{
    public class UIWeapon : MonoBehaviour
    {
        [field: SerializeField] public UIDataSO UIData { get; private set; }

        [SerializeField] private SpriteRenderer uiWeaponSpriteRenderer;
        
        [SerializeField] private float attackCoolTime;
        [SerializeField] private int damage;
        [SerializeField] private float kbPower;

        public DamageCaster Caster { get; private set; }
        private float lastAttackTime;

        public void InitWeapon(UIDataSO data)
        {
            UIData = data;
            Caster = transform.root.GetComponent<DamageCaster>();
            SetUpWeaponData();
        }

        private void OnEnable()
        {
            SetUpWeaponData();
        }

        private void OnValidate()
        {
            SetUpWeaponData();
        }

        private void SetUpWeaponData()
        {
            if (uiWeaponSpriteRenderer == null || UIData == null) return;
            uiWeaponSpriteRenderer.sprite = UIData.uiSprite;
            uiWeaponSpriteRenderer.transform.localPosition = UIData.weaponSpritePos;
            gameObject.name = UIData.uiName;
            
            damage = UIData.damage;
            attackCoolTime = UIData.swingCoolTime;
            kbPower = UIData.knockbackPower;
            
            Caster.SetRange(UIData.attackRange);
        }

        public void Attack()
        {
            if (lastAttackTime + attackCoolTime > Time.time) return;
            lastAttackTime = Time.time;
            Caster.CastDamage(damage, kbPower);
        }
    }
}