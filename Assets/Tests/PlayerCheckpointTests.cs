
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerCheckpointTests
{
    private GameObject player;
    private PlayerHealth health;
    private PlayerCheckpoint checkpoint;

    [SetUp]
    public void SetUp()
    {
        player = new GameObject("PlayerCheckpoint_Test");

        health = player.AddComponent<PlayerHealth>();
        checkpoint = player.AddComponent<PlayerCheckpoint>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(player);
    }

    [Test]
    public void ActivateCheckpoint_SavesPosition()
    {
        Vector3 position = new Vector3(5f, 10f, 0f);

        ExpectActivationWithoutLava(3);

        checkpoint.ActivateCheckpoint(position);

        Assert.IsTrue(checkpoint.HasCheckpoint);
        Assert.AreEqual(position, checkpoint.LastCheckpointPosition);
    }

    [Test]
    public void ActivateCheckpoint_SavesCurrentHealth()
    {
        health.TakeDamage(1);

        ExpectActivationWithoutLava(2);

        checkpoint.ActivateCheckpoint(new Vector3(3f, 8f, 0f));

        Assert.AreEqual(2, checkpoint.HealthAtCheckpoint);
    }

    [Test]
    public void ActivateCheckpoint_SavesCollectedMinerals()
    {
        ExpectActivationWithoutLava(3);

        checkpoint.ActivateCheckpoint(new Vector3(2f, 5f, 0f));

        CollectionAssert.AreEqual(
            MineralCheckpointId.ObtenerRecogidos(),
            checkpoint.MineralesEnCheckpoint
        );
    }

    [Test]
    public void ActivateCheckpoint_DoesNotActivateWhenPlayerIsDead()
    {
        health.KillInstantly();

        checkpoint.ActivateCheckpoint(new Vector3(5f, 10f, 0f));

        Assert.IsFalse(checkpoint.HasCheckpoint);
        Assert.AreEqual(Vector3.zero, checkpoint.LastCheckpointPosition);
    }

    [Test]
    public void ActivateCheckpoint_WithoutLava_DoesNotSaveLavaState()
    {
        ExpectActivationWithoutLava(3);

        checkpoint.ActivateCheckpoint(new Vector3(5f, 10f, 0f));

        Assert.IsTrue(checkpoint.HasCheckpoint);
        Assert.IsFalse(checkpoint.HasLavaState);
    }

    private void ExpectActivationWithoutLava(int expectedHealth)
    {
        LogAssert.Expect(
            LogType.Warning,
            "No hay ninguna lava asignada al checkpoint."
        );

        LogAssert.Expect(
            LogType.Log,
            "Checkpoint activado con " + expectedHealth +
            " corazones. Lava guardada: False"
        );
    }
}
