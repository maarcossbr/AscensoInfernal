using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Meteorito : MonoBehaviour
{
    [Header("Configuracion del meteorito")]
    [SerializeField] private float velocidadCaida = 9f;
    [SerializeField] private int dano = 1;
    [SerializeField] private float tiempoMaximoActivo = 8f;

    private float tiempoRestante;
    private Rigidbody2D rb;
    private bool estaActivo;
    private Action<Meteorito> devolverAlPool;

    private void Awake()
    {
        ObtenerRigidbody();
    }

    // Obtiene el componente fisico del meteorito.
    private bool ObtenerRigidbody()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (rb == null)
        {
            Debug.LogError(
                "Meteorito: No se ha encontrado un Rigidbody2D en el objeto.",
                this
            );

            return false;
        }

        return true;
    }

    // Activa el meteorito y configura su comportamiento.
    public void Activar(
        float nuevaVelocidad,
        int nuevoDano,
        Action<Meteorito> callbackDevolucion)
    {
        if (!ObtenerRigidbody())
            return;

        velocidadCaida = Mathf.Max(0.1f, nuevaVelocidad);
        dano = Mathf.Max(1, nuevoDano);
        devolverAlPool = callbackDevolucion;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.simulated = true;

        estaActivo = true;
        tiempoRestante = tiempoMaximoActivo;

        rb.linearVelocity = Vector2.down * velocidadCaida;
    }

    private void Update()
    {
        if (!estaActivo)
            return;

        tiempoRestante -= Time.deltaTime;

        if (tiempoRestante <= 0f)
        {
            Devolver();
        }
    }

    // Solo procesa el impacto si el objeto es el jugador.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!estaActivo)
            return;

        PlayerHealth saludJugador =
            other.GetComponentInParent<PlayerHealth>();

        // Si no es el jugador, el meteorito continua su trayectoria.
        if (saludJugador == null)
            return;

        saludJugador.TakeDamage(dano);
        Devolver();
    }

    // Devuelve el meteorito al pool.
    private void Devolver()
    {
        if (!estaActivo)
            return;

        estaActivo = false;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        if (devolverAlPool != null)
        {
            devolverAlPool(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        estaActivo = false;
    }
}