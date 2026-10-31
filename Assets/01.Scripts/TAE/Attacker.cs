using UnityEngine;

namespace _01.Scripts.TAE
{
    [DisallowMultipleComponent]
    public class Attacker : MonoBehaviour
    {
        [SerializeField, Min(1)] private int damage = 10;

        public bool Attack(BattleUnit target)
        {
            if (!isActiveAndEnabled || damage <= 0)
                return false;
            if (target == null || !target.IsAlive)
                return false;

            target.HealthCompo.TakeDamage(damage);
            return true;
        }
    }
}
