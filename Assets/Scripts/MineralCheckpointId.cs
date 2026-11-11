
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Mineral))]
public class MineralCheckpointId : MonoBehaviour
{
    [SerializeField] private string mineralId;

    public string MineralId => mineralId;

    private static readonly HashSet<string> recogidos = new();
    private static ulong escenaRegistrada = ulong.MaxValue;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Inicializar()
    {
        recogidos.Clear();
        escenaRegistrada = ulong.MaxValue;
    }

    private void Awake()
    {
        ulong escenaActual = gameObject.scene.handle.GetRawData();

        if (escenaActual != escenaRegistrada)
        {
            recogidos.Clear();
            escenaRegistrada = escenaActual;
        }

        if (string.IsNullOrWhiteSpace(mineralId))
        {
            Debug.LogError(
                "Mineral sin identificador: " + gameObject.name
            );
            return;
        }

        // Si estaba recogido en el checkpoint,
        // no debe reaparecer.
        if (CheckpointRespawn.MineralGuardado(mineralId))
        {
            recogidos.Add(mineralId);
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (string.IsNullOrWhiteSpace(mineralId))
            return;

        recogidos.Add(mineralId);
    }

    public static string[] ObtenerRecogidos()
    {
        string[] resultado = new string[recogidos.Count];
        recogidos.CopyTo(resultado);

        return resultado;
    }
}
