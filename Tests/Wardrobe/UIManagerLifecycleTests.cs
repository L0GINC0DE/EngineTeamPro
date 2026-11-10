using _01.Scripts.TAE;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class UIManagerLifecycleTests
{
    private GameObject owner;

    [TearDown]
    public void TearDown()
    {
        if (owner != null) Object.DestroyImmediate(owner);
    }

    [Test]
    public void HidingBeforeOpeningIsSafeAndRepeatable()
    {
        owner = new GameObject("UIManager lifecycle");
        var view = owner.AddComponent<UIManager>();
        var body = new GameObject("Body", typeof(RectTransform), typeof(Button));
        body.transform.SetParent(owner.transform);
        var weapon = new GameObject("Weapon", typeof(RectTransform), typeof(Button));
        weapon.transform.SetParent(owner.transform);
        view.bodyBtn = body.GetComponent<Button>();
        view.weaponBtn = weapon.GetComponent<Button>();
        view.infoUI = new GameObject("Panel");
        view.infoUI.transform.SetParent(owner.transform);

        Assert.DoesNotThrow(() => view.HideUI());
        Assert.DoesNotThrow(() => view.HideUI());
        Assert.That(view.infoUI.activeSelf, Is.False);
    }

    [Test]
    public void DisablingWithoutInspectorReferencesIsSafe()
    {
        owner = new GameObject("Unconfigured UIManager");
        owner.AddComponent<UIManager>();
        owner.SetActive(false);
    }
}
