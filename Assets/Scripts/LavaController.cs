
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class LavaController : MonoBehaviour
{
    public float baseSpeed = 0.6f;
    public float acceleration = 0.02f;

    public float currentSpeed;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        currentSpeed = baseSpeed;
    }

    private void FixedUpdate()
    {
        currentSpeed += acceleration * Time.fixedDeltaTime;

        Vector2 nuevaPosicion = rb.position + Vector2.up * currentSpeed * Time.fixedDeltaTime;

        rb.MovePosition(nuevaPosicion);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Restablecer el tiempo del juego.
            Time.timeScale = 1f;

            // Reiniciar la escena actual.
            SceneManager.LoadScene(
                SceneManager.GetActiveScene().name
            );
        }
    }

}
