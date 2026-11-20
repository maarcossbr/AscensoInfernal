using UnityEngine;

public class bullet : MonoBehaviour
{
     [Header("Configuracion Bala")]
    public float velocidad = 5f;
    public float tiempoDeVida = 4f;

    private Rigidbody2D rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = transform.right * velocidad;
        Destroy(gameObject, tiempoDeVida);
    }

}
