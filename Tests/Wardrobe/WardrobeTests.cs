using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using _01.Scripts.KTH;
using _01.Scripts.TAE;
using _01.Scripts.TAE.Data;
using _01.Scripts.TAE.Test;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

public class WardrobeTests
{
    private readonly List<Object> objects = new();
    private PlatformPlayer player;
    private InteractTest wardrobe;
    private UIManager view;
    private UIData oldBody, oldWeapon, newBody, newWeapon;

    private GameObject MakeObject(string name, Transform parent = null)
    {
        var go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent);
        objects.Add(go);
        return go;
    }

    private UIData MakeData(float speed, float damage)
    {
        var data = ScriptableObject.CreateInstance<UIData>();
        data.moveSpeed = speed;
        data.defendPower = speed + 1;
        data.damage = damage;
        data.attackRange = damage + 1;
        data.attackInterval = 0.5f;
        objects.Add(data);
        return data;
    }

    private static void SetField(object target, string name, object value) =>
        target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);

    [SetUp]
    public void SetUp()
    {
        oldBody = MakeData(2, 3);
        oldWeapon = MakeData(4, 5);
        newBody = MakeData(6, 7);
        newWeapon = MakeData(8, 9);
        var root = MakeObject("Player");
        root.SetActive(false);
        root.AddComponent<Rigidbody2D>().gravityScale = 0;
        root.AddComponent<PlayerInputReader>();
        var interactor = root.AddComponent<Interactor>();
        SetField(interactor, "range", 2f);
        SetField(interactor, "whatIsInteractable", (LayerMask)(1 << 0));
        MakeObject("Body", root.transform).AddComponent<SpriteRenderer>();
        var movement = MakeObject("Movement", root.transform).AddComponent<AgentMovement>();
        SetField(movement, "checker", root.transform);
        var weaponObject = MakeObject("Weapon", root.transform);
        weaponObject.AddComponent<SpriteRenderer>();
        weaponObject.AddComponent<Weapon>();
        player = root.AddComponent<PlatformPlayer>();
        SetField(player, "startingBody", oldBody);
        SetField(player, "startingWeapon", oldWeapon);
        root.SetActive(true);

        var closet = MakeObject("Wardrobe");
        closet.AddComponent<BoxCollider2D>();
        wardrobe = closet.AddComponent<InteractTest>();
        SetField(wardrobe, "data", newBody);
        view = MakeObject("UIManager").AddComponent<UIManager>();
        view.infoUI = MakeObject("Panel", view.transform);
        view.bodyBtn = MakeObject("Body button", view.infoUI.transform).AddComponent<Button>();
        view.weaponBtn = MakeObject("Weapon button", view.infoUI.transform).AddComponent<Button>();
        view.bodyImg = MakeObject("Body icon", view.infoUI.transform).AddComponent<Image>();
        view.weaponImg = MakeObject("Weapon icon", view.infoUI.transform).AddComponent<Image>();
        view.infoUI.SetActive(false);
        Physics2D.SyncTransforms();
    }

    [TearDown]
    public void TearDown()
    {
        for (int i = objects.Count - 1; i >= 0; i--)
            if (objects[i] != null) Object.DestroyImmediate(objects[i]);
        objects.Clear();
    }

    [UnityTest]
    public IEnumerator HoldingEnterOpensWardrobeOnceAndEarlyReleaseCancels()
    {
        var previousMode = InputSystem.settings.updateMode;
        var previousBackground = InputSystem.settings.backgroundBehavior;
        var previousEditor = InputSystem.settings.editorInputBehaviorInPlayMode;
        var keyboard = InputSystem.AddDevice<Keyboard>();
        int attacks = 0;
        int interactions = 0;
        player.InputCompo.OnAttackKeyPressed += () => attacks++;
        player.InputCompo.OnInteractKeyPressed += () => interactions++;
        try
        {
            InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsManually;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            yield return null;
            InputState.Change(keyboard, new KeyboardState(Key.Enter));
            Assert.That(keyboard.enterKey.isPressed, Is.True, "Input device must receive Enter.");
            Assert.That(interactions, Is.Zero, "Pressing must not complete a Hold interaction immediately.");
            Assert.That(view.infoUI.activeSelf, Is.False);

            InputState.Change(keyboard, new KeyboardState());
            yield return new WaitForSecondsRealtime(InputSystem.settings.defaultHoldTime + 0.1f);
            InputSystem.Update();
            Assert.That(interactions, Is.Zero, "Releasing early must cancel the interaction.");

            InputState.Change(keyboard, new KeyboardState(Key.Enter));
            yield return new WaitForSecondsRealtime(InputSystem.settings.defaultHoldTime + 0.1f);
            InputSystem.Update();
            Assert.That(interactions, Is.EqualTo(1), "Holding must complete the interaction.");
            Assert.That(view.infoUI.activeSelf, Is.True);
            Assert.That(player.CanUseWardrobe, Is.True);
            Assert.That(attacks, Is.Zero);

            yield return new WaitForSecondsRealtime(InputSystem.settings.defaultHoldTime + 0.1f);
            InputSystem.Update();
            Assert.That(interactions, Is.EqualTo(1), "Continuing to hold must not repeat the interaction.");
        }
        finally
        {
            InputSystem.RemoveDevice(keyboard);
            InputSystem.settings.updateMode = previousMode;
            InputSystem.settings.backgroundBehavior = previousBackground;
            InputSystem.settings.editorInputBehaviorInPlayMode = previousEditor;
        }
    }

    [Test]
    public void ChangingBodyPreservesWeaponAndAppliesStatsImmediately()
    {
        wardrobe.Interact(player);
        view.bodyBtn.onClick.Invoke();
        Assert.That(player.data.CurrentBodyData, Is.SameAs(newBody));
        Assert.That(player.data.CurrentWeaponData, Is.SameAs(oldWeapon));
        Assert.That(player.defendPower, Is.EqualTo(newBody.defendPower));
        Assert.That(player.CanUseWardrobe, Is.False, "Selecting an item must close the wardrobe immediately.");
        Assert.That(typeof(AgentMovement).GetField("moveSpeed", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player.MoveCompo), Is.EqualTo(newBody.moveSpeed));
    }

    [Test]
    public void ChangingWeaponPreservesBodyAndAppliesStatsImmediately()
    {
        SetField(wardrobe, "data", newWeapon);
        wardrobe.Interact(player);
        view.weaponBtn.onClick.Invoke();
        Assert.That(player.data.CurrentBodyData, Is.SameAs(oldBody));
        Assert.That(player.data.CurrentWeaponData, Is.SameAs(newWeapon));
        Assert.That(player.WeaponCompo.damage, Is.EqualTo(newWeapon.damage));
        Assert.That(player.WeaponCompo.attackInterval, Is.EqualTo(newWeapon.attackInterval));
        Assert.That(player.attackRange, Is.EqualTo(newWeapon.attackRange));
        Assert.That(player.CanUseWardrobe, Is.False, "Selecting an item must close the wardrobe immediately.");
    }

    [Test]
    public void ClosedWardrobeRejectsChangesAndKeepsEquippedItems()
    {
        Assert.That(player.ChangeBody(), Is.False);
        wardrobe.Interact(player);
        view.bodyBtn.onClick.Invoke();
        view.HideUI();
        view.weaponBtn.onClick.Invoke();
        Assert.That(player.ChangeWeapon(), Is.False);
        Assert.That(player.data.CurrentBodyData, Is.SameAs(newBody));
        Assert.That(player.data.CurrentWeaponData, Is.SameAs(oldWeapon));
    }

    [Test]
    public void DeactivatingPanelImmediatelyRejectsEquipmentChanges()
    {
        wardrobe.Interact(player);
        view.infoUI.SetActive(false);
        Assert.That(player.CanUseWardrobe, Is.False);
        Assert.That(player.ChangeBody(), Is.False);
        Assert.That(player.ChangeWeapon(), Is.False);
        Assert.That(player.data.CurrentBodyData, Is.SameAs(oldBody));
        Assert.That(player.data.CurrentWeaponData, Is.SameAs(oldWeapon));
    }

    [Test]
    public void DeactivatingPanelAllowsMovementAndReopening()
    {
        var keyboard = InputSystem.AddDevice<Keyboard>();
        try
        {
            wardrobe.Interact(player);
            view.infoUI.SetActive(false);
            typeof(PlayerInputReader).GetProperty("MoveInput").SetValue(player.InputCompo, Vector2.right);
            typeof(PlatformPlayer).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(player, null);
            Assert.That(typeof(AgentMovement).GetField("moveDir", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player.MoveCompo), Is.EqualTo(Vector2.right));
            typeof(PlatformPlayer).GetMethod("HandleInteractKeyPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(player, null);
            Assert.That(view.infoUI.activeInHierarchy, Is.True);
        }
        finally
        {
            InputSystem.RemoveDevice(keyboard);
        }
    }

    [Test]
    public void OutOfRangeAndDisabledWardrobesRejectChanges()
    {
        wardrobe.Interact(player);
        player.transform.position = Vector3.right * 10;
        Physics2D.SyncTransforms();
        Assert.That(player.ChangeBody(), Is.False);
        player.transform.position = Vector3.zero;
        wardrobe.enabled = false;
        Physics2D.SyncTransforms();
        Assert.That(player.ChangeWeapon(), Is.False);
    }

    [Test]
    public void WardrobeWithNoItemCannotBeUsed()
    {
        SetField(wardrobe, "data", null);
        wardrobe.Interact(player);
        Assert.That(player.ChangeBody(), Is.False);
        Assert.That(player.ChangeWeapon(), Is.False);
        Assert.That(player.data.CurrentBodyData, Is.SameAs(oldBody));
        Assert.That(player.data.CurrentWeaponData, Is.SameAs(oldWeapon));
    }

    [Test]
    public void CurrentBodyCannotAlsoBeEquippedAsWeapon()
    {
        SetField(wardrobe, "data", oldBody);
        wardrobe.Interact(player);
        Assert.That(view.weaponBtn.interactable, Is.False);
        Assert.That(player.ChangeWeapon(), Is.False);
        view.weaponBtn.onClick.Invoke();
        Assert.That(view.infoUI.activeSelf, Is.True);
        Assert.That(player.data.CurrentWeaponData, Is.SameAs(oldWeapon));
    }

    [Test]
    public void CurrentWeaponCannotAlsoBeEquippedAsBody()
    {
        SetField(wardrobe, "data", oldWeapon);
        wardrobe.Interact(player);
        Assert.That(view.bodyBtn.interactable, Is.False);
        Assert.That(player.ChangeBody(), Is.False);
        view.bodyBtn.onClick.Invoke();
        Assert.That(view.infoUI.activeSelf, Is.True);
        Assert.That(player.data.CurrentBodyData, Is.SameAs(oldBody));
    }

    [Test]
    public void StartingWithTheSameBodyAndWeaponLeavesWeaponEmpty()
    {
        player.gameObject.SetActive(false);
        SetField(player, "startingBody", oldBody);
        SetField(player, "startingWeapon", oldBody);
        var clone = Object.Instantiate(player.gameObject);
        objects.Add(clone);
        clone.SetActive(true);
        var clonedPlayer = clone.GetComponent<PlatformPlayer>();
        Assert.That(clonedPlayer.data.CurrentBodyData, Is.SameAs(oldBody));
        Assert.That(clonedPlayer.data.CurrentWeaponData, Is.Null);
    }

    [Test]
    public void OpeningTwiceDoesNotRemoveOtherButtonListeners()
    {
        int clicks = 0;
        view.bodyBtn.onClick.AddListener(() => clicks++);
        wardrobe.Interact(player);
        view.HideUI();
        wardrobe.Interact(player);
        view.bodyBtn.onClick.Invoke();
        Assert.That(clicks, Is.EqualTo(1));
        Assert.That(player.data.CurrentBodyData, Is.SameAs(newBody));
    }
}
