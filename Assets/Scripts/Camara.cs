
using UnityEngine;

public class Camara : MonoBehaviour
{
    [SerializeField] private Transform objetivo;
    [SerializeField] private float offsetVertical = 2f;
    [SerializeField] private float suavizado = 0.2f;

    private float velocidadVertical;

    private void LateUpdate()
    {
        if (objetivo == null)
            return;

        float alturaObjetivo = objetivo.position.y + offsetVertical;

        float nuevaAltura = Mathf.SmoothDamp(
            transform.position.y,
            alturaObjetivo,
            ref velocidadVertical,
            suavizado
        );

        transform.position = new Vector3(
            transform.position.x,
            nuevaAltura,
            transform.position.z
        );
    }
}
