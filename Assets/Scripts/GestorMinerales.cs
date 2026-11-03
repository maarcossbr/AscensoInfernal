
using UnityEngine;
using TMPro;

public class GestorMinerales : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] private int mineralesNecesarios = 3;

    [Header("Interfaz")]
    [SerializeField] private TMP_Text textoContador;

    private int mineralesRecogidos = 0;

    public bool TieneTodos =>
        mineralesRecogidos >= mineralesNecesarios;

    public int MineralesRestantes =>
        Mathf.Max(0, mineralesNecesarios - mineralesRecogidos);

    private void Start()
    {
        ActualizarContador();
    }

    public void RecogerMineral()
    {
        if (TieneTodos)
            return;

        mineralesRecogidos++;
        ActualizarContador();

        Debug.Log("Minerales recogidos: " +
                  mineralesRecogidos);
    }

    private void ActualizarContador()
    {
        if (textoContador != null)
        {
            textoContador.text =
                $"Minerales: {mineralesRecogidos}/{mineralesNecesarios}";
        }
    }
}
