
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class LavaController : MonoBehaviour
{
    [Header("Velocidad de la lava")]
    public float baseSpeed = 0.6f;
    public float acceleration = 0.02f;
    public float maxSpeed = 2f;

    public float currentSpeed;

    [Header("Pantalla de muerte")]
    [SerializeField] private GameObject panelMuerte;

    private Rigidbody2D rb;
    private bool jugadorMuerto = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // La velocidad maxima nunca puede ser inferior
        // a la velocidad inicial.
        maxSpeed = Mathf.Max(baseSpeed, maxSpeed);

        currentSpeed = baseSpeed;

        if (panelMuerte != null)
            panelMuerte.SetActive(false);
    }

    private void FixedUpdate()
    {
        // Aumentar la velocidad progresivamente sin superar
        // el limite establecido.
        currentSpeed = Mathf.MoveTowards(
            currentSpeed,
            maxSpeed,
            acceleration * Time.fixedDeltaTime
        );

        Vector2 nuevaPosicion = rb.position +
            Vector2.up * currentSpeed * Time.fixedDeltaTime;

        rb.MovePosition(nuevaPosicion);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (jugadorMuerto || !collision.CompareTag("Player"))
            return;

        jugadorMuerto = true;

        PlayerHealth health =
            collision.GetComponentInParent<PlayerHealth>();

        if (health != null)
        {
            health.KillInstantly();
            return;
        }

        if (panelMuerte != null)
        {
            panelMuerte.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            Debug.LogError("No se ha asignado el panel de muerte.");
            Reintentar();
        }
    }

    public void Reintentar()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    public void VolverAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu_Inicio");
    }
}
