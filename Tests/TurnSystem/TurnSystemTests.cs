using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using _01.Scripts.TAE;
using _01.Scripts.TAE.Managers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TurnSystemTests
{
    private readonly List<GameObject> objects = new();

    [TearDown]
    public void TearDown()
    {
        for (int i = objects.Count - 1; i >= 0; i--)
            if (objects[i] != null) Object.DestroyImmediate(objects[i]);
        objects.Clear();
    }

    private BattleUnit CreateUnit(UnitType type, int health = 50)
    {
        var owner = new GameObject(type.ToString());
        objects.Add(owner);
        owner.SetActive(false);
        BattleUnit unit = owner.AddComponent<BattleUnit>();
        unit.type = type;
        SetField(owner.GetComponent<Agents.HealthModule>(), "maxHealth", health);
        owner.SetActive(true);
        return unit;
    }

    private TurnManager CreateBattle(BattleUnit player, params BattleUnit[] enemies)
    {
        var owner = new GameObject("TurnManager");
        objects.Add(owner);
        owner.SetActive(false);
        var manager = owner.AddComponent<TurnManager>();
        SetField(manager, "player", player);
        owner.SetActive(true);
        manager.AddTurn(enemies);
        manager.StartTurn();
        return manager;
    }

    private static void SetField(object target, string name, object value) =>
        target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);

    [Test]
    public void SuccessfulAttackRunsEachEnemyOnceInRegistrationOrder()
    {
        var player = CreateUnit(UnitType.Player);
        var first = CreateUnit(UnitType.Enemy);
        var boss = CreateUnit(UnitType.Boss);
        SetField(boss.AttackCompo, "damage", 7);
        var manager = CreateBattle(player, first, boss);
        var playerHealth = new List<int>();
        player.HealthCompo.OnHealthChanged += (previous, next) => playerHealth.Add(next);

        Assert.That(manager.PlayerAttack(first), Is.True);
        Assert.That(first.HealthCompo.CurrentHealth, Is.EqualTo(40));
        Assert.That(playerHealth, Is.EqualTo(new[] { 40, 33 }));
        Assert.That(manager.CurrentUnit, Is.SameAs(player));
        Assert.That(manager.IsPlayerTurn, Is.True);
    }

    [Test]
    public void InvalidTargetsDoNotConsumeThePlayerTurn()
    {
        var player = CreateUnit(UnitType.Player);
        var enemy = CreateUnit(UnitType.Enemy);
        var outsider = CreateUnit(UnitType.Enemy);
        var manager = CreateBattle(player, enemy);
        Assert.That(manager.PlayerAttack(null), Is.False);
        Assert.That(manager.PlayerAttack(player), Is.False);
        Assert.That(manager.PlayerAttack(outsider), Is.False);
        Assert.That(player.HealthCompo.CurrentHealth, Is.EqualTo(50));
        Assert.That(manager.CurrentUnit, Is.SameAs(player));
    }

    [Test]
    public void UnassignedPlayerDoesNotSelectAnotherUnitAutomatically()
    {
        CreateUnit(UnitType.Player);
        var enemy = CreateUnit(UnitType.Enemy);
        var manager = CreateBattle(null, enemy);
        Assert.That(manager.CurrentUnit, Is.Null);
        Assert.That(manager.IsPlayerTurn, Is.False);
    }

    [Test]
    public void DeadEnemyIsSkippedAndCannotBeTargeted()
    {
        var player = CreateUnit(UnitType.Player);
        var dead = CreateUnit(UnitType.Enemy);
        var alive = CreateUnit(UnitType.Enemy);
        var manager = CreateBattle(player, dead, alive);
        dead.HealthCompo.TakeDamage(50);
        Assert.That(manager.PlayerAttack(dead), Is.False);
        manager.PlayerAttack(alive);
        Assert.That(player.HealthCompo.CurrentHealth, Is.EqualTo(40));
    }

    [Test]
    public void KillingFinalEnemyEndsBattleWithoutRetaliation()
    {
        var player = CreateUnit(UnitType.Player);
        var enemy = CreateUnit(UnitType.Enemy, 10);
        var manager = CreateBattle(player, enemy);
        manager.PlayerAttack(enemy);
        manager.EndTurn();
        manager.StartTurn();
        Assert.That(manager.BattleEnded, Is.True);
        Assert.That(manager.CurrentUnit, Is.Null);
        Assert.That(player.HealthCompo.CurrentHealth, Is.EqualTo(50));
    }

    [Test]
    public void PlayerDeathStopsRemainingEnemies()
    {
        var player = CreateUnit(UnitType.Player, 10);
        var first = CreateUnit(UnitType.Enemy);
        var second = CreateUnit(UnitType.Enemy);
        var manager = CreateBattle(player, first, second);
        manager.EndTurn();
        Assert.That(player.HealthCompo.IsDead, Is.True);
        Assert.That(manager.BattleEnded, Is.True);
        Assert.That(manager.PlayerAttack(first), Is.False);
    }

    [Test]
    public void EnemyEntriesAreProcessedAsRegistered()
    {
        var player = CreateUnit(UnitType.Player);
        var enemy = CreateUnit(UnitType.Enemy);
        var manager = CreateBattle(player, enemy, null, enemy);
        manager.EndTurn();
        Assert.That(player.HealthCompo.CurrentHealth, Is.EqualTo(30));
    }

    [Test]
    public void RegisteringNewEncounterReplacesOldEnemiesAndClearsVictory()
    {
        var player = CreateUnit(UnitType.Player);
        var first = CreateUnit(UnitType.Enemy, 10);
        var manager = CreateBattle(player, first);
        manager.PlayerAttack(first);
        var second = CreateUnit(UnitType.Enemy);
        manager.AddTurn(new[] { second });
        manager.StartTurn();
        Assert.That(manager.BattleEnded, Is.False);
        Assert.That(manager.PlayerAttack(first), Is.False);
        manager.PlayerAttack(second);
        Assert.That(player.HealthCompo.CurrentHealth, Is.EqualTo(40));
        Assert.That(manager.GetFirstLivingEnemy(), Is.SameAs(second));
    }

    [Test]
    public void HitCallbacksCannotRunAnotherAttackOrAdvanceTheTurn()
    {
        var player = CreateUnit(UnitType.Player);
        var enemy = CreateUnit(UnitType.Enemy);
        var manager = CreateBattle(player, enemy);
        enemy.HealthCompo.OnHealthChanged += (previous, next) =>
        {
            Assert.That(manager.PlayerAttack(enemy), Is.False);
            manager.EndTurn();
            manager.StartTurn();
            manager.AddTurn(new[] { enemy });
        };
        manager.PlayerAttack(enemy);
        Assert.That(enemy.HealthCompo.CurrentHealth, Is.EqualTo(40));
        Assert.That(player.HealthCompo.CurrentHealth, Is.EqualTo(40));
        Assert.That(manager.IsPlayerTurn, Is.True);
    }

    [Test]
    public void ExistingComponentsUseTheManagerCurrentUnit()
    {
        var player = CreateUnit(UnitType.Player);
        var enemy = CreateUnit(UnitType.Enemy);
        var manager = CreateBattle(player, enemy);
        Assert.That(manager.CurrentUnit, Is.SameAs(player));
        manager.EndTurn();
        Assert.That(player.HealthCompo.CurrentHealth, Is.EqualTo(40));
        Assert.That(manager.CurrentUnit, Is.SameAs(player));
        Assert.That(manager.IsPlayerTurn, Is.True);
    }

    [Test]
    public void StartingAgainDoesNotRepeatAnActiveTurn()
    {
        var player = CreateUnit(UnitType.Player);
        var enemy = CreateUnit(UnitType.Enemy);
        var manager = CreateBattle(player, enemy);
        manager.StartTurn();
        manager.StartTurn();
        manager.PlayerAttack(enemy);
        Assert.That(player.HealthCompo.CurrentHealth, Is.EqualTo(40));
    }

    [Test]
    public void CommandRejectsDeadDisabledAndSelfTargets()
    {
        var player = CreateUnit(UnitType.Player);
        var enemy = CreateUnit(UnitType.Enemy);
        Assert.That(new AttackCommand(player, player).Execute(), Is.False);
        enemy.gameObject.SetActive(false);
        Assert.That(new AttackCommand(player, enemy).Execute(), Is.False);
        enemy.gameObject.SetActive(true);
        enemy.HealthCompo.TakeDamage(50);
        Assert.That(new AttackCommand(player, enemy).Execute(), Is.False);
        Assert.That(new AttackCommand(null, player).Execute(), Is.False);
    }

    [Test]
    public void ZeroDamageAttackDoesNotConsumeTurn()
    {
        var player = CreateUnit(UnitType.Player);
        var enemy = CreateUnit(UnitType.Enemy);
        var manager = CreateBattle(player, enemy);
        SetField(player.AttackCompo, "damage", 0);
        Assert.That(manager.PlayerAttack(enemy), Is.False);
        Assert.That(manager.IsPlayerTurn, Is.True);
        Assert.That(player.HealthCompo.CurrentHealth, Is.EqualTo(50));
    }

    [Test]
    public void AttackInputAndAttackUiRouteThroughCommands()
    {
        var player = CreateUnit(UnitType.Player);
        var enemy = CreateUnit(UnitType.Enemy);
        var manager = CreateBattle(player, enemy);
        player.gameObject.AddComponent<PlayerAttack>().Attack();
        player.gameObject.AddComponent<AttackUI>().Use();
        Assert.That(enemy.HealthCompo.CurrentHealth, Is.EqualTo(30));
        Assert.That(player.HealthCompo.CurrentHealth, Is.EqualTo(30));
        Assert.That(manager.IsPlayerTurn, Is.True);
    }

    [Test]
    public void EmptyEncounterEndsOnceWithoutModuloByZero()
    {
        var player = CreateUnit(UnitType.Player);
        var manager = CreateBattle(player);
        manager.EndTurn();
        manager.StartTurn();
        Assert.That(manager.BattleEnded, Is.True);
        Assert.That(manager.CurrentUnit, Is.Null);
    }

    [Test]
    public void DisabledManagerDoesNotAcceptActionsOrAdvanceTurns()
    {
        var player = CreateUnit(UnitType.Player);
        var enemy = CreateUnit(UnitType.Enemy);
        var manager = CreateBattle(player, enemy);
        manager.enabled = false;
        Assert.That(manager.PlayerAttack(enemy), Is.False);
        manager.EndTurn();
        Assert.That(player.HealthCompo.CurrentHealth, Is.EqualTo(50));
        manager.enabled = true;
        Assert.That(manager.PlayerAttack(enemy), Is.True);
    }

    [Test]
    public void RevivedPlayerCanStartANewEncounterAfterDefeat()
    {
        var player = CreateUnit(UnitType.Player, 10);
        var enemy = CreateUnit(UnitType.Enemy);
        var manager = CreateBattle(player, enemy);
        manager.EndTurn();
        Assert.That(manager.BattleEnded, Is.True);
        player.HealthCompo.ResetHealth();
        manager.AddTurn(new[] { enemy });
        manager.StartTurn();
        Assert.That(manager.BattleEnded, Is.False);
        Assert.That(manager.IsPlayerTurn, Is.True);
    }

    [Test]
    public void InactiveEnemyDoesNotRetaliate()
    {
        var player = CreateUnit(UnitType.Player);
        var first = CreateUnit(UnitType.Enemy);
        var second = CreateUnit(UnitType.Enemy);
        var manager = CreateBattle(player, first, second);
        first.gameObject.SetActive(false);
        manager.PlayerAttack(second);
        Assert.That(player.HealthCompo.CurrentHealth, Is.EqualTo(40));
        Assert.That(manager.PlayerAttack(first), Is.False);
    }

    [UnityTest]
    public IEnumerator ExternalFinalEnemyDeathEndsBattleWhileAwaitingInput()
    {
        var player = CreateUnit(UnitType.Player);
        var enemy = CreateUnit(UnitType.Enemy);
        var manager = CreateBattle(player, enemy);
        enemy.HealthCompo.TakeDamage(50);
        yield return null;
        Assert.That(manager.BattleEnded, Is.True);
        Assert.That(manager.IsPlayerTurn, Is.False);
    }

    [UnityTest]
    public IEnumerator DisablingAndReenablingPlayerDoesNotDuplicateActions()
    {
        var player = CreateUnit(UnitType.Player);
        var enemy = CreateUnit(UnitType.Enemy);
        var manager = CreateBattle(player, enemy);
        player.gameObject.SetActive(false);
        player.gameObject.SetActive(true);
        yield return null;
        manager.PlayerAttack(enemy);
        Assert.That(player.HealthCompo.CurrentHealth, Is.EqualTo(40));
        Assert.That(manager.IsPlayerTurn, Is.True);
    }

    [UnityTest]
    public IEnumerator DestroyingPlayerWhileWaitingEndsBattle()
    {
        var player = CreateUnit(UnitType.Player);
        var enemy = CreateUnit(UnitType.Enemy);
        var manager = CreateBattle(player, enemy);
        Object.DestroyImmediate(player.gameObject);
        yield return null;
        Assert.That(manager.BattleEnded, Is.True);
        Assert.That(manager.CurrentUnit, Is.Null);
    }
}
