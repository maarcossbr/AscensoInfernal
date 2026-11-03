using System.Collections;
using UnityEngine;
using TMPro;

public class MetaNivel : MonoBehaviour
{
    [Header("Sistema de minerales")]
    [SerializeField] private GestorMinerales gestorMinerales;
    [SerializeField] private TMP_Text textoAviso;

    private bool nivelCompletado = false;
    private Coroutine avisoActual;

    private void Start()
    {
        Time.timeScale = 1f;

        if (textoAviso != null)
            textoAviso.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (nivelCompletado || !other.CompareTag("Player"))
            return;

        if (gestorMinerales == null)
        {
            Debug.LogError("Falta asignar GestorMinerales.");
            return;
        }

        // Si faltan minerales, mostrar cuántos faltan
        if (!gestorMinerales.TieneTodos)
        {
            int restantes = gestorMinerales.MineralesRestantes;

            if (textoAviso != null)
            {
                if (restantes == 1)
                    textoAviso.text = "Te falta 1 mineral";
                else
                    textoAviso.text = $"Te faltan {restantes} minerales";

                textoAviso.gameObject.SetActive(true);

                if (avisoActual != null)
                    StopCoroutine(avisoActual);

                avisoActual = StartCoroutine(OcultarAviso());
            }

            return;
        }

        // Victoria
        nivelCompletado = true;

        if (textoAviso != null)
            textoAviso.gameObject.SetActive(false);

        Debug.Log("¡VICTORIA! Has completado el nivel.");

        Time.timeScale = 0f;
    }

    private IEnumerator OcultarAviso()
    {
        yield return new WaitForSeconds(2f);

        if (textoAviso != null)
            textoAviso.gameObject.SetActive(false);

        avisoActual = null;
    }
}