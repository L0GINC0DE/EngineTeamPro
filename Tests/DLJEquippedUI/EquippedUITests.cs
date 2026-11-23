using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using _01.Scripts.TAE;
using _01.Scripts.TAE.Managers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class EquippedUITests
{
    readonly List<Object> owned = new();
    const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    GameObject Make(string name, params Type[] types) { var go = new GameObject(name, types); owned.Add(go); return go; }
    static void Set(object target, string field, object value) => target.GetType().GetField(field, Private).SetValue(target, value);
    DLJ_EquippedUIView View(Transform parent, Transform anchor = null)
    {
        var go = Make("Slot", typeof(RectTransform), typeof(CanvasGroup));
        var rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        rect.sizeDelta = new Vector2(86.4f, 42f);
        var view = go.AddComponent<DLJ_EquippedUIView>();
        Set(view,"worldAnchor",anchor);
        return view;
    }
    T UI<T>(Transform parent = null) where T : PlayableUI
    {
        var go = Make(typeof(T).Name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var ui = go.AddComponent<T>();
        ui.UISo = ScriptableObject.CreateInstance<PlayableUISO>(); owned.Add(ui.UISo);
        var tex = new Texture2D(100, 42); owned.Add(tex);
        var sprite = Sprite.Create(tex, new Rect(0, 0, 100, 42), new Vector2(.5f,.5f), 100); owned.Add(sprite);
        go.GetComponent<Image>().sprite = sprite;
        go.GetComponent<Button>().onClick.AddListener(ui.Select);
        return ui;
    }
    [TearDown] public void Cleanup()
    {
        for (int i = owned.Count - 1; i >= 0; i--) if (owned[i] != null) Object.DestroyImmediate(owned[i]);
        owned.Clear();
    }

    [UnityTest] public IEnumerator EquippedButtonKeepsItsIdentityAndRestoresLayout()
    {
        var parent = Make("Original", typeof(RectTransform), typeof(Canvas));
        var ui = UI<AttackUI>(parent.transform);
        var rect = (RectTransform)ui.transform;
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.anchoredPosition3D = new Vector3(27, 13, 2); rect.sizeDelta = new Vector2(330,139);
        rect.localScale = new Vector3(2,3,1);
        var originalColor = new Color(.3f,.5f,.8f,1); ui.GetComponent<Image>().color = originalColor;
        var view = View(parent.transform);
        Assert.That(view.Attach(ui), Is.True);
        yield return null;
        Assert.That(ui.gameObject.activeInHierarchy, Is.True);
        Assert.That(ui.GetComponentInParent<Canvas>(), Is.SameAs(parent.GetComponent<Canvas>()),
            "The equipped button must stay in the original Canvas");
        Assert.That(ui.transform.parent, Is.EqualTo(view.transform));
        Assert.That(ui.GetComponent<Image>().color, Is.EqualTo(originalColor));
        int clicks = 0; ui.GetComponent<Button>().onClick.AddListener(() => clicks++);
        ui.GetComponent<Button>().onClick.Invoke();
        Assert.That(clicks, Is.EqualTo(1));
        view.Restore();
        Assert.That(ui.transform.parent, Is.EqualTo(parent.transform));
        Assert.That(rect.anchoredPosition3D, Is.EqualTo(new Vector3(27,13,2)));
        Assert.That(rect.sizeDelta, Is.EqualTo(new Vector2(330,139)));
        Assert.That(rect.localScale, Is.EqualTo(new Vector3(2,3,1)));
        Assert.That(rect.anchorMin, Is.EqualTo(Vector2.zero));
        Assert.That(rect.anchorMax, Is.EqualTo(Vector2.one));
        Assert.That(ui.GetComponent<Image>().color, Is.EqualTo(originalColor));
        Assert.That(view.transform.childCount, Is.Zero);
        view.Restore();
    }

    [UnityTest] public IEnumerator HealthBarContinuesUpdatingAfterAttachment()
    {
        var health = Make("Health").AddComponent<Agents.HealthModule>();
        var palette = Make("Palette", typeof(RectTransform), typeof(Canvas));
        var ui = UI<HealthBarUI>(palette.transform); ui.gameObject.SetActive(false);
        var slider = Make("Slider", typeof(RectTransform), typeof(Slider)).GetComponent<Slider>();
        slider.transform.SetParent(ui.transform, false);
        Set(ui,"health",health); ui.gameObject.SetActive(true);
        var view = View(palette.transform); view.Attach(ui);
        health.TakeDamage(17); yield return null;
        Assert.That(slider.value, Is.EqualTo(33));
        Assert.That(slider.maxValue, Is.EqualTo(50));
        Assert.That(ui.gameObject.activeInHierarchy, Is.True);
        view.Restore(); health.Heal(5);
        Assert.That(slider.value, Is.EqualTo(38));
    }

    [UnityTest] public IEnumerator EquippedViewFollowsThePlayerAndUsesTheSharedRaycaster()
    {
        var events = Make("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
        var camera = Make("Camera", typeof(Camera)).GetComponent<Camera>();
        camera.tag = "MainCamera"; camera.orthographic = true; camera.transform.position = new Vector3(0,0,-10);
        var palette = Make("Palette", typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup), typeof(GraphicRaycaster));
        var canvas = palette.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.planeDistance = 100f;
        var anchor = Make("Player anchor").transform;
        var view = View(palette.transform, anchor);
        var ui = UI<AttackUI>(palette.transform);
        view.Attach(ui);
        anchor.position = new Vector3(1,2,0);
        yield return null; yield return null; Canvas.ForceUpdateCanvases();
        var pointer = new PointerEventData(events) { position = camera.WorldToScreenPoint(anchor.position) };
        var hits = new List<RaycastResult>(); events.RaycastAll(pointer,hits);
        Vector2 buttonPosition = RectTransformUtility.WorldToScreenPoint(camera, ui.transform.position);
        Assert.That(Vector2.Distance(pointer.position, buttonPosition), Is.LessThan(1f), "Button must follow the player anchor");
        Assert.That(hits.Exists(hit => hit.gameObject == ui.gameObject), Is.True, "The shared Canvas must hit the equipped button");
        palette.GetComponent<CanvasGroup>().blocksRaycasts = false;
        hits.Clear(); events.RaycastAll(pointer,hits);
        Assert.That(hits.Exists(hit => hit.gameObject == ui.gameObject), Is.False);
    }

    [UnityTest] public IEnumerator PlainImageKeepsItsOriginalGraphic()
    {
        var palette = Make("Palette", typeof(RectTransform), typeof(Canvas));
        var ui = UI<TextBarUI>(palette.transform);
        var image = ui.GetComponent<Image>();
        image.sprite = null;
        var view = View(palette.transform); view.Attach(ui); yield return null;
        Assert.That(image.isActiveAndEnabled, Is.True);
        Assert.That(image.color.a, Is.EqualTo(1f));
    }

    [UnityTest] public IEnumerator DeletedSourceLeavesAnEmptySlot()
    {
        var palette = Make("Palette", typeof(RectTransform), typeof(Canvas));
        var ui = UI<AttackUI>(palette.transform);
        var view = View(palette.transform); view.Attach(ui); yield return null;
        Object.Destroy(ui.gameObject); yield return null; yield return null;
        Assert.That(view.transform.childCount, Is.Zero);
    }

    [UnityTest] public IEnumerator TurnPresentationDoesNotMoveEquippedDialogueAwayFromItsSlot()
    {
        var palette = Make("Palette", typeof(RectTransform), typeof(Canvas));
        var ui = UI<TextBarUI>(palette.transform);
        ((RectTransform)ui.transform).anchoredPosition = new Vector2(0,-700);
        var go = Make("Presentation"); go.SetActive(false);
        var presentation = go.AddComponent<PlayerTurnPresentation>();
        presentation.enabled = false;
        Set(presentation,"textBar",(RectTransform)ui.transform);
        Set(presentation,"textBarTargetPosition",new Vector2(0,-405));
        Set(presentation,"transitionDuration",.15f);
        go.SetActive(true);
        typeof(PlayerTurnPresentation).GetMethod("MoveToTurnPosition",Private).Invoke(presentation,new object[]{true});
        yield return new WaitForSeconds(.02f);
        var view = View(palette.transform); view.Attach(ui);
        yield return new WaitForSeconds(.2f);
        Assert.That(((RectTransform)ui.transform).anchoredPosition, Is.EqualTo(Vector2.zero),
            "An existing turn tween must not drag the equipped dialogue away from its slot");
    }

    [UnityTest] public IEnumerator SelectionKeepsEquippedActionsUsableAndRestoresOnNextTurn()
    {
        Make("EventSystem", typeof(EventSystem));
        var battle = Make("Battle").AddComponent<TurnManager>();
        var playerGo = Make("Player"); playerGo.SetActive(false);
        var player = playerGo.AddComponent<Player>();
        var unit = playerGo.GetComponent<BattleUnit>(); unit.type = UnitType.Player;
        var enemy = Make("Enemy").AddComponent<BattleUnit>(); enemy.type = UnitType.Enemy;
        Set(battle,"player",unit); Set(battle,"turnDelay",.01f);
        var appearanceGo = Make("Visual", typeof(SpriteRenderer), typeof(UnityEngine.U2D.Animation.SpriteLibrary), typeof(PlayerAppearance));
        appearanceGo.transform.SetParent(playerGo.transform);
        playerGo.SetActive(true); battle.AddTurn(new[]{enemy}); battle.StartTurn();
        var palette = Make("Palette", typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup));
        var body = UI<HealthBarUI>(palette.transform); var weapon = UI<HealUI>(palette.transform);
        var spare = UI<DefendUI>(palette.transform);
        var bodyView = View(palette.transform); var weaponView = View(palette.transform);
        var managerGo = Make("UIManager"); managerGo.SetActive(false);
        var manager = managerGo.AddComponent<PlayerUIManager>();
        Set(manager,"player",player); Set(manager,"buttonGroup",palette.GetComponent<CanvasGroup>());
        Set(manager,"bodyUIView",bodyView); Set(manager,"weaponUIView",weaponView);
        managerGo.SetActive(true); yield return null;
        manager.SelectPlayableUI(body);
        Assert.That(body.gameObject.activeInHierarchy, Is.True, "Equipping must not disable the real UI");
        Assert.That(manager.CurrentStep, Is.EqualTo(SelectionStep.Weapon));
        body.GetComponent<Button>().onClick.Invoke();
        Assert.That(manager.CurrentStep, Is.EqualTo(SelectionStep.Weapon), "A body slot cannot also become the weapon");
        manager.SelectPlayableUI(weapon);
        Assert.That(manager.CurrentStep, Is.EqualTo(SelectionStep.Action));
        manager.SelectPlayableUI(spare);
        Assert.That(spare.gameObject.activeSelf, Is.True, "Direct selection during action must not hide a third button");
        player.BattleUnit.HealthCompo.TakeDamage(30);
        weapon.GetComponent<Button>().onClick.Invoke();
        Assert.That(player.BodyUI, Is.SameAs(body)); Assert.That(player.WeaponUI, Is.SameAs(weapon));
        Assert.That(player.BattleUnit.HealthCompo.CurrentHealth, Is.EqualTo(45), "Equipped heal must execute its original action");
        yield return new WaitForSeconds(.1f);
        Assert.That(body.transform.parent, Is.EqualTo(palette.transform));
        Assert.That(weapon.transform.parent, Is.EqualTo(palette.transform));
        Assert.That(manager.CurrentStep, Is.EqualTo(SelectionStep.Player));
        manager.SelectPlayableUI(body); manager.enabled = false;
        Assert.That(body.transform.parent, Is.EqualTo(palette.transform));
    }

    [UnityTest] public IEnumerator NativeKeyboardNavigationReachesEquippedDefenseAndSubmitsIt()
    {
        var events = Make("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
        var camera = Make("Camera", typeof(Camera)).GetComponent<Camera>();
        camera.tag = "MainCamera"; camera.orthographic = true; camera.orthographicSize = 5f;
        camera.transform.position = new Vector3(0,0,-10);
        var battle = Make("Battle").AddComponent<TurnManager>();
        var playerGo = Make("Player"); playerGo.SetActive(false);
        var player = playerGo.AddComponent<Player>();
        var unit = playerGo.GetComponent<BattleUnit>(); unit.type = UnitType.Player;
        var enemy = Make("Enemy").AddComponent<BattleUnit>(); enemy.type = UnitType.Enemy;
        Set(battle,"player",unit); Set(battle,"turnDelay",.01f);
        playerGo.SetActive(true); battle.AddTurn(new[]{enemy}); battle.StartTurn();
        var palette = Make("Palette", typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup));
        var canvas = palette.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 100f;
        var body = UI<DefendUI>(palette.transform); var weapon = UI<HealUI>(palette.transform);
        var top = UI<ActUI>(palette.transform); var bottom = UI<TextBarUI>(palette.transform);
        ((RectTransform)top.transform).anchoredPosition = new Vector2(0, Screen.height * .4f);
        ((RectTransform)bottom.transform).anchoredPosition = new Vector2(0, -Screen.height * .4f);
        var bodyAnchor = Make("Body anchor").transform; bodyAnchor.position = new Vector3(-.5f,1.5f,0);
        var weaponAnchor = Make("Weapon anchor").transform; weaponAnchor.position = new Vector3(2.5f,1.5f,0);
        var bodyView = View(palette.transform, bodyAnchor); var weaponView = View(palette.transform, weaponAnchor);
        var managerGo = Make("UIManager"); managerGo.SetActive(false);
        var manager = managerGo.AddComponent<PlayerUIManager>();
        Set(manager,"player",player); Set(manager,"buttonGroup",palette.GetComponent<CanvasGroup>());
        Set(manager,"bodyUIView",bodyView); Set(manager,"weaponUIView",weaponView);
        managerGo.SetActive(true); yield return null; Canvas.ForceUpdateCanvases();

        manager.SelectPlayableUI(body); yield return null;
        events.SetSelectedGameObject(top.gameObject);
        Move(events, MoveDirection.Down, Vector2.down);
        Assert.That(events.currentSelectedGameObject, Is.EqualTo(body.gameObject),
            "Down must reach the visible defense below, even while choosing a weapon");
        ExecuteEvents.Execute(body.gameObject, new BaseEventData(events), ExecuteEvents.submitHandler);
        Assert.That(manager.CurrentStep, Is.EqualTo(SelectionStep.Weapon), "Focus must not equip the same UI twice");
        Assert.That(battle.IsPlayerTurn, Is.True);

        manager.SelectPlayableUI(weapon); yield return null;
        events.SetSelectedGameObject(top.gameObject); Move(events, MoveDirection.Down, Vector2.down);
        Assert.That(events.currentSelectedGameObject, Is.EqualTo(body.gameObject));
        Move(events, MoveDirection.Right, Vector2.right);
        Assert.That(events.currentSelectedGameObject, Is.EqualTo(weapon.gameObject));
        Move(events, MoveDirection.Left, Vector2.left);
        Assert.That(events.currentSelectedGameObject, Is.EqualTo(body.gameObject));
        Move(events, MoveDirection.Up, Vector2.up);
        Assert.That(events.currentSelectedGameObject, Is.EqualTo(top.gameObject));
        Move(events, MoveDirection.Down, Vector2.down);
        Assert.That(body.GetComponent<Selectable>().navigation.mode, Is.EqualTo(Navigation.Mode.Automatic));
        ExecuteEvents.Execute(events.currentSelectedGameObject, new BaseEventData(events), ExecuteEvents.submitHandler);
        Assert.That(player.BodyUI, Is.SameAs(body));
        Assert.That(player.DefenseStats.isDefending, Is.True, "Keyboard submit must execute the equipped defense");
        Assert.That(battle.IsPlayerTurn, Is.False);
        yield return new WaitForSeconds(.1f);
        Assert.That(body.transform.parent, Is.EqualTo(palette.transform));
        manager.enabled = false;
        Assert.That(top.GetComponent<Selectable>().navigation.mode, Is.EqualTo(Navigation.Mode.Automatic));
        Assert.That(body.GetComponent<Selectable>().navigation.mode, Is.EqualTo(Navigation.Mode.Automatic));
    }

    static void Move(EventSystem events, MoveDirection direction, Vector2 vector)
    {
        var axis = new AxisEventData(events) { moveDir = direction, moveVector = vector };
        ExecuteEvents.Execute(events.currentSelectedGameObject, axis, ExecuteEvents.moveHandler);
    }
}
