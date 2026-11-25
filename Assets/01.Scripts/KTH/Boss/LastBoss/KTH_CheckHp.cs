using _01.Scripts.TAE.Agents;
using UnityEngine;

public class KTH_CheckHp : MonoBehaviour
{
    [SerializeField] private KTH_Boss phase1;
    [SerializeField] private KTH_Boss phase2;
    [SerializeField] private HealthModule hm;
    [Range(0, 1)]
    [SerializeField] private float phaseRatio = 0.5f;

    private bool changed;

    private void Awake()
    {
        phase1.enabled = true;
        phase2.enabled = false;
        hm=GetComponent<HealthModule>();
    }

    private void OnEnable() => hm.OnHealthChanged += Check;
    private void OnDisable() => hm.OnHealthChanged -= Check;

    private void Check(int prev, int next)
    {
        if (changed || next > hm.MaxHealth * phaseRatio) return;

        changed = true;
        phase1.enabled = false;
        phase2.enabled = true;
    }
}
