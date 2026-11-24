using _01.Scripts.TAE.Managers;
using UnityEngine;

namespace _01.Scripts.TAE.Players
{
    public class PlayerAttack : MonoBehaviour
    {
        [SerializeField] private BattleUnit target;

        public void Attack() => TryAttack();

        public bool TryAttack()
        {
            TurnManager manager = TurnManager.Instance;
            return manager != null && manager.PlayerAttack(target != null ? target : manager.GetFirstLivingEnemy());
        }
    }
}
