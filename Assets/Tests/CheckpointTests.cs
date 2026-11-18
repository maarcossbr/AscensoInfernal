
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CheckpointTests
{
    private GameObject checkpointObject;
    private GameObject playerObject;
    private GameObject respawnObject;

    private Checkpoint checkpoint;
    private PlayerCheckpoint playerCheckpoint;
    private Collider2D playerCollider;

    
    [SetUp]
    public void SetUp()
    {
        // Crear el objeto del checkpoint
        checkpointObject = new GameObject("Checkpoint_Test");

        // Anadir primero el collider
        BoxCollider2D checkpointCollider =
            checkpointObject.AddComponent<BoxCollider2D>();

        checkpointCollider.isTrigger = true;

        // Anadir el componente Checkpoint
        checkpoint = checkpointObject.AddComponent<Checkpoint>();

        // Crear el punto de reaparicion
        respawnObject = new GameObject("RespawnPoint_Test");

        // Crear el jugador
        playerObject = new GameObject("Player_Test");
        playerObject.tag = "Player";

        playerCollider = playerObject.AddComponent<BoxCollider2D>();

        // Anadir los componentes del jugador
        playerObject.AddComponent<PlayerHealth>();

        playerCheckpoint =
            playerObject.AddComponent<PlayerCheckpoint>();
    }


    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(checkpointObject);
        Object.DestroyImmediate(playerObject);
        Object.DestroyImmediate(respawnObject);
    }

    private void AsignarRespawnPoint(Transform posicion)
    {
        FieldInfo field = typeof(Checkpoint).GetField(
            "respawnPoint",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(
            field,
            "No se ha encontrado el campo respawnPoint."
        );

        field.SetValue(checkpoint, posicion);
    }

    private void SimularEntrada()
    {
        MethodInfo method = typeof(Checkpoint).GetMethod(
            "OnTriggerEnter2D",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(
            method,
            "No se ha encontrado OnTriggerEnter2D."
        );

        method.Invoke(checkpoint, new object[] { playerCollider });
    }

    private void EsperarActivacionSinLava()
    {
        LogAssert.Expect(
            LogType.Warning,
            "No hay ninguna lava asignada al checkpoint."
        );

        LogAssert.Expect(
            LogType.Log,
            "Checkpoint activado con 3 corazones. Lava guardada: False"
        );
    }

    [Test]
    public void OnTriggerEnter2D_PlayerEnters_ActivatesCheckpoint()
    {
        Vector3 posicionEsperada =
            new Vector3(10f, 25f, 0f);

        respawnObject.transform.position = posicionEsperada;

        AsignarRespawnPoint(respawnObject.transform);

        EsperarActivacionSinLava();

        SimularEntrada();

        Assert.IsTrue(
            playerCheckpoint.HasCheckpoint,
            "El checkpoint deberia haberse activado."
        );

        Assert.AreEqual(
            posicionEsperada,
            playerCheckpoint.LastCheckpointPosition
        );
    }

    [Test]
    public void OnTriggerEnter2D_WithoutRespawnPoint_UsesCheckpointPosition()
    {
        Vector3 posicionEsperada =
            new Vector3(5f, 40f, 0f);

        checkpointObject.transform.position = posicionEsperada;

        AsignarRespawnPoint(null);

        EsperarActivacionSinLava();

        SimularEntrada();

        Assert.IsTrue(playerCheckpoint.HasCheckpoint);

        Assert.AreEqual(
            posicionEsperada,
            playerCheckpoint.LastCheckpointPosition
        );
    }

    [Test]
    public void OnTriggerEnter2D_NonPlayerObject_DoesNotActivateCheckpoint()
    {
        GameObject otherObject = new GameObject("OtherObject_Test");

        try
        {
            Collider2D otherCollider =
                otherObject.AddComponent<BoxCollider2D>();

            MethodInfo method = typeof(Checkpoint).GetMethod(
                "OnTriggerEnter2D",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            Assert.IsNotNull(method);

            method.Invoke(
                checkpoint,
                new object[] { otherCollider }
            );

            Assert.IsFalse(
                playerCheckpoint.HasCheckpoint,
                "Un objeto que no es el jugador no debe activar el checkpoint."
            );
        }
        finally
        {
            Object.DestroyImmediate(otherObject);
        }
    }

    [Test]
    public void OnTriggerEnter2D_PlayerWithoutPlayerCheckpoint_DoesNothing()
    {
        GameObject playerWithoutCheckpoint =
            new GameObject("PlayerWithoutCheckpoint_Test");

        try
        {
            playerWithoutCheckpoint.tag = "Player";

            Collider2D otherCollider =
                playerWithoutCheckpoint.AddComponent<BoxCollider2D>();

            MethodInfo method = typeof(Checkpoint).GetMethod(
                "OnTriggerEnter2D",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            Assert.IsNotNull(method);

            method.Invoke(
                checkpoint,
                new object[] { otherCollider }
            );

            Assert.IsFalse(
                playerCheckpoint.HasCheckpoint,
                "Un jugador distinto no debe activar el checkpoint del jugador de prueba."
            );
        }
        finally
        {
            Object.DestroyImmediate(playerWithoutCheckpoint);
        }
    }
}
