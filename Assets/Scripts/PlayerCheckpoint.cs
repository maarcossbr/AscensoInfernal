
using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
public class PlayerCheckpoint : MonoBehaviour
{
    [Header("Lava")]
    [SerializeField] private LavaController lava;

    [Min(0f)]
    [SerializeField] private float segundosSeguridad = 8f;

    public Vector3 LastCheckpointPosition { get; private set; }
    public int HealthAtCheckpoint { get; private set; }
    public bool HasCheckpoint { get; private set; }

    public Vector2 LavaPositionAtCheckpoint { get; private set; }
    public float LavaSpeedAtCheckpoint { get; private set; }
    public bool HasLavaState { get; private set; }
    public string[] MineralesEnCheckpoint { get; private set; }

    public void ActivateCheckpoint(Vector3 position)
    {
        PlayerHealth health = GetComponent<PlayerHealth>();

        if (health.IsDead)
            return;

        LastCheckpointPosition = position;
        HealthAtCheckpoint = health.CurrentHealth;
        HasCheckpoint = true;
        MineralesEnCheckpoint = MineralCheckpointId.ObtenerRecogidos();
        HasLavaState = false;

        if (lava != null)
        {
            Rigidbody2D lavaRb = lava.GetComponent<Rigidbody2D>();
            Collider2D lavaCollider = lava.GetComponent<Collider2D>();
            Collider2D playerCollider = GetComponent<Collider2D>();

            if (lavaRb != null &&
                lavaCollider != null &&
                playerCollider != null)
            {
                float tiempo = segundosSeguridad;
                float velocidad = Mathf.Max(0f, lava.currentSpeed);
                float aceleracion = Mathf.Max(0f, lava.acceleration);

                float distanciaSegura =
                    velocidad * tiempo +
                    0.5f * aceleracion * tiempo * tiempo;

                // Altura de los pies al reaparecer.
                float piesJugador = position.y +
                    (playerCollider.bounds.min.y - transform.position.y);

                // Distancia entre el centro de la lava
                // y el borde superior de su collider.
                float offsetLava =
                    lavaCollider.bounds.max.y - lavaRb.position.y;

                float posicionSeguraY =
                    piesJugador - distanciaSegura - offsetLava;

                LavaPositionAtCheckpoint = new Vector2(
                    lavaRb.position.x,
                    Mathf.Min(lavaRb.position.y, posicionSeguraY)
                );

                LavaSpeedAtCheckpoint = lava.currentSpeed;
                HasLavaState = true;
            }
            else
            {
                Debug.LogWarning(
                    "Falta Rigidbody2D o Collider2D para guardar la lava."
                );
            }
        }
        else
        {
            Debug.LogWarning(
                "No hay ninguna lava asignada al checkpoint."
            );
        }

        Debug.Log(
            $"Checkpoint activado con {HealthAtCheckpoint} corazones. " +
            $"Lava guardada: {HasLavaState}"
        );
    }
}
