
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerHealthTests
{
    private GameObject player;
    private PlayerHealth health;

    [SetUp]
    public void SetUp()
    {
        player = new GameObject("Player_Test");
        health = player.AddComponent<PlayerHealth>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(player);
    }

    [Test]
    public void TakeDamage_ReduceHealth()
    {
        bool result = health.TakeDamage(1);

        Assert.IsTrue(result);
        Assert.AreEqual(2, health.CurrentHealth);
    }

    [Test]
    public void TakeDamage_NoReduceHealthWhenAmountIsZero()
    {
        bool result = health.TakeDamage(0);

        Assert.IsFalse(result);
        Assert.AreEqual(3, health.CurrentHealth);
    }

    [Test]
    public void TakeDamage_NoReduceHealthWhenAmountIsNegative()
    {
        bool result = health.TakeDamage(-1);

        Assert.IsFalse(result);
        Assert.AreEqual(3, health.CurrentHealth);
    }

    [Test]
    public void TakeDamage_HealthCannotBeNegative()
    {
        health.TakeDamage(10);

        Assert.AreEqual(0, health.CurrentHealth);
        Assert.IsTrue(health.IsDead);
    }

    [Test]
    public void TakeDamage_PreventsDamageDuringInvulnerability()
    {
        health.TakeDamage(1);

        bool result = health.TakeDamage(1);

        Assert.IsFalse(result);
        Assert.AreEqual(2, health.CurrentHealth);
        Assert.IsTrue(health.IsInvulnerable);
    }

    [UnityTest]
    public IEnumerator TakeDamage_InvulnerabilityEndsAfterDuration()
    {
        health.TakeDamage(1);

        yield return new WaitForSeconds(1.1f);

        Assert.IsFalse(health.IsInvulnerable);

        bool result = health.TakeDamage(1);

        Assert.IsTrue(result);
        Assert.AreEqual(1, health.CurrentHealth);
    }

    [Test]
    public void KillInstantly_SetsHealthToZero()
    {
        health.KillInstantly();

        Assert.AreEqual(0, health.CurrentHealth);
        Assert.IsTrue(health.IsDead);
    }

    [Test]
    public void KillInstantly_InvokesDiedOnlyOnce()
    {
        int deathCount = 0;

        health.Died += () => deathCount++;

        health.KillInstantly();
        health.KillInstantly();

        Assert.AreEqual(1, deathCount);
    }

    [Test]
    public void RestoreFullHealth_RestoresMaximumHealth()
    {
        health.TakeDamage(1);
        health.RestoreFullHealth();

        Assert.AreEqual(3, health.CurrentHealth);
    }

    [Test]
    public void RestoreHealth_RestoresSpecifiedAmount()
    {
        health.RestoreHealth(2);

        Assert.AreEqual(2, health.CurrentHealth);
    }

    [Test]
    public void RestoreHealth_CannotExceedMaximumHealth()
    {
        health.RestoreHealth(100);

        Assert.AreEqual(3, health.CurrentHealth);
    }

    [Test]
    public void RestoreHealth_CannotBeLowerThanOne()
    {
        health.RestoreHealth(0);

        Assert.AreEqual(1, health.CurrentHealth);
    }

    [Test]
    public void HealthChanged_IsInvokedWhenHealthChanges()
    {
        int receivedHealth = -1;
        int receivedMaxHealth = -1;

        health.HealthChanged += (current, maximum) =>
        {
            receivedHealth = current;
            receivedMaxHealth = maximum;
        };

        health.TakeDamage(1);

        Assert.AreEqual(2, receivedHealth);
        Assert.AreEqual(3, receivedMaxHealth);
    }

    [Test]
    public void Died_IsInvokedWhenPlayerDies()
    {
        bool diedEventReceived = false;

        health.Died += () => diedEventReceived = true;

        health.KillInstantly();

        Assert.IsTrue(diedEventReceived);
    }
}
