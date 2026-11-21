using _01.Scripts.TAE.Managers;
using UnityEngine;

namespace _01.Scripts.TAE.Players
{
    public class PlayerAttack : MonoBehaviour
    {
        [SerializeField] private BattleUnit target;

        public void Attack()
        {
            TurnManager.Instance?.PlayerAttack(target != null ? target : TurnManager.Instance.GetFirstLivingEnemy());
        }
    }
}
