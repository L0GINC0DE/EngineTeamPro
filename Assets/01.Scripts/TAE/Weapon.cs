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

        public UIData data { get; private set; }

        private void Awake()
        {
            weaponSprite = GetComponent<SpriteRenderer>();
        }

        public void SetWeapon(UIData data)
        {
            if (data == null) return;
            this.data = data;
            if (weaponSprite == null) weaponSprite = GetComponent<SpriteRenderer>();
            if (weaponSprite != null) weaponSprite.sprite = data.icon;
            attackInterval = data.attackInterval;
            damage = data.damage;
        }
    }
}
