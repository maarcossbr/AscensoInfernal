
using UnityEngine;

public class MetaNivel : MonoBehaviour
{
    private bool nivelCompletado = false;

    private void Start()
    {
        Time.timeScale = 1f;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (nivelCompletado || !other.CompareTag("Player"))
            return;

        nivelCompletado = true;

        Debug.Log("¡VICTORIA! Has completado el nivel.");

        Time.timeScale = 0f;
    }
}
