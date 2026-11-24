
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class LavaControllerTests
{
    private GameObject lavaObject;
    private GameObject playerObject;
    private GameObject otherObject;

    private LavaController lava;
    private PlayerHealth health;

    [SetUp]
    public void SetUp()
    {
        // Crear lava
        lavaObject = new GameObject("Lava_Test");

        lavaObject.AddComponent<Rigidbody2D>();
        lava = lavaObject.AddComponent<LavaController>();

        // Crear jugador
        playerObject = new GameObject("Player_Test");
        playerObject.tag = "Player";

        playerObject.AddComponent<BoxCollider2D>();
        health = playerObject.AddComponent<PlayerHealth>();

        // Crear objeto que no es el jugador
        otherObject = new GameObject("Other_Test");
        otherObject.AddComponent<BoxCollider2D>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(lavaObject);
        Object.DestroyImmediate(playerObject);
        Object.DestroyImmediate(otherObject);
    }

    // Permite ejecutar metodos privados de LavaController.
    private void InvokePrivateMethod(string methodName, params object[] parameters)
    {
        MethodInfo method = typeof(LavaController).GetMethod(
            methodName,
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(
            method,
            "No se ha encontrado el metodo " + methodName
        );

        method.Invoke(lava, parameters);
    }

    [Test]
    public void Start_InitializesCurrentSpeed()
    {
        lava.baseSpeed = 3f;
        lava.currentSpeed = 0f;

        InvokePrivateMethod("Start");

        Assert.AreEqual(3f, lava.currentSpeed);
    }

    [Test]
    public void FixedUpdate_IncreasesCurrentSpeed()
    {
        lava.acceleration = 0.5f;
        lava.currentSpeed = 2f;

        InvokePrivateMethod("FixedUpdate");

        float expectedSpeed =
            2f + 0.5f * Time.fixedDeltaTime;

        Assert.AreEqual(
            expectedSpeed,
            lava.currentSpeed,
            0.0001f
        );
    }

    [Test]
    public void OnTriggerEnter2D_KillsPlayer()
    {
        int deathEvents = 0;

        health.Died += () => deathEvents++;

        Collider2D playerCollider =
            playerObject.GetComponent<Collider2D>();

        InvokePrivateMethod(
            "OnTriggerEnter2D",
            playerCollider
        );

        Assert.IsTrue(health.IsDead);
        Assert.AreEqual(0, health.CurrentHealth);
        Assert.AreEqual(1, deathEvents);
    }

    [Test]
    public void OnTriggerEnter2D_IgnoresObjectsWithoutPlayerTag()
    {
        Collider2D otherCollider =
            otherObject.GetComponent<Collider2D>();

        InvokePrivateMethod(
            "OnTriggerEnter2D",
            otherCollider
        );

        Assert.IsFalse(health.IsDead);
        Assert.AreEqual(3, health.CurrentHealth);
    }

    [Test]
    public void OnTriggerEnter2D_DoesNotRepeatDeathAfterFirstCollision()
    {
        int deathEvents = 0;

        health.Died += () => deathEvents++;

        Collider2D playerCollider =
            playerObject.GetComponent<Collider2D>();

        // Primera colision
        InvokePrivateMethod(
            "OnTriggerEnter2D",
            playerCollider
        );

        // Segunda colision
        InvokePrivateMethod(
            "OnTriggerEnter2D",
            playerCollider
        );

        Assert.IsTrue(health.IsDead);
        Assert.AreEqual(0, health.CurrentHealth);
        Assert.AreEqual(1, deathEvents);
    }
}
