
using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class MeteoritoTests
{
    private GameObject meteoritoObject;
    private GameObject playerObject;
    private GameObject otherObject;

    private Meteorito meteorito;
    private Rigidbody2D rb;
    private PlayerHealth health;

    [SetUp]
    public void SetUp()
    {
        // Crear meteorito
        meteoritoObject = new GameObject("Meteorito_Test");

        rb = meteoritoObject.AddComponent<Rigidbody2D>();
        meteorito = meteoritoObject.AddComponent<Meteorito>();

        // Crear jugador
        playerObject = new GameObject("Player_Test");

        playerObject.AddComponent<BoxCollider2D>();
        health = playerObject.AddComponent<PlayerHealth>();

        // Crear objeto que no es el jugador
        otherObject = new GameObject("Other_Test");
        otherObject.AddComponent<BoxCollider2D>();
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(meteoritoObject);
        UnityEngine.Object.DestroyImmediate(playerObject);
        UnityEngine.Object.DestroyImmediate(otherObject);
    }

    // Permite ejecutar metodos privados.
    private void InvokePrivateMethod(string methodName, params object[] parameters)
    {
        MethodInfo method = typeof(Meteorito).GetMethod(
            methodName,
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(
            method,
            "No se ha encontrado el metodo " + methodName
        );

        method.Invoke(meteorito, parameters);
    }

    // Permite consultar campos privados.
    private object GetPrivateField(string fieldName)
    {
        FieldInfo field = typeof(Meteorito).GetField(
            fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(
            field,
            "No se ha encontrado el campo " + fieldName
        );

        return field.GetValue(meteorito);
    }

    [Test]
    public void Activar_SetsFallingVelocity()
    {
        meteorito.Activar(9f, 1, null);

        Assert.AreEqual(
            new Vector2(0f, -9f),
            rb.linearVelocity
        );
    }

    [Test]
    public void Activar_ClampsMinimumSpeedAndDamage()
    {
        meteorito.Activar(-5f, -2, null);

        Assert.AreEqual(
            new Vector2(0f, -0.1f),
            rb.linearVelocity
        );

        Assert.AreEqual(1, GetPrivateField("dano"));
    }

    [Test]
    public void OnTriggerEnter2D_DamagesPlayerAndReturnsMeteorite()
    {
        meteorito.Activar(9f, 2, (m) =>
        {
            Assert.AreSame(meteorito, m);
        });

        int returnedCount = 0;

        meteorito.Activar(9f, 2, (m) =>
        {
            returnedCount++;
        });

        Collider2D playerCollider =
            playerObject.GetComponent<Collider2D>();

        InvokePrivateMethod(
            "OnTriggerEnter2D",
            playerCollider
        );

        Assert.AreEqual(1, health.CurrentHealth);
        Assert.AreEqual(1, returnedCount);
        Assert.AreEqual(Vector2.zero, rb.linearVelocity);
    }

    [Test]
    public void OnTriggerEnter2D_IgnoresObjectsWithoutPlayerHealth()
    {
        meteorito.Activar(9f, 1, null);

        Collider2D otherCollider =
            otherObject.GetComponent<Collider2D>();

        InvokePrivateMethod(
            "OnTriggerEnter2D",
            otherCollider
        );

        Assert.AreEqual(3, health.CurrentHealth);
        Assert.IsTrue(meteoritoObject.activeSelf);
        Assert.AreEqual(
            new Vector2(0f, -9f),
            rb.linearVelocity
        );
    }

    [Test]
    public void Update_ReturnsMeteoriteWhenTimeExpires()
    {
        int returnedCount = 0;

        meteorito.Activar(9f, 1, (m) =>
        {
            returnedCount++;
        });

        FieldInfo timeField = typeof(Meteorito).GetField(
            "tiempoRestante",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(timeField);
        timeField.SetValue(meteorito, 0f);

        InvokePrivateMethod("Update");

        Assert.AreEqual(1, returnedCount);
        Assert.AreEqual(Vector2.zero, rb.linearVelocity);
    }

    [Test]
    public void OnDisable_ResetsVelocity()
    {
        meteorito.Activar(9f, 1, null);

        rb.linearVelocity = new Vector2(5f, 5f);

        meteoritoObject.SetActive(false);

        Assert.AreEqual(Vector2.zero, rb.linearVelocity);
    }
}
