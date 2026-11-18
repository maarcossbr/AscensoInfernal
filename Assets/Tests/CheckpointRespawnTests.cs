
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class CheckpointRespawnTests
{
    private GameObject playerObject;
    private GameObject mineralManagerObject;

    private Rigidbody2D rb;
    private PlayerHealth health;
    private PlayerCheckpoint playerCheckpoint;
    private GestorMinerales gestorMinerales;

    private string escenaActual;

    [SetUp]
    public void SetUp()
    {
        escenaActual = SceneManager.GetActiveScene().name;

        CheckpointRespawn.CancelarRespawn();

        // Crear jugador
        playerObject = new GameObject("Player_Respawn_Test");
        playerObject.tag = "Player";

        rb = playerObject.AddComponent<Rigidbody2D>();
        health = playerObject.AddComponent<PlayerHealth>();
        playerCheckpoint = playerObject.AddComponent<PlayerCheckpoint>();
        playerObject.AddComponent<CheckpointRespawn>();

        // Crear gestor de minerales
        mineralManagerObject = new GameObject("GestorMinerales_Test");
        gestorMinerales =
            mineralManagerObject.AddComponent<GestorMinerales>();
    }

    [TearDown]
    public void TearDown()
    {
        CheckpointRespawn.CancelarRespawn();

        if (playerObject != null)
            Object.DestroyImmediate(playerObject);

        if (mineralManagerObject != null)
            Object.DestroyImmediate(mineralManagerObject);
    }

    private void InvocarStart()
    {
        MethodInfo method = typeof(CheckpointRespawn).GetMethod(
            "Start",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(method);

        method.Invoke(
            playerObject.GetComponent<CheckpointRespawn>(),
            null
        );
    }

    private object ObtenerCampoEstatico(string nombre)
    {
        FieldInfo field = typeof(CheckpointRespawn).GetField(
            nombre,
            BindingFlags.NonPublic | BindingFlags.Static
        );

        Assert.IsNotNull(field);

        return field.GetValue(null);
    }

    [Test]
    public void PrepararRespawn_StoresRespawnData()
    {
        Vector3 posicion = new Vector3(10f, 25f, 0f);

        CheckpointRespawn.PrepararRespawn(
            escenaActual,
            posicion,
            2,
            false,
            Vector2.zero,
            0f,
            null
        );

        Assert.AreEqual(
            escenaActual,
            ObtenerCampoEstatico("escenaGuardada")
        );

        Assert.AreEqual(
            posicion,
            ObtenerCampoEstatico("posicionGuardada")
        );

        Assert.AreEqual(
            2,
            ObtenerCampoEstatico("vidasGuardadas")
        );

        Assert.IsTrue(
            (bool)ObtenerCampoEstatico("respawnPendiente")
        );
    }

    [Test]
    public void PrepararRespawn_ClonesMineralArray()
    {
        string[] minerales = { "mineral_1", "mineral_2" };

        CheckpointRespawn.PrepararRespawn(
            escenaActual,
            Vector3.zero,
            3,
            false,
            Vector2.zero,
            0f,
            minerales
        );

        minerales[0] = "modificado";

        string[] guardados =
            (string[])ObtenerCampoEstatico("mineralesGuardados");

        Assert.AreEqual("mineral_1", guardados[0]);
        Assert.AreEqual("mineral_2", guardados[1]);
    }

    [Test]
    public void PrepararRespawn_NullMinerals_StoresEmptyArray()
    {
        CheckpointRespawn.PrepararRespawn(
            escenaActual,
            Vector3.zero,
            3,
            false,
            Vector2.zero,
            0f,
            null
        );

        string[] guardados =
            (string[])ObtenerCampoEstatico("mineralesGuardados");

        Assert.IsNotNull(guardados);
        Assert.AreEqual(0, guardados.Length);
    }

    [Test]
    public void TryGetLavaState_WithSavedLava_ReturnsTrue()
    {
        Vector2 posicionLava = new Vector2(0f, 15f);
        float velocidadLava = 2.5f;

        CheckpointRespawn.PrepararRespawn(
            escenaActual,
            Vector3.zero,
            3,
            true,
            posicionLava,
            velocidadLava,
            null
        );

        bool resultado = CheckpointRespawn.TryGetLavaState(
            out Vector2 posicion,
            out float velocidad
        );

        Assert.IsTrue(resultado);
        Assert.AreEqual(posicionLava, posicion);
        Assert.AreEqual(velocidadLava, velocidad);
    }

    [Test]
    public void TryGetLavaState_WithoutSavedLava_ReturnsFalse()
    {
        CheckpointRespawn.PrepararRespawn(
            escenaActual,
            Vector3.zero,
            3,
            false,
            Vector2.zero,
            0f,
            null
        );

        bool resultado = CheckpointRespawn.TryGetLavaState(
            out Vector2 posicion,
            out float velocidad
        );

        Assert.IsFalse(resultado);
        Assert.AreEqual(Vector2.zero, posicion);
        Assert.AreEqual(0f, velocidad);
    }

    [Test]
    public void CancelarRespawn_ClearsPendingState()
    {
        CheckpointRespawn.PrepararRespawn(
            escenaActual,
            Vector3.zero,
            3,
            true,
            new Vector2(0f, 10f),
            2f,
            null
        );

        CheckpointRespawn.CancelarRespawn();

        Assert.IsFalse(
            (bool)ObtenerCampoEstatico("respawnPendiente")
        );

        Assert.IsFalse(
            (bool)ObtenerCampoEstatico("lavaGuardada")
        );
    }

    [Test]
    public void MineralGuardado_ExistingMineral_ReturnsTrue()
    {
        CheckpointRespawn.PrepararRespawn(
            escenaActual,
            Vector3.zero,
            3,
            false,
            Vector2.zero,
            0f,
            new[] { "mineral_1", "mineral_2" }
        );

        Assert.IsTrue(
            CheckpointRespawn.MineralGuardado("mineral_1")
        );
    }

    [Test]
    public void MineralGuardado_UnknownMineral_ReturnsFalse()
    {
        CheckpointRespawn.PrepararRespawn(
            escenaActual,
            Vector3.zero,
            3,
            false,
            Vector2.zero,
            0f,
            new[] { "mineral_1", "mineral_2" }
        );

        Assert.IsFalse(
            CheckpointRespawn.MineralGuardado("mineral_3")
        );
    }

    [Test]
    public void Start_RestoresPlayerPositionAndHealth()
    {
        Vector3 posicion = new Vector3(12f, 30f, 0f);

        CheckpointRespawn.PrepararRespawn(
            escenaActual,
            posicion,
            2,
            false,
            Vector2.zero,
            0f,
            null
        );

        rb.position = Vector2.zero;
        rb.linearVelocity = new Vector2(5f, 5f);
        rb.angularVelocity = 10f;

        InvocarStart();

        Assert.AreEqual(
            new Vector2(posicion.x, posicion.y),
            rb.position
        );

        Assert.AreEqual(Vector2.zero, rb.linearVelocity);
        Assert.AreEqual(0f, rb.angularVelocity);
        Assert.AreEqual(2, health.CurrentHealth);

        Assert.IsTrue(playerCheckpoint.HasCheckpoint);

        Assert.AreEqual(
            posicion,
            playerCheckpoint.LastCheckpointPosition
        );

        Assert.IsFalse(
            (bool)ObtenerCampoEstatico("respawnPendiente")
        );
    }

    [Test]
    public void Start_RestoresCollectedMinerals()
    {
        CheckpointRespawn.PrepararRespawn(
            escenaActual,
            new Vector3(5f, 20f, 0f),
            3,
            false,
            Vector2.zero,
            0f,
            new[] { "mineral_1", "mineral_2" }
        );

        InvocarStart();

        Assert.AreEqual(1, gestorMinerales.MineralesRestantes);
        Assert.IsFalse(gestorMinerales.TieneTodos);
    }
}
