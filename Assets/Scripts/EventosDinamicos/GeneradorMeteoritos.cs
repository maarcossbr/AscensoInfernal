
using System.Collections;
using UnityEngine;

public class GeneradorMeteoritos : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private MeteoritoPool pool;
    [SerializeField] private Transform jugador;
    [SerializeField] private Camera camara;

    [Header("Posicion de aparicion")]
    [SerializeField] private float alturaSobreCamara = 2f;
    [SerializeField] private float margenHorizontal = 0.5f;
    [SerializeField] private float distanciaMinimaJugador = 2f;

    [Header("Limites horizontales del nivel")]
    [SerializeField] private float limiteXIzquierdo = -5f;
    [SerializeField] private float limiteXDerecho = 5f;

    private bool oleadaEnCurso;
    private Coroutine rutinaOleada;

    public bool OleadaEnCurso => oleadaEnCurso;

    // Se conserva por compatibilidad con el codigo anterior.
    // Las lluvias continuas no tienen una finalizacion automatica.
    public bool OleadaCompletada { get; private set; }

    // Inicia una lluvia continua.
    public bool IniciarLluvia(
        float retrasoInicial,
        float intervalo,
        float velocidad,
        int nuevoDano)
    {
        if (oleadaEnCurso)
        {
            Debug.LogWarning(
                "GeneradorMeteoritos: Ya hay una lluvia activa.",
                this
            );

            return false;
        }

        if (pool == null || camara == null)
        {
            Debug.LogError(
                "GeneradorMeteoritos: Faltan referencias en el Inspector.",
                this
            );

            return false;
        }

        retrasoInicial = Mathf.Max(0f, retrasoInicial);
        intervalo = Mathf.Max(0.1f, intervalo);
        velocidad = Mathf.Max(0.1f, velocidad);
        nuevoDano = Mathf.Max(1, nuevoDano);

        OleadaCompletada = false;
        oleadaEnCurso = true;

        rutinaOleada = StartCoroutine(
            GenerarLluvia(
                retrasoInicial,
                intervalo,
                velocidad,
                nuevoDano
            )
        );

        return true;
    }

    // Metodo anterior conservado para no romper llamadas existentes.
    // El parametro cantidad deja de limitar la generacion.
    public bool IniciarOleada(
        int cantidad,
        float retrasoInicial,
        float intervalo,
        float velocidad,
        int nuevoDano)
    {
        return IniciarLluvia(
            retrasoInicial,
            intervalo,
            velocidad,
            nuevoDano
        );
    }

    private IEnumerator GenerarLluvia(
        float retrasoInicial,
        float intervalo,
        float velocidad,
        int nuevoDano)
    {
        if (retrasoInicial > 0f)
            yield return new WaitForSeconds(retrasoInicial);

        Debug.Log(
            "GeneradorMeteoritos: Comienza la lluvia continua."
        );

        while (oleadaEnCurso)
        {
            Vector3 posicion = ObtenerPosicionAparicion();

            Meteorito meteorito = pool.Obtener(
                posicion,
                velocidad,
                nuevoDano
            );

            if (meteorito != null)
            {
                Debug.Log(
                    "GeneradorMeteoritos: Meteorito generado."
                );
            }

            yield return new WaitForSeconds(intervalo);
        }

        rutinaOleada = null;
    }

    // Detiene la generacion de nuevos meteoritos.
    // Los meteoritos que ya estan activos siguen su trayectoria.
    public void DetenerOleada()
    {
        if (rutinaOleada != null)
        {
            StopCoroutine(rutinaOleada);
            rutinaOleada = null;
        }

        if (!oleadaEnCurso)
            return;

        oleadaEnCurso = false;
        OleadaCompletada = false;

        Debug.Log(
            "GeneradorMeteoritos: Lluvia detenida."
        );
    }

    private void OnDisable()
    {
        DetenerOleada();
    }

    private Vector3 ObtenerPosicionAparicion()
    {
        float limiteIzquierdo =
            limiteXIzquierdo + margenHorizontal;

        float limiteDerecho =
            limiteXDerecho - margenHorizontal;

        float posicionX = Random.Range(
            limiteIzquierdo,
            limiteDerecho
        );

        if (jugador != null)
        {
            for (int i = 0; i < 20; i++)
            {
                if (Mathf.Abs(posicionX - jugador.position.x)
                    >= distanciaMinimaJugador)
                {
                    break;
                }

                posicionX = Random.Range(
                    limiteIzquierdo,
                    limiteDerecho
                );
            }
        }

        float posicionY =
            camara.transform.position.y
            + camara.orthographicSize
            + alturaSobreCamara;

        return new Vector3(posicionX, posicionY, 0f);
    }

    private void OnDrawGizmos()
    {
        float izquierda =
            limiteXIzquierdo + margenHorizontal;

        float derecha =
            limiteXDerecho - margenHorizontal;

        float centroY = camara != null
            ? camara.transform.position.y
            : transform.position.y;

        float altura = camara != null
            ? camara.orthographicSize + alturaSobreCamara
            : 10f;

        Gizmos.color = Color.red;

        Gizmos.DrawLine(
            new Vector3(izquierda, centroY - altura, 0f),
            new Vector3(izquierda, centroY + altura, 0f)
        );

        Gizmos.color = Color.green;

        Gizmos.DrawLine(
            new Vector3(derecha, centroY - altura, 0f),
            new Vector3(derecha, centroY + altura, 0f)
        );

        Gizmos.color = Color.yellow;

        Gizmos.DrawLine(
            new Vector3(izquierda, centroY, 0f),
            new Vector3(derecha, centroY, 0f)
        );
    }
}
