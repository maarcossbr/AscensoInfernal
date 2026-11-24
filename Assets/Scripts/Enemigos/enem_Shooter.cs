using System;
using UnityEngine;

public class enem_Shooter : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Prefabs")]
    public GameObject balaPrefab;
    public Transform puntoDisparo;
    [Header("Configuracion Proyectil")]
    public float velocidadBala = 5f;
    public float velocidadBalaCargada=15f;
    public float tiempoEntreDisparos = 2f;
    

    private float cronometro;
    private int n_disparos = 0;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        cronometro += Time.deltaTime;
        if (cronometro >= tiempoEntreDisparos)
        {
            if (n_disparos >= 3)
            {
                Disparar(true);
                n_disparos=0;
            }
            else
            {
                Disparar(false);
                n_disparos++;
            }
            cronometro = 0f;
        }

    }
    void Disparar(Boolean tiroCargado)
    {
        GameObject bala=Instantiate(balaPrefab, puntoDisparo.position, puntoDisparo.rotation);
        bullet scriptBala =bala.GetComponent<bullet>();
        if (tiroCargado)
        {
            scriptBala.velocidad=velocidadBalaCargada;
        }
        else
        {
            scriptBala.velocidad=velocidadBala;
        }
        
    }
}
