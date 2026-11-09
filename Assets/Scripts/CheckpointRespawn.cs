
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(100)]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerCheckpoint))]
public class CheckpointRespawn : MonoBehaviour
{
    private static bool respawnPendiente;
    private static string escenaGuardada;
    private static Vector3 posicionGuardada;
    private static int vidasGuardadas;

    private static bool lavaGuardada;
    private static Vector2 posicionLavaGuardada;
    private static float velocidadLavaGuardada;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ReiniciarDatos()
    {
        CancelarRespawn();
        escenaGuardada = null;
        posicionGuardada = Vector3.zero;
        vidasGuardadas = 0;
        posicionLavaGuardada = Vector2.zero;
        velocidadLavaGuardada = 0f;
    }

    public static void PrepararRespawn(
        string escena,
        Vector3 posicion,
        int vidas,
        bool tieneLava,
        Vector2 posicionLava,
        float velocidadLava)
    {
        escenaGuardada = escena;
        posicionGuardada = posicion;
        vidasGuardadas = vidas;

        lavaGuardada = tieneLava;
        posicionLavaGuardada = posicionLava;
        velocidadLavaGuardada = velocidadLava;

        respawnPendiente = true;
    }

    public static bool TryGetLavaState(
        out Vector2 posicion,
        out float velocidad)
    {
        posicion = posicionLavaGuardada;
        velocidad = velocidadLavaGuardada;

        return respawnPendiente &&
               lavaGuardada &&
               SceneManager.GetActiveScene().name == escenaGuardada;
    }

    public static void CancelarRespawn()
    {
        respawnPendiente = false;
        lavaGuardada = false;
    }

    private void Start()
    {
        if (!respawnPendiente ||
            SceneManager.GetActiveScene().name != escenaGuardada)
            return;

        respawnPendiente = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        rb.position = new Vector2(
            posicionGuardada.x,
            posicionGuardada.y
        );

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        Physics2D.SyncTransforms();

        GetComponent<PlayerHealth>()
            .RestoreHealth(vidasGuardadas);

        GetComponent<PlayerCheckpoint>()
            .ActivateCheckpoint(posicionGuardada);

        Debug.Log("Jugador reaparecido en checkpoint.");
    }
}
