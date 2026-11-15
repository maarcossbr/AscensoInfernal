using System.Collections.Generic;
using UnityEngine;

public class MeteoritoPool : MonoBehaviour
{
    [Header("Configuracion del pool")]
    [SerializeField] private Meteorito prefabMeteorito;

    [SerializeField] private int cantidadInicial = 5;

    [SerializeField] private int maximoMeteoritos = 5;


    // Cola con los meteoritos disponibles para reutilizar.
    private Queue<Meteorito> meteoritosDisponibles =
        new Queue<Meteorito>();

    // Permite comprobar si un meteorito ya esta dentro del pool.
    private HashSet<Meteorito> meteoritosEnPool =
        new HashSet<Meteorito>();

    // Numero total de meteoritos creados.
    private int totalMeteoritos;


    private void Awake()
    {
        InicializarPool();
    }


    /// Crea los meteoritos iniciales del pool.
    private void InicializarPool()
    {
        if (prefabMeteorito == null)
        {
            Debug.LogError(
                "MeteoritoPool: No se ha asignado el prefab del meteorito.",
                this
            );

            return;
        }

        // Evitamos configuraciones incorrectas.
        maximoMeteoritos = Mathf.Max(1, maximoMeteoritos);

        cantidadInicial = Mathf.Clamp(
            cantidadInicial,
            1,
            maximoMeteoritos
        );

        // Creamos los objetos que estaran disponibles inicialmente.
        for (int i = 0; i < cantidadInicial; i++)
        {
            CrearMeteorito();
        }

        Debug.Log(
            "MeteoritoPool inicializado. Meteoritos creados: "
            + totalMeteoritos
        );
    }


    /// Crea un meteorito y lo introduce en el pool.
    private Meteorito CrearMeteorito()
    {
        Meteorito nuevoMeteorito = Instantiate(
            prefabMeteorito,
            transform
        );

        // Los meteoritos permanecen desactivados hasta necesitarlos.
        nuevoMeteorito.gameObject.SetActive(false);

        totalMeteoritos++;

        // Lo guardamos como objeto disponible.
        meteoritosDisponibles.Enqueue(nuevoMeteorito);
        meteoritosEnPool.Add(nuevoMeteorito);

        return nuevoMeteorito;
    }


    /// Obtiene un meteorito disponible y lo activa.
    public Meteorito Obtener(
        Vector3 posicion,
        float velocidad,
        int dano)
    {
        Meteorito meteorito;

        // Comprobamos si tenemos meteoritos disponibles.
        if (meteoritosDisponibles.Count > 0)
        {
            meteorito = meteoritosDisponibles.Dequeue();

            meteoritosEnPool.Remove(meteorito);
        }
        else
        {
            // Si no quedan disponibles, comprobamos el limite.
            if (totalMeteoritos >= maximoMeteoritos)
            {
                Debug.LogWarning(
                    "MeteoritoPool: Se ha alcanzado el limite de meteoritos.",
                    this
                );

                return null;
            }

            // Creamos uno nuevo si todavia podemos hacerlo.
            meteorito = CrearMeteorito();

            meteoritosDisponibles.Dequeue();
            meteoritosEnPool.Remove(meteorito);
        }

        // Establecemos su posicion y rotacion.
        meteorito.transform.position = posicion;
        meteorito.transform.rotation = Quaternion.identity;

        // Activamos el objeto.
        meteorito.gameObject.SetActive(true);

        // Configuramos su movimiento y su comportamiento.
        meteorito.Activar(
            velocidad,
            dano,
            Devolver
        );

        return meteorito;
    }


    /// Devuelve un meteorito al pool para reutilizarlo.
    public void Devolver(Meteorito meteorito)
    {
        if (meteorito == null)
            return;

        // Evitamos introducir el mismo objeto dos veces.
        if (meteoritosEnPool.Contains(meteorito))
            return;

        // Desactivamos el meteorito.
        meteorito.gameObject.SetActive(false);

        // Lo marcamos como disponible.
        meteoritosDisponibles.Enqueue(meteorito);
        meteoritosEnPool.Add(meteorito);
    }


    /// Devuelve el numero de meteoritos disponibles.
    public int ObtenerCantidadDisponible()
    {
        return meteoritosDisponibles.Count;
    }


    /// Devuelve el numero total de meteoritos creados.
    public int ObtenerCantidadTotal()
    {
        return totalMeteoritos;
    }
}
