
using UnityEngine;

public class Mineral : MonoBehaviour
{
    private GestorMinerales gestor;
    private bool recogido = false;

    private void Start()
    {
        gestor = FindFirstObjectByType<GestorMinerales>();

        if (gestor == null)
        {
            Debug.LogError(
                "No se ha encontrado GestorMinerales."
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (recogido || !other.CompareTag("Player"))
            return;

        if (gestor == null)
            return;

        recogido = true;
        gestor.RecogerMineral();

        Destroy(gameObject);
    }
}
