using System;
using UnityEngine;

public class KTH_Boss : KTH_Enemy
{
    [SerializeField]private KTH_PatternSO ps;
    
    int index;
    public KTH_BossAction NextAction=>ps.patterns[index];

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            TakeTurn();
        }
    }
    
    public void TakeTurn()
    {
        if (ps.patterns.Count == 0) return;

        ps.patterns[index].Execute(this);
        index=(index+1)%ps.patterns.Count;
    }
}
