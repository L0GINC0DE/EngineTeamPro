using System.Collections;
using System.Linq;
using System.Reflection;
using _01.Scripts.TAE;
using _01.Scripts.TAE.Managers;
using _01.Scripts.TAE.Test;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

public class LegacyAssetsTests
{
    [UnityTest]
    public IEnumerator ConfiguredPrefabsUseIndependentBattleUnitComponents()
    {
        var playerObject = Object.Instantiate(Resources.Load<GameObject>("Player"));
        var managerObject = Object.Instantiate(Resources.Load<GameObject>("TurnManager"));
        var enemiesObject = Object.Instantiate(Resources.Load<GameObject>("Test"));
        typeof(TurnManager).GetField("player", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(managerObject.GetComponent<TurnManager>(), playerObject.GetComponent<BattleUnit>());
        try
        {
            yield return null;
            var player = playerObject.GetComponent<BattleUnit>();
            var bootstrap = enemiesObject.GetComponent<TestTurnSystem>();
            var manager = managerObject.GetComponent<TurnManager>();
            Assert.That(player, Is.Not.Null);
            Assert.That(player.GetType(), Is.EqualTo(typeof(BattleUnit)));
            Assert.That(player.HealthCompo, Is.Not.Null);
            Assert.That(player.AttackCompo, Is.Not.Null);
            Assert.That(bootstrap.enemies.Count, Is.EqualTo(4));
            Assert.That(bootstrap.enemies.All(enemy => enemy != null && enemy.IsAlive), Is.True);
            Assert.That(manager.CurrentUnit, Is.SameAs(player));
            manager.PlayerAttack(bootstrap.enemies[0]);
            Assert.That(player.HealthCompo.CurrentHealth, Is.EqualTo(10));
            Assert.That(bootstrap.enemies[0].HealthCompo.CurrentHealth, Is.EqualTo(40));
        }
        finally
        {
            Object.DestroyImmediate(enemiesObject);
            Object.DestroyImmediate(managerObject);
            Object.DestroyImmediate(playerObject);
        }
    }

#if UNITY_EDITOR
    [UnityTest]
    public IEnumerator SpaceKeyInTestSceneAttacksAndRunsEnemyTurns()
    {
        var previousUpdateMode = InputSystem.settings.updateMode;
        var previousBackgroundBehavior = InputSystem.settings.backgroundBehavior;
        var previousEditorBehavior = InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsManually;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        var keyboard = InputSystem.AddDevice<Keyboard>();
        _ = keyboard.spaceKey.wasPressedThisFrame;
        yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
            "Assets/Scenes/TestScene.unity", new LoadSceneParameters(LoadSceneMode.Additive));
        yield return null;
        Scene scene = SceneManager.GetSceneByPath("Assets/Scenes/TestScene.unity");
        try
        {
            var manager = TurnManager.Instance;
            var player = manager.CurrentUnit;
            var target = manager.GetFirstLivingEnemy();
            Assert.That(player.HealthCompo.CurrentHealth, Is.EqualTo(50));
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Space));
            InputSystem.Update();
            Assert.That(keyboard.spaceKey.wasPressedThisFrame, Is.True);
            yield return null;
            InputSystem.Update();
            Assert.That(target.HealthCompo.CurrentHealth, Is.EqualTo(40), "Space must trigger a real attack.");
            Assert.That(player.HealthCompo.CurrentHealth, Is.EqualTo(10), "All four enemies must retaliate once.");
            Assert.That(manager.IsPlayerTurn, Is.True);

            // Holding the key must not trigger repeated attacks every frame.
            yield return null;
            Assert.That(target.HealthCompo.CurrentHealth, Is.EqualTo(40));
        }
        finally
        {
            InputSystem.RemoveDevice(keyboard);
            InputSystem.settings.updateMode = previousUpdateMode;
            InputSystem.settings.backgroundBehavior = previousBackgroundBehavior;
            InputSystem.settings.editorInputBehaviorInPlayMode = previousEditorBehavior;
            foreach (GameObject root in scene.GetRootGameObjects()) Object.DestroyImmediate(root);
        }
        yield return SceneManager.UnloadSceneAsync(scene);
    }

    [UnityTest]
    public IEnumerator ExistingTestSceneLoadsAndCanCompleteAnAttackRound()
    {
        var operation = EditorSceneManager.LoadSceneAsyncInPlayMode(
            "Assets/Scenes/TestScene.unity", new LoadSceneParameters(LoadSceneMode.Additive));
        yield return operation;
        yield return null;
        Scene scene = SceneManager.GetSceneByPath("Assets/Scenes/TestScene.unity");
        try
        {
            var manager = TurnManager.Instance;
            var player = Object.FindObjectsByType<BattleUnit>(FindObjectsSortMode.None)
                .Single(unit => unit.type == UnitType.Player);
            Assert.That(manager, Is.Not.Null);
            Assert.That(manager.CurrentUnit, Is.SameAs(player));
            Assert.That(manager.PlayerAttack(manager.GetFirstLivingEnemy()), Is.True);
            Assert.That(player.HealthCompo.CurrentHealth, Is.EqualTo(10));
        }
        finally
        {
            foreach (GameObject root in scene.GetRootGameObjects()) Object.DestroyImmediate(root);
        }
        yield return SceneManager.UnloadSceneAsync(scene);
    }
#endif
}
