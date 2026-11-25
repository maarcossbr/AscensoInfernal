
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

    private int indiceOleadaActiva = -1;

    // Registra si cada zona ya ha provocado un temblor.
    private bool[] tembloresRealizados;

    private void Awake()
    {
        // Inicializar el registro segun el numero de zonas.
        tembloresRealizados = new bool[oleadas.Length];
    }

    private void Update()
    {
        if (jugador == null || generador == null)
            return;

        int nuevoIndice = ObtenerIndiceOleadaCorrespondiente(
            jugador.position.y
        );

        // El jugador sigue en la misma zona.
        if (nuevoIndice == indiceOleadaActiva)
        {
            // Si la lluvia se detuvo inesperadamente, la recuperamos.
            if (nuevoIndice != -1 && !generador.OleadaEnCurso)
            {
                IniciarLluvia(nuevoIndice);
            }

            return;
        }

        // Hemos cambiado de zona.
        generador.DetenerOleada();

        indiceOleadaActiva = nuevoIndice;

        // Todavia no se ha alcanzado la primera zona.
        if (indiceOleadaActiva == -1)
            return;

        IniciarLluvia(indiceOleadaActiva);
    }

    private void IniciarLluvia(int indice)
    {
        if (indice < 0 || indice >= oleadas.Length)
            return;

        ConfiguracionOleada configuracion = oleadas[indice];

        bool iniciada = generador.IniciarLluvia(
            retrasoMeteoritos,
            configuracion.intervalo,
            configuracion.velocidad,
            configuracion.dano
        );

        if (!iniciada)
        {
            indiceOleadaActiva = -1;
            return;
        }

        // El temblor solo se produce la primera vez
        // que se activa cada zona.
        if (!tembloresRealizados[indice])
        {
            if (camara != null)
            {
                camara.IniciarSacudida(
                    intensidadSacudida,
                    duracionSacudida
                );

                tembloresRealizados[indice] = true;
            }
            else
            {
                Debug.LogWarning(
                    "GestorEventosDinamicos: No se ha asignado la camara.",
                    this
                );
            }
        }

        Debug.Log(
            "GestorEventosDinamicos: Zona de lluvia "
            + (indice + 1) + " activada."
        );
    }

    private int ObtenerIndiceOleadaCorrespondiente(
        float alturaJugador)
    {
        if (oleadas == null || oleadas.Length == 0)
            return -1;

        int indiceSeleccionado = -1;
        float mayorAlturaAlcanzada = float.NegativeInfinity;

        for (int i = 0; i < oleadas.Length; i++)
        {
            ConfiguracionOleada configuracion = oleadas[i];

            if (configuracion == null)
                continue;

            float altura = configuracion.alturaActivacion;

            if (alturaJugador >= altura &&
                altura >= mayorAlturaAlcanzada)
            {
                mayorAlturaAlcanzada = altura;
                indiceSeleccionado = i;
            }
        }

        return indiceSeleccionado;
    }
}
