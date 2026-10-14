
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlataformaMovil : MonoBehaviour
{
    [SerializeField] private Vector2 desplazamiento = new Vector2(-2f, 0f);
    [SerializeField] private float duracionIda = 1.5f;

    public float VelocidadHorizontal { get; private set; }

    private Rigidbody2D rb;
    private Vector2 posicionInicial;
    private float tiempo;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        posicionInicial = rb.position;
    }

    private void FixedUpdate()
    {
        if (duracionIda <= 0f)
        {
            VelocidadHorizontal = 0f;
            return;
        }

        tiempo += Time.fixedDeltaTime;

        float progreso = Mathf.PingPong(tiempo / duracionIda, 1f);
        Vector2 destino = posicionInicial + desplazamiento * progreso;

        VelocidadHorizontal =
            (destino.x - rb.position.x) / Time.fixedDeltaTime;

        rb.MovePosition(destino);
    }
}
