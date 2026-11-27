
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class Camara : MonoBehaviour
{
    [Header("Seguimiento del jugador")]
    [SerializeField] private Transform objetivo;
    [SerializeField] private float offsetHorizontal = 0f;
    [SerializeField] private float offsetVertical = 2f;
    [SerializeField] private float suavizadoHorizontal = 0.2f;
    [SerializeField] private float suavizadoVertical = 0.2f;
    [SerializeField] private float maxAltura = 98f;

    [Header("Limites de la camara")]
    [SerializeField] private bool limitarMovimientoHorizontal = true;
    [SerializeField] private float limiteIzquierdo = -15f;
    [SerializeField] private float limiteDerecho = 15f;

    [Header("Sacudida de camara")]
    [SerializeField] private float intensidadSacudida = 0.4f;
    [SerializeField] private float duracionSacudida = 0.8f;
    [SerializeField] private float frecuenciaSacudida = 20f;

    private Camera camara;

    private float velocidadHorizontal;
    private float velocidadVertical;

    private float alturaBase;
    private float posicionXBase;
    private float posicionZBase;

    private float tiempoRestanteSacudida;
    private float intensidadActual;
    private float duracionSacudidaActual;
    private float semillaSacudida;

    private void Awake()
    {
        camara = GetComponent<Camera>();

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

        // Seguimiento horizontal del jugador.
        float objetivoX = objetivo.position.x + offsetHorizontal;

        // Aplicar limites a la posicion de la camara.
        if (limitarMovimientoHorizontal)
        {
            objetivoX = Mathf.Clamp(
                objetivoX,
                limiteIzquierdo,
                limiteDerecho
            );
        }

        posicionXBase = Mathf.SmoothDamp(
            posicionXBase,
            objetivoX,
            ref velocidadHorizontal,
            suavizadoHorizontal
        );

        // Evitar que el suavizado sobrepase los limites.
        if (limitarMovimientoHorizontal)
        {
            posicionXBase = Mathf.Clamp(
                posicionXBase,
                limiteIzquierdo,
                limiteDerecho
            );
        }

        // Seguimiento vertical del jugador.
        float alturaObjetivo = objetivo.position.y + offsetVertical;

        if (objetivo.position.y > maxAltura)
            alturaObjetivo = maxAltura + offsetVertical;

        alturaBase = Mathf.SmoothDamp(
            alturaBase,
            alturaObjetivo,
            ref velocidadVertical,
            suavizadoVertical
        );
    }

    private void ActualizarSacudida()
    {
        Vector2 desplazamiento = Vector2.zero;

        if (tiempoRestanteSacudida > 0f)
        {
            tiempoRestanteSacudida -= Time.deltaTime;

            float progreso = Mathf.Clamp01(
                tiempoRestanteSacudida / duracionSacudidaActual
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

    public void IniciarSacudida(float intensidad, float duracion)
    {
        if (duracion <= 0f || intensidad <= 0f)
            return;

        intensidadActual = intensidad;
        duracionSacudidaActual = duracion;
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
