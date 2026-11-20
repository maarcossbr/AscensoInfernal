using System;
using UnityEngine;

public class enem_basico : MonoBehaviour
{
     [Header("Configuracion Enemigo")]
    public float velocidad = 0.5f;
    public float distancia = 2;

    private Rigidbody2D rb;
    private Vector2 posicionInicial;
    private Boolean izq = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    void Start()
    {
        posicionInicial = rb.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        // Movimiento
        if (izq)
        {
            Vector2 newPosition= rb.position + Vector2.left*velocidad*Time.fixedDeltaTime;
            if (posicionInicial.x - distancia < rb.position.x)
            {
                rb.MovePosition(newPosition);
            }
            else
            {
                izq = false;
                newPosition= rb.position + Vector2.right*velocidad*Time.fixedDeltaTime;
                rb.MovePosition(newPosition);

            }
        }
        else
        {
            Vector2 newPosition= rb.position + Vector2.right*velocidad*Time.fixedDeltaTime;
            if (posicionInicial.x + distancia > rb.position.x)
            {
                rb.MovePosition(newPosition);
            }
            else
            {
                izq = true;
                newPosition= rb.position + Vector2.left*velocidad*Time.fixedDeltaTime;
                rb.MovePosition(newPosition);

            }
        }
    }

}
