using _01.Scripts.TAE.Agents;
using UnityEngine;

public class KTH_LastBossPattern3 : KTH_BossAction
{
    [SerializeField]private HealthModule hm;
    [SerializeField]private int healAmount = 20;
    
    [SerializeField]private float windupTime = 0.6f;
    
    [Range(0, 1)]
    [SerializeField]private float maxHealReduction = 0.7f;
    
    [SerializeField]private KTH_AttackBundle bundle = new() { baseDamage = 0f };

    private void Awake()
    {
        if (hm == null) hm = GetComponentInParent<HealthModule>();
    }

    public override void Execute(KTH_Boss kthBoss)
    {
        KTH_Throw.Windup(kthBoss, bundle, windupTime, () => HealTiming(kthBoss));
    }

    private void HealTiming(KTH_Boss kthBoss)
    {
        KTH_Throw.Judge(kthBoss.target, bundle, kthBoss.transform.position, 1, result =>
        {
            float ratio = 1f - maxHealReduction * result.achievement;
            int amount = Mathf.RoundToInt(healAmount * ratio);
            hm.Heal(amount);
            Debug.Log($"[휴식] 달성률 {result.achievement:0.##} → 회복 {amount}");
        })();
    }
}
