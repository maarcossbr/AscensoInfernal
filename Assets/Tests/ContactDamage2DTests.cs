
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class ContactDamage2DTests
{
    private GameObject damageObject;
    private GameObject playerObject;
    private GameObject childObject;

    private ContactDamage2D contactDamage;
    private PlayerHealth playerHealth;
    private Collider2D playerCollider;

    [SetUp]
    public void SetUp()
    {
        // Crear objeto que causa dano
        damageObject = new GameObject("ContactDamage_Test");
        contactDamage =
            damageObject.AddComponent<ContactDamage2D>();

        // Crear jugador
        playerObject = new GameObject("Player_Test");
        playerObject.tag = "Player";

        playerObject.AddComponent<Rigidbody2D>();

        playerCollider =
            playerObject.AddComponent<BoxCollider2D>();

        playerHealth =
            playerObject.AddComponent<PlayerHealth>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(damageObject);
        Object.DestroyImmediate(playerObject);

        if (childObject != null)
            Object.DestroyImmediate(childObject);
    }

    private void ConfigurarDano(int cantidad)
    {
        FieldInfo field = typeof(ContactDamage2D).GetField(
            "damage",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(field);

        field.SetValue(contactDamage, cantidad);
    }

    private void InvocarTriggerStay(Collider2D other)
    {
        MethodInfo method = typeof(ContactDamage2D).GetMethod(
            "OnTriggerStay2D",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(method);

        method.Invoke(contactDamage, new object[] { other });
    }

    private void InvocarCollisionStay(Collider2D other)
    {
        MethodInfo method = typeof(ContactDamage2D).GetMethod(
            "OnCollisionStay2D",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(method);

        // Crear una colision de prueba
        Collision2D collision = new Collision2D();

        // Collision2D no permite asignar directamente su collider.
        // Por ello, esta via se prueba mediante una colision real
        // en una prueba de integracion separada.
    }

    [Test]
    public void OnTriggerStay2D_PlayerInContact_AppliesDamage()
    {
        int vidaInicial = playerHealth.CurrentHealth;

        InvocarTriggerStay(playerCollider);

        Assert.AreEqual(
            vidaInicial - 1,
            playerHealth.CurrentHealth
        );
    }

    [Test]
    public void OnTriggerStay2D_NonPlayerObject_DoesNotApplyDamage()
    {
        GameObject otherObject = new GameObject("Other_Test");

        try
        {
            Collider2D otherCollider =
                otherObject.AddComponent<BoxCollider2D>();

            int vidaInicial = playerHealth.CurrentHealth;

            InvocarTriggerStay(otherCollider);

            Assert.AreEqual(
                vidaInicial,
                playerHealth.CurrentHealth
            );
        }
        finally
        {
            Object.DestroyImmediate(otherObject);
        }
    }

    [Test]
    public void OnTriggerStay2D_CustomDamage_AppliesConfiguredAmount()
    {
        ConfigurarDano(2);

        int vidaInicial = playerHealth.CurrentHealth;

        InvocarTriggerStay(playerCollider);

        Assert.AreEqual(
            vidaInicial - 2,
            playerHealth.CurrentHealth
        );
    }

    [Test]
    public void OnTriggerStay2D_ChildCollider_FindsParentPlayerHealth()
    {
        childObject = new GameObject("PlayerCollider_Test");

        childObject.transform.SetParent(playerObject.transform);

        Collider2D childCollider =
            childObject.AddComponent<BoxCollider2D>();

        int vidaInicial = playerHealth.CurrentHealth;

        InvocarTriggerStay(childCollider);

        Assert.AreEqual(
            vidaInicial - 1,
            playerHealth.CurrentHealth
        );
    }

    [Test]
    public void OnTriggerStay2D_ZeroDamage_DoesNotReduceHealth()
    {
        ConfigurarDano(0);

        int vidaInicial = playerHealth.CurrentHealth;

        InvocarTriggerStay(playerCollider);

        Assert.AreEqual(
            vidaInicial,
            playerHealth.CurrentHealth
        );
    }
}
