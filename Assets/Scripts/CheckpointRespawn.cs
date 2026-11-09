
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
    private static string[] mineralesGuardados = System.Array.Empty<string>();

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
        mineralesGuardados = System.Array.Empty<string>();
    }


    public static void PrepararRespawn(string escena, Vector3 posicion, int vidas, bool tieneLava, Vector2 posicionLava, float velocidadLava, string[] minerales)
    {
        escenaGuardada = escena;
        posicionGuardada = posicion;
        vidasGuardadas = vidas;

        lavaGuardada = tieneLava;
        posicionLavaGuardada = posicionLava;
        velocidadLavaGuardada = velocidadLava;

        mineralesGuardados = minerales != null
            ? (string[])minerales.Clone()
            : System.Array.Empty<string>();

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


        GestorMinerales gestor = FindFirstObjectByType<GestorMinerales>();

        if (gestor != null)
        {
            foreach (string mineral in mineralesGuardados)
            {
                gestor.RecogerMineral();
            }
        }
        else if (mineralesGuardados.Length > 0)
        {
            Debug.LogWarning(
                "No se encontró GestorMinerales al reaparecer."
            );
        }

        GetComponent<PlayerCheckpoint>()
            .ActivateCheckpoint(posicionGuardada);

        Debug.Log("Jugador reaparecido en checkpoint.");
    }


    public static bool MineralGuardado(string id)
    {
        if (!respawnPendiente ||
            SceneManager.GetActiveScene().name != escenaGuardada)
            return false;

        foreach (string mineral in mineralesGuardados)
        {
            if (mineral == id)
                return true;
        }

        return false;
    }

}
