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

    private bool oleadaEnCurso;
    private Coroutine rutinaOleada;

    // Inicia una lluvia continua de meteoritos.
    public bool IniciarOleada(
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

        intervalo = Mathf.Max(0.1f, intervalo);
        velocidad = Mathf.Max(0.1f, velocidad);
        nuevoDano = Mathf.Max(1, nuevoDano);

        oleadaEnCurso = true;

        rutinaOleada = StartCoroutine(
            GenerarOleada(intervalo, velocidad, nuevoDano)
        );

        return true;
    }

    // Genera meteoritos continuamente hasta detener la oleada.
    private IEnumerator GenerarOleada(
        float intervalo,
        float velocidad,
        int nuevoDano)
    {
        int meteoritosGenerados = 0;

        Debug.Log(
            "GeneradorMeteoritos: Comienza la lluvia de meteoritos."
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
                meteoritosGenerados++;

                Debug.Log(
                    "Meteoritos generados durante la oleada: "
                    + meteoritosGenerados
                );
            }
            else
            {
                Debug.LogWarning(
                    "GeneradorMeteoritos: No hay meteoritos disponibles."
                );
            }

            yield return new WaitForSeconds(intervalo);
        }

        rutinaOleada = null;

        Debug.Log(
            "GeneradorMeteoritos: La lluvia de meteoritos ha terminado."
        );
    }

    // Detiene la generacion continua de meteoritos.
    public void DetenerOleada()
    {
        if (!oleadaEnCurso)
            return;

        oleadaEnCurso = false;

        if (rutinaOleada != null)
        {
            StopCoroutine(rutinaOleada);
            rutinaOleada = null;
        }

        Debug.Log(
            "GeneradorMeteoritos: Oleada detenida."
        );
    }

    // Calcula una posicion aleatoria encima de la camara.
    private Vector3 ObtenerPosicionAparicion()
    {
        float alturaCamara = camara.orthographicSize;
        float anchuraCamara = alturaCamara * camara.aspect;

        float limiteIzquierdo =
            camara.transform.position.x
            - anchuraCamara
            + margenHorizontal;

        float limiteDerecho =
            camara.transform.position.x
            + anchuraCamara
            - margenHorizontal;

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
}