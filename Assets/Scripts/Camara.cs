
using UnityEngine;

public class Camara : MonoBehaviour
{
    [Header("Seguimiento del jugador")]
    [SerializeField] private Transform objetivo;
    [SerializeField] private float offsetVertical = 2f;
    [SerializeField] private float suavizado = 0.2f;
    [SerializeField] private float maxAltura = 98f;

    [Header("Sacudida de camara")]
    [SerializeField] private float intensidadSacudida = 0.4f;
    [SerializeField] private float duracionSacudida = 0.8f;
    [SerializeField] private float frecuenciaSacudida = 20f;

    private float velocidadVertical;
    private float alturaBase;
    private float posicionXBase;
    private float posicionZBase;

    private float tiempoRestanteSacudida;
    private float intensidadActual;
    private float semillaSacudida;

    private void Awake()
    {
        // Guardamos la posicion inicial de la camara.
        alturaBase = transform.position.y;
        posicionXBase = transform.position.x;
        posicionZBase = transform.position.z;
    }

    private void LateUpdate()
    {
        ActualizarSeguimiento();
        ActualizarSacudida();
    }

    private void ActualizarSeguimiento()
    {
        if (objetivo == null)
            return;

        float alturaObjetivo = objetivo.position.y + offsetVertical;

        if (objetivo.position.y > maxAltura)
            alturaObjetivo = maxAltura + offsetVertical;

        alturaBase = Mathf.SmoothDamp(
            alturaBase,
            alturaObjetivo,
            ref velocidadVertical,
            suavizado
        );
    }

    private void ActualizarSacudida()
    {
        Vector2 desplazamiento = Vector2.zero;

        if (tiempoRestanteSacudida > 0f)
        {
            tiempoRestanteSacudida -= Time.deltaTime;

            float progreso = Mathf.Clamp01(
                tiempoRestanteSacudida / duracionSacudida
            );

            float intensidad = intensidadActual *
                Mathf.SmoothStep(0f, 1f, progreso);

            float ruidoX = Mathf.PerlinNoise(
                semillaSacudida,
                Time.time * frecuenciaSacudida
            ) * 2f - 1f;

            float ruidoY = Mathf.PerlinNoise(
                semillaSacudida + 100f,
                Time.time * frecuenciaSacudida
            ) * 2f - 1f;

            desplazamiento = new Vector2(
                ruidoX,
                ruidoY
            ) * intensidad;
        }
        else
        {
            tiempoRestanteSacudida = 0f;
        }

        transform.position = new Vector3(
            posicionXBase + desplazamiento.x,
            alturaBase + desplazamiento.y,
            posicionZBase
        );
    }

   public void IniciarSacudida(
    float intensidad,
    float duracion
    )
    {
        Debug.Log("TERREMOTO ACTIVADO");

        if (duracion <= 0f || intensidad <= 0f)
            return;

        intensidadActual = intensidad;
        duracionSacudida = duracion;
        tiempoRestanteSacudida = duracion;

        semillaSacudida = Random.Range(0f, 1000f);
    }

    [ContextMenu("Probar sacudida")]
    private void ProbarSacudida()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "La sacudida solo se puede probar durante el juego."
            );

            return;
        }

        IniciarSacudida(
            intensidadSacudida,
            duracionSacudida
        );
    }
}
