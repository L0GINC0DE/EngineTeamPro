using System;
using _01.Scripts.TAE.Data;
using UnityEngine;

namespace _01.Scripts.TAE
{
    public class Weapon : MonoBehaviour
    {
        private SpriteRenderer weaponSprite;
        public float attackInterval;
        public float damage;

        public TestPlayerData data;

        private void Awake()
        {
            weaponSprite = GetComponent<SpriteRenderer>();
        }

        public void SetWeapon(TestPlayerData data)
        {
            this.data = data;
            weaponSprite.sprite = data.uiSprite;
            attackInterval = data.attackInterval;
            damage = data.damage;
        }
    }
}