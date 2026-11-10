using System;
using Agents;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class KTH_ATPattern1 : KTH_BossAction
{
    [SerializeField] private KTH_TalkUI ui;
    [SerializeField] private KTH_TextSo[] texts;
    [SerializeField] private int damage = 10;
    [SerializeField] private Color selectColor;
    
    public override void Execute(KTH_Boss kthBoss)
    {
        KTH_TextSo so = texts[Random.Range(0, texts.Length)];
        ui.Show(selectColor,so, correct =>
        {
            int d = correct ? damage / 2 : damage;
            if (kthBoss.target.TryGetComponent(out HealthModule hm))
                hm.TakeDamage(d, kthBoss.transform.position);
        });
    }
}
