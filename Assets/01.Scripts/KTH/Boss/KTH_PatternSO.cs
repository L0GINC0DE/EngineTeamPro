using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PatternSo",menuName = "KTH/PatternSo")]
public class KTH_PatternSO : ScriptableObject
{ 
    public List<KTH_BossAction> patterns = new();
}
