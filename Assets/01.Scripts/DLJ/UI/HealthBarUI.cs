using System;
using _01.Scripts.TAE.Agents;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class HealthBarUI : ActionUI
{
    [Serializable]
    private struct ColorLayer
    {
        public Image image;
        public Sprite sprite;
    }

    [SerializeField] private HealthModule health;
    [SerializeField] private GameObject content;
    [SerializeField] private ColorLayer[] colorLayers = Array.Empty<ColorLayer>();

    private Slider healthSlider;
    private Button healthButton;

    protected override PlayerAction CreateAction() => new HealthBarAction(this);

    private void OnEnable()
    {
        healthSlider = GetComponentInChildren<Slider>(true);
        if (health != null) health.OnHealthChanged += OnHealthChanged;
        Refresh();
    }

    private void OnDisable()
    {
        if (healthButton != null) healthButton.onClick.RemoveListener(Select);
        if (health != null) health.OnHealthChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(int previous, int next) => Refresh();

    internal void SetColor(bool showColor)
    {
        foreach (ColorLayer layer in colorLayers)
        {
            if (layer.image != null)
                layer.image.overrideSprite = showColor ? layer.sprite : null;
        }
    }

    public void Refresh()
    {
        if (content != null) content.SetActive(!IsDestroyed);
        if (health == null || healthSlider == null) return;

        int current = health.CurrentHealth;
        healthSlider.minValue = 0f;
        healthSlider.maxValue = health.MaxHealth;
        healthSlider.SetValueWithoutNotify(current);
    }

    public override void SetDestroyed(bool destroyed)
    {
        base.SetDestroyed(destroyed);
        Refresh();
    }
}
