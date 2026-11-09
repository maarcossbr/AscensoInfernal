
using UnityEngine;

[DefaultExecutionOrder(50)]
[RequireComponent(typeof(LavaController))]
[RequireComponent(typeof(Rigidbody2D))]
public class LavaCheckpointReset : MonoBehaviour
{
    private void Start()
    {
        if (!CheckpointRespawn.TryGetLavaState(
            out Vector2 posicion,
            out float velocidad))
            return;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        LavaController lava = GetComponent<LavaController>();

        rb.position = posicion;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        lava.currentSpeed = velocidad;

        Physics2D.SyncTransforms();

        Debug.Log(
            $"Lava restaurada en {posicion}, " +
            $"velocidad: {velocidad:F2}"
        );
    }
}
