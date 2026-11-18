
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerCheckpointLavaTests
{
    private GameObject player;
    private GameObject lavaObject;

    private PlayerHealth health;
    private PlayerCheckpoint checkpoint;
    private LavaController lava;

    private Rigidbody2D lavaRb;

    [SetUp]
    public void SetUp()
    {
        // Crear jugador
        player = new GameObject("Player_LavaTest");

        BoxCollider2D playerCollider = player.AddComponent<BoxCollider2D>();
        playerCollider.size = new Vector2(1f, 2f);

        health = player.AddComponent<PlayerHealth>();
        checkpoint = player.AddComponent<PlayerCheckpoint>();

        // Crear lava
        lavaObject = new GameObject("Lava_Test");

        lavaRb = lavaObject.AddComponent<Rigidbody2D>();
        lavaRb.bodyType = RigidbodyType2D.Kinematic;

        BoxCollider2D lavaCollider = lavaObject.AddComponent<BoxCollider2D>();
        lavaCollider.size = new Vector2(4f, 2f);

        lava = lavaObject.AddComponent<LavaController>();

        // Asignar la lava al checkpoint
        FieldInfo lavaField = typeof(PlayerCheckpoint).GetField(
            "lava",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(lavaField, "No se ha encontrado el campo lava.");

        lavaField.SetValue(checkpoint, lava);

        // Sincronizar los colliders
        Physics2D.SyncTransforms();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(player);
        Object.DestroyImmediate(lavaObject);
    }

    private void ExpectSuccessfulActivation()
    {
        LogAssert.Expect(
            LogType.Log,
            "Checkpoint activado con 3 corazones. Lava guardada: True"
        );
    }

    [Test]
    public void ActivateCheckpoint_SavesLavaPosition()
    {
        lava.currentSpeed = 2f;
        lava.acceleration = 0.5f;

        lavaRb.position = new Vector2(0f, 0f);

        Physics2D.SyncTransforms();

        ExpectSuccessfulActivation();

        checkpoint.ActivateCheckpoint(new Vector3(0f, 10f, 0f));

        Assert.IsTrue(checkpoint.HasLavaState);

        // Distancia:
        // 2 * 8 + 0.5 * 0.5 * 8 * 8 = 32
        //
        // Pies del jugador: 9
        // Offset de la lava: 1
        // Posicion segura: 9 - 32 - 1 = -24

        Assert.AreEqual(
            -24f,
            checkpoint.LavaPositionAtCheckpoint.y,
            0.01f
        );
    }

    [Test]
    public void ActivateCheckpoint_SavesLavaSpeed()
    {
        lava.currentSpeed = 7f;
        lava.acceleration = 0.5f;

        Physics2D.SyncTransforms();

        ExpectSuccessfulActivation();

        checkpoint.ActivateCheckpoint(new Vector3(0f, 10f, 0f));

        Assert.AreEqual(7f, checkpoint.LavaSpeedAtCheckpoint);
    }

    [Test]
    public void ActivateCheckpoint_DoesNotMoveLavaUpwards()
    {
        lava.currentSpeed = 0f;
        lava.acceleration = 0f;

        lavaRb.position = new Vector2(0f, 5f);

        Physics2D.SyncTransforms();

        ExpectSuccessfulActivation();

        checkpoint.ActivateCheckpoint(new Vector3(0f, 10f, 0f));

        Assert.AreEqual(
            5f,
            checkpoint.LavaPositionAtCheckpoint.y,
            0.01f
        );
    }

    [Test]
    public void ActivateCheckpoint_SavesLavaHorizontalPosition()
    {
        lava.currentSpeed = 2f;
        lava.acceleration = 0.5f;

        lavaRb.position = new Vector2(12f, 0f);

        Physics2D.SyncTransforms();

        ExpectSuccessfulActivation();

        checkpoint.ActivateCheckpoint(new Vector3(0f, 10f, 0f));

        Assert.AreEqual(
            12f,
            checkpoint.LavaPositionAtCheckpoint.x,
            0.01f
        );
    }

    [Test]
    public void ActivateCheckpoint_ClampsNegativeSpeedAndAcceleration()
    {
        lava.currentSpeed = -5f;
        lava.acceleration = -2f;

        Physics2D.SyncTransforms();

        ExpectSuccessfulActivation();

        checkpoint.ActivateCheckpoint(new Vector3(0f, 10f, 0f));

        // Los valores negativos se consideran cero
        // para calcular la distancia de seguridad.

        Assert.AreEqual(
            0f,
            checkpoint.LavaPositionAtCheckpoint.y,
            0.01f
        );

        // Se guarda la velocidad original, no la corregida.
        Assert.AreEqual(-5f, checkpoint.LavaSpeedAtCheckpoint);
    }
}
