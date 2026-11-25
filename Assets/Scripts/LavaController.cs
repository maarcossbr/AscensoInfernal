
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class LavaController : MonoBehaviour
{
    public float baseSpeed = 0.6f;
    public float acceleration = 0.02f;

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
        currentSpeed = baseSpeed;

        if (panelMuerte != null)
            panelMuerte.SetActive(false);
    }

    private void FixedUpdate()
    {
        currentSpeed += acceleration * Time.fixedDeltaTime;

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
        // Recuperamos el tiempo normal del juego.
        Time.timeScale = 1f;

        // La nueva instancia del gestor inicializara las oleadas.
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
