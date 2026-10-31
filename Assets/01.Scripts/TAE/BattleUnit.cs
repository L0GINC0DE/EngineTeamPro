using Agents;
using UnityEngine;

namespace _01.Scripts.TAE
{
    public enum UnitType { Player, Enemy, Boss }

    [RequireComponent(typeof(Attacker), typeof(HealthModule))]
    public class BattleUnit : MonoBehaviour
    {
        public UnitType type;
        public Attacker AttackCompo { get; private set; }
        public HealthModule HealthCompo { get; private set; }
        public bool IsAlive => isActiveAndEnabled && HealthCompo != null && !HealthCompo.IsDead;

        private void Awake()
        {
            AttackCompo = GetComponent<Attacker>();
            HealthCompo = GetComponent<HealthModule>();
        }
    }
}
