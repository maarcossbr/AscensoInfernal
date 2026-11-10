
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerDamageFlash : MonoBehaviour
{
    [Header("Efecto de daño")]
    [SerializeField] private float duracion = 1f;
    [SerializeField] private float intervalo = 0.12f;

    private PlayerHealth health;
    private SpriteRenderer sprite;
    private Color colorOriginal;
    private Coroutine parpadeoActual;
    private int vidaAnterior;

    private void Awake()
    {
        health = GetComponent<PlayerHealth>();
        sprite = GetComponent<SpriteRenderer>();

        colorOriginal = sprite.color;
        vidaAnterior = health.CurrentHealth;
    }

    private void OnEnable()
    {
        vidaAnterior = health.CurrentHealth;
        health.HealthChanged += AlCambiarVida;
    }

    private void OnDisable()
    {
        health.HealthChanged -= AlCambiarVida;

        if (parpadeoActual != null)
        {
            StopCoroutine(parpadeoActual);
            parpadeoActual = null;
        }

        sprite.color = colorOriginal;
    }

    private void AlCambiarVida(int actual, int maxima)
    {
        bool recibioDanio =
            actual < vidaAnterior && actual > 0;

        vidaAnterior = actual;

        if (!recibioDanio)
        {
            if (parpadeoActual != null)
            {
                StopCoroutine(parpadeoActual);
                parpadeoActual = null;
            }

            sprite.color = colorOriginal;
            return;
        }

        if (parpadeoActual != null)
            StopCoroutine(parpadeoActual);

        parpadeoActual = StartCoroutine(Parpadear());
    }

    private IEnumerator Parpadear()
    {
        float tiempo = 0f;
        bool transparente = false;

        while (tiempo < duracion)
        {
            transparente = !transparente;

            Color color = colorOriginal;
            color.a *= transparente ? 0.35f : 1f;
            sprite.color = color;

            yield return new WaitForSeconds(intervalo);

            tiempo += intervalo;
        }

        sprite.color = colorOriginal;
        parpadeoActual = null;
    }
}
