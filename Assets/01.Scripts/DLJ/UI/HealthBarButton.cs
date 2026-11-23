using UnityEngine.UI;

public sealed class HealthBarButton : Button
{
    private HealthBarUI healthBar;

    protected override void OnEnable()
    {
        healthBar = GetComponent<HealthBarUI>();
        base.OnEnable();
    }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        base.DoStateTransition(state, instant);
        if (healthBar != null && gameObject.activeInHierarchy)
            healthBar.SetColor(state is SelectionState.Highlighted or SelectionState.Pressed or SelectionState.Selected);
    }

    protected override void InstantClearState()
    {
        base.InstantClearState();
        if (healthBar != null)
            healthBar.SetColor(false);
    }
}
