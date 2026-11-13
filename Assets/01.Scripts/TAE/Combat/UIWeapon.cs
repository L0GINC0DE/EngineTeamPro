using System;
using _01.Scripts.TAE.Agents;
using _01.Scripts.TAE.Data;
using UnityEngine;

namespace _01.Scripts.TAE.Combat
{
    public class UIWeapon : MonoBehaviour
    {
        public AttackHitBox AttackCompo { get; private set; }
        [field: SerializeField] public UIDataSO UIData { get; private set; }

        [SerializeField] private SpriteRenderer uiWeaponSpriteRenderer;
        
        [SerializeField] private float attackCoolTime;
        [SerializeField] private int damage;
        [SerializeField] private float kbPower;
        
        private float lastAttackTime;

        public void InitWeapon(UIDataSO data)
        {
            UIData = data;
            AttackCompo = GetComponentInChildren<AttackHitBox>();
            SetUpWeaponData();
        }
        
        private void OnValidate()
        {
            SetUpWeaponData();
        }

        private void SetUpWeaponData()
        {
            if (uiWeaponSpriteRenderer == null) return;
            uiWeaponSpriteRenderer.sprite = UIData.uiSprite;
            uiWeaponSpriteRenderer.transform.localPosition = UIData.weaponSpritePos;
            gameObject.name = UIData.uiName;
            
            damage = UIData.damage;
            attackCoolTime = UIData.swingCoolTime;
            kbPower = UIData.knockbackPower;
        }

        public void Attack()
        {
            if (lastAttackTime + attackCoolTime > Time.time) return;
            lastAttackTime = Time.time;
            AttackCompo.Attack(damage, kbPower);
        }
    }
}