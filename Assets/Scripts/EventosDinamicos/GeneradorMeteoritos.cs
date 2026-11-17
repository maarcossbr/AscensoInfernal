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
    public bool OleadaCompletada { get; private set; }


    // Inicia una lluvia continua de meteoritos.

    public bool IniciarOleada(
        int cantidad,
        float retrasoInicial,
        float intervalo,
        float velocidad,
        int nuevoDano)
    {
        if (oleadaEnCurso)
        {
            Debug.LogWarning(
                "GeneradorMeteoritos: Ya hay una oleada en curso.",
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

        cantidad = Mathf.Max(1, cantidad);
        retrasoInicial = Mathf.Max(0f, retrasoInicial);
        intervalo = Mathf.Max(0.1f, intervalo);
        velocidad = Mathf.Max(0.1f, velocidad);
        nuevoDano = Mathf.Max(1, nuevoDano);

        OleadaCompletada = false;
        oleadaEnCurso = true;

        rutinaOleada = StartCoroutine(
            GenerarOleada(
                cantidad,
                retrasoInicial,
                intervalo,
                velocidad,
                nuevoDano
            )
        );

        return true;
    }



    // Genera una cantidad limitada de meteoritos tras el terremoto.
    private IEnumerator GenerarOleada(
        int cantidad,
        float retrasoInicial,
        float intervalo,
        float velocidad,
        int nuevoDano)
    {
        // Esperamos antes de que empiecen a caer los meteoritos.
        yield return new WaitForSeconds(retrasoInicial);

        int meteoritosGenerados = 0;

        Debug.Log(
            "GeneradorMeteoritos: Comienza la lluvia de meteoritos."
        );

        // Generamos solamente la cantidad configurada.
        while (oleadaEnCurso && meteoritosGenerados < cantidad)
        {
            Vector3 posicion = ObtenerPosicionAparicion();

            Meteorito meteorito = pool.Obtener(
                posicion,
                velocidad,
                nuevoDano
            );

            if (meteorito != null)
            {
                meteoritosGenerados++;

                Debug.Log(
                    "Meteoritos generados: "
                    + meteoritosGenerados + "/" + cantidad
                );
            }
            else
            {
                Debug.LogWarning(
                    "GeneradorMeteoritos: No hay meteoritos disponibles."
                );
            }

            if (meteoritosGenerados < cantidad)
            {
                yield return new WaitForSeconds(intervalo);
            }
        }

        // Esperamos a que desaparezcan los meteoritos que siguen cayendo.
        while (oleadaEnCurso &&
               pool.ObtenerCantidadDisponible() < pool.ObtenerCantidadTotal())
        {
            yield return null;
        }

        // Marcamos la oleada como completada.
        oleadaEnCurso = false;
        OleadaCompletada = true;
        rutinaOleada = null;

        Debug.Log(
            "GeneradorMeteoritos: Oleada completada con "
            + meteoritosGenerados + " meteoritos."
        );
    }


    // Detiene la generacion continua de meteoritos.

    public void DetenerOleada()
    {
        if (!oleadaEnCurso)
            return;

        oleadaEnCurso = false;
        OleadaCompletada = false;

        if (rutinaOleada != null)
        {
            StopCoroutine(rutinaOleada);
            rutinaOleada = null;
        }

        Debug.Log(
            "GeneradorMeteoritos: Oleada detenida."
        );
    }


    private void OnDisable()
    {
        DetenerOleada();
    }


    // Calcula una posicion aleatoria encima de la camara.
    private Vector3 ObtenerPosicionAparicion()
    {

        float alturaCamara = camara.orthographicSize;

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
            + alturaCamara
            + alturaSobreCamara;

        return new Vector3(posicionX, posicionY, 0f);
    }


    private void OnDrawGizmos()
    {
        float izquierda = limiteXIzquierdo + margenHorizontal;
        float derecha = limiteXDerecho - margenHorizontal;

        float centroY = camara != null
            ? camara.transform.position.y
            : transform.position.y;

        float altura = camara != null
            ? camara.orthographicSize + alturaSobreCamara
            : 10f;

        // Limite izquierdo (rojo)
        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            new Vector3(izquierda, centroY - altura, 0f),
            new Vector3(izquierda, centroY + altura, 0f)
        );

        // Limite derecho (verde)
        Gizmos.color = Color.green;
        Gizmos.DrawLine(
            new Vector3(derecha, centroY - altura, 0f),
            new Vector3(derecha, centroY + altura, 0f)
        );

        // Linea horizontal que une ambos limites
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(
            new Vector3(izquierda, centroY, 0f),
            new Vector3(derecha, centroY, 0f)
        );
    }

}