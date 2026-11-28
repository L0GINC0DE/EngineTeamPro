using System.Collections;
using _01.Scripts.TAE.Agents;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ParryTests
{
    [Test]
    public void LegacyReductionWorksWithoutEquipmentStats()
    {
        var attack = new KTH_AttackBundle { baseDamage = 20f, parryable = false };

        var result = KTH_DefenseCalculator.Calculate(attack, 1, 0, 0, 1, null);

        Assert.That(result.damage, Is.EqualTo(10));
        Assert.That(result.parried, Is.False);
    }

    [TestCase(DLJ_ParryMode.Unparryable, 1, 0, 20, 0)]
    [TestCase(DLJ_ParryMode.Block, 1, 0, 0, 0)]
    [TestCase(DLJ_ParryMode.Block, 0, 1, 15, 0)]
    [TestCase(DLJ_ParryMode.Reduce, 1, 0, 10, 0)]
    [TestCase(DLJ_ParryMode.Counter, 1, 0, 0, 30)]
    [TestCase(DLJ_ParryMode.Counter, 0, 1, 15, 0)]
    public void AttackModeControlsDamageAndCounter(DLJ_ParryMode mode, int perfect, int good,
        int playerDamage, int counterDamage)
    {
        var attack = new KTH_AttackBundle { baseDamage = 20, parryMode = mode, counterMultiplier = 1.5f };
        var result = KTH_DefenseCalculator.Calculate(attack, perfect, good, 0, 1, null);

        Assert.That(result.damage, Is.EqualTo(playerDamage));
        Assert.That(result.counterDamage, Is.EqualTo(counterDamage));
    }

    [Test]
    public void UnparryableContactDoesNotSpendInputAndCounterHitsAttackerOnce()
    {
        var defender = new GameObject("Defender");
        var attacker = new GameObject("Attacker");
        try
        {
            var health = defender.AddComponent<HealthModule>();
            var attackerHealth = attacker.AddComponent<HealthModule>();
            var parry = defender.AddComponent<KTH_ParryModule>();
            var boss = attacker.AddComponent<KTH_Boss>();
            parry.Press();

            var unblockable = new KTH_AttackBundle { baseDamage = 8, parryMode = DLJ_ParryMode.Unparryable };
            KTH_Throw.Judge(boss, unblockable, Vector3.right)();
            Assert.That(health.CurrentHealth, Is.EqualTo(42));

            var counter = new KTH_AttackBundle { baseDamage = 12, parryMode = DLJ_ParryMode.Counter };
            var hit = KTH_Throw.Judge(boss, counter, Vector3.right);
            hit();
            hit();
            Assert.That(health.CurrentHealth, Is.EqualTo(42));
            Assert.That(attackerHealth.CurrentHealth, Is.EqualTo(38));
        }
        finally
        {
            Object.DestroyImmediate(defender);
            Object.DestroyImmediate(attacker);
        }
    }

    [UnityTest]
    public IEnumerator ASecondContactNeedsAnotherPress()
    {
        var defender = new GameObject("Defender");
        try
        {
            var health = defender.AddComponent<HealthModule>();
            var parry = defender.AddComponent<KTH_ParryModule>();
            var attack = new KTH_AttackBundle
            {
                baseDamage = 20, inputCount = 2, perfectWindow = 0.04f, goodWindow = 0.04f
            };
            parry.Press();
            var incoming = parry.BeginBundle(attack, 1, Vector3.right);
            incoming.Hit();
            incoming.Hit();
            Assert.That(health.CurrentHealth, Is.EqualTo(50));

            yield return new WaitForSeconds(0.08f);
            yield return null;
            Assert.That(health.CurrentHealth, Is.EqualTo(35));
        }
        finally
        {
            Object.DestroyImmediate(defender);
        }
    }

    [Test]
    public void LateInputCanBlockButDisablingParryStillAppliesDamage()
    {
        var defender = new GameObject("Defender");
        try
        {
            var health = defender.AddComponent<HealthModule>();
            var parry = defender.AddComponent<KTH_ParryModule>();
            var attack = new KTH_AttackBundle();

            parry.BeginBundle(attack, 1, Vector3.right).Hit();
            parry.Press();
            Assert.That(health.CurrentHealth, Is.EqualTo(50));

            parry.BeginBundle(attack, 1, Vector3.right).Hit();
            parry.enabled = false;
            Assert.That(health.CurrentHealth, Is.EqualTo(40));
        }
        finally
        {
            Object.DestroyImmediate(defender);
        }
    }

    [Test]
    public void ImmediateContactUsesTheSameTimingAndResolvesWithoutWaiting()
    {
        var defender = new GameObject("Defender");
        try
        {
            var parry = defender.AddComponent<DLJ_Parry>();
            var profile = new DLJ_ParryProfile(DLJ_ParryMode.Block);
            DLJ_ParryGrade? grade = null;

            parry.Press();
            parry.JudgeContact(profile, false, result => grade = result);
            Assert.That(grade, Is.EqualTo(DLJ_ParryGrade.Perfect));

            parry.JudgeContact(profile, false, result => grade = result);
            Assert.That(grade, Is.EqualTo(DLJ_ParryGrade.Fail));
        }
        finally
        {
            Object.DestroyImmediate(defender);
        }
    }
}
