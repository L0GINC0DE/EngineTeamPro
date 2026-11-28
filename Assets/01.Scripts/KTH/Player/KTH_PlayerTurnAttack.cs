using _01.Scripts.TAE.Agents;
using _01.Scripts.TAE.Managers;
using UnityEngine;

/// <summary>
/// 반격 패턴 테스트용: 내 턴에 키를 누르면 보스에게 데미지를 넣고 턴을 넘긴다.
/// TurnManager가 없으면 턴 상관없이 키를 누를 때마다 데미지만 넣는다.
/// 아무 오브젝트에나 붙여서 쓰면 된다.
/// </summary>
public class KTH_PlayerTurnAttack : MonoBehaviour
{
    [Tooltip("비우면 씬에서 KTH_Boss를 찾는다")]
    [SerializeField] private KTH_Boss boss;
    [SerializeField] private KeyCode attackKey = KeyCode.Q;
    [SerializeField, Min(1)] private int damage = 10;
    [Tooltip("데미지를 넣은 뒤 턴을 넘길지 (TurnManager가 있을 때만)")]
    [SerializeField] private bool endTurnAfterAttack = true;

    private void Update()
    {
        if (Input.GetKeyDown(attackKey)) Attack();
    }

    public void Attack()
    {
        TurnManager manager = TurnManager.Instance;
        if (manager != null && manager.isActiveAndEnabled && !manager.IsPlayerTurn)
        {
            Debug.Log("[플레이어 공격] 내 턴이 아님", this);
            return;
        }

        if (boss == null) boss = FindAnyObjectByType<KTH_Boss>();
        HealthModule hp = boss != null ? boss.GetComponentInChildren<HealthModule>() : null;
        if (hp == null || hp.IsDead)
        {
            Debug.Log("[플레이어 공격] 공격할 보스(HealthModule)가 없음", this);
            return;
        }

        hp.TakeDamage(damage, transform.position);
        Debug.Log($"[플레이어 공격] {boss.name}에게 {damage} 데미지 (남은 체력 {hp.CurrentHealth})", this);

        if (endTurnAfterAttack && manager != null && manager.isActiveAndEnabled)
            manager.EndTurn();
    }
}
