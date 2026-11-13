using UnityEngine;

namespace _01.Scripts.TAE.Interface
{
    public interface IDamageable
    {
        void ApplyDamage(int dmg, Vector2 kbPower);
    }
}