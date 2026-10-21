using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class KTH_Boss : MonoBehaviour
{
    [SerializeField]private KTH_PatternSO ps;
    
    int index;
    public KTH_BossAction NextAction=>ps.patterns[index];
    
    public void TakeTurn()
    {
        ps.patterns[index].Execute(this);
        index=(index+1)%ps.patterns.Count;
    }
}
