namespace _01.Scripts.TAE
{
    public sealed class AttackCommand : ICommand
    {
        private readonly BattleUnit attacker;
        private readonly BattleUnit target;

        public AttackCommand(BattleUnit attacker, BattleUnit target)
        {
            this.attacker = attacker;
            this.target = target;
        }

        public bool Execute()
        {
            if (attacker == null || target == null) return false;
            if (!attacker.IsAlive || attacker == target) return false;

            return attacker.AttackCompo != null && attacker.AttackCompo.Attack(target);
        }
    }
}
