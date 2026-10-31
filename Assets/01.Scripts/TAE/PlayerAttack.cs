using UnityEngine;
using _01.Scripts.TAE.Managers;

namespace _01.Scripts.TAE
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
