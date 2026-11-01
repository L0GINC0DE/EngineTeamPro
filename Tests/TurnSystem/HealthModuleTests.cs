using System.Collections.Generic;
using Agents;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;

public class HealthModuleTests
{
    private GameObject owner;
    private HealthModule health;

    [SetUp]
    public void SetUp()
    {
        owner = new GameObject("Health test");
        health = owner.AddComponent<HealthModule>();
        health.OnHitEvent = new UnityEvent();
    }

    [TearDown]
    public void TearDown() => Object.DestroyImmediate(owner);

    [Test]
    public void ResetHealthRestoresHealthWithoutHitFeedback()
    {
        health.TakeDamage(10);
        int hits = 0;
        health.OnHitEvent.AddListener(() => hits++);
        health.ResetHealth();
        Assert.That(health.CurrentHealth, Is.EqualTo(health.MaxHealth));
        Assert.That(hits, Is.Zero, "Restoring health is not a hit.");
    }

    [Test]
    public void LethalDamageRaisesDeathOnceAndClampsHealth()
    {
        int deaths = 0;
        var changes = new List<int>();
        health.OnDead += () => deaths++;
        health.OnHealthChanged += (previous, next) => changes.Add(next);
        health.TakeDamage(int.MaxValue);
        health.TakeDamage(10);
        Assert.That(health.CurrentHealth, Is.Zero);
        Assert.That(deaths, Is.EqualTo(1));
        Assert.That(changes, Is.EqualTo(new[] { 0 }));
    }

    [Test]
    public void NonPositiveDamageDoesNotHealOrRaiseHit()
    {
        int hits = 0;
        health.OnHitEvent.AddListener(() => hits++);
        health.TakeDamage(-100);
        health.TakeDamage(0);
        Assert.That(health.CurrentHealth, Is.EqualTo(health.MaxHealth));
        Assert.That(hits, Is.Zero);
    }
}
