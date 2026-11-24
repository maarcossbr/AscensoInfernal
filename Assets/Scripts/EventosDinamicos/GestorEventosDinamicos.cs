using UnityEngine;

public class GestorEventosDinamicos : MonoBehaviour
{
    [System.Serializable]
    private class ConfiguracionOleada
    {
        [Header("Activacion")]
        public float alturaActivacion = 30f;

        [Header("Generacion")]
        public float intervalo = 0.5f;
        public float velocidad = 9f;
        public int dano = 1;

        [Min(1)]
        public int cantidadMeteoritos = 8;
    }

    [Header("Referencias")]
    [SerializeField] private Transform jugador;
    [SerializeField] private GeneradorMeteoritos generador;
    [SerializeField] private Camara camara;

    [Header("Configuracion del terremoto")]
    [SerializeField] private float intensidadSacudida = 0.15f;
    [SerializeField] private float duracionSacudida = 0.45f;
    [Header("Retraso despues del terremoto")]
    [Min(0f)]
    [SerializeField] private float retrasoMeteoritos = 1f;


    [Header("Configuracion de oleadas")]
    [SerializeField]
    private ConfiguracionOleada[] oleadas =
    {
    new ConfiguracionOleada
    {
        alturaActivacion = 30f,
        intervalo = 0.5f,
        velocidad = 9f,
        dano = 1,
        cantidadMeteoritos = 8
    },

    new ConfiguracionOleada
    {
        alturaActivacion = 60f,
        intervalo = 0.35f,
        velocidad = 9f,
        dano = 1,
        cantidadMeteoritos = 12
    },

    new ConfiguracionOleada
    {
        alturaActivacion = 85f,
        intervalo = 0.25f,
        velocidad = 9f,
        dano = 1,
        cantidadMeteoritos = 16
    }
};


    private static bool[] oleadasCompletadas = new bool[3];

    private int indiceOleadaEnCurso = -1;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void InicializarProgreso()
    {
        oleadasCompletadas = new bool[3];
    }


    private void Update()
    {
        if (jugador == null || generador == null)
            return;

        // Comprobamos si existe una oleada activa.
        if (indiceOleadaEnCurso != -1)
        {
            // Esperamos mientras sigan generándose o cayendo meteoritos.
            if (generador.OleadaEnCurso)
                return;

            // Comprobamos si la oleada ha terminado correctamente.
            if (generador.OleadaCompletada)
            {
                oleadasCompletadas[indiceOleadaEnCurso] = true;

                Debug.Log(
                    "GestorEventosDinamicos: Oleada "
                    + (indiceOleadaEnCurso + 1)
                    + " completada."
                );
            }

            indiceOleadaEnCurso = -1;
            return;
        }

        // Buscamos la siguiente oleada pendiente.
        for (int i = 0; i < oleadas.Length; i++)
        {
            if (EstaCompletada(i))
                continue;

            ConfiguracionOleada configuracion = oleadas[i];

            if (jugador.position.y < configuracion.alturaActivacion)
                return;

            // Iniciamos la oleada con la cantidad y el retraso configurados.
            bool iniciada = generador.IniciarOleada(
                configuracion.cantidadMeteoritos,
                retrasoMeteoritos,
                configuracion.intervalo,
                configuracion.velocidad,
                configuracion.dano
            );

            if (!iniciada)
                return;

            indiceOleadaEnCurso = i;

            // Activamos el terremoto al comenzar la oleada.
            if (camara != null)
            {
                camara.IniciarSacudida(
                    intensidadSacudida,
                    duracionSacudida
                );
            }
            else
            {
                Debug.LogWarning(
                    "GestorEventosDinamicos: No se ha asignado la camara.",
                    this
                );
            }

            Debug.Log(
                "GestorEventosDinamicos: Oleada "
                + (i + 1) + " iniciada."
            );

            return;
        }
    }

    private bool EstaCompletada(int indice)
    {
        if (indice < 0 || indice >= oleadasCompletadas.Length)
            return false;

        return oleadasCompletadas[indice];
    }

    // Restablece el progreso al comenzar una partida nueva.
    public static void ReiniciarProgreso()
    {
        oleadasCompletadas = new bool[3];

        Debug.Log(
            "GestorEventosDinamicos: Progreso de oleadas restablecido."
        );
    }
}
