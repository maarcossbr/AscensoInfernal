
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerHealth))]
public class HealthDebug : MonoBehaviour
{
    private PlayerHealth health;

    private void Awake()
    {
        health = GetComponent<PlayerHealth>();
    }

    private void OnEnable()
    {
        if (health == null)
            health = GetComponent<PlayerHealth>();

        health.HealthChanged += OnHealthChanged;
        health.Died += OnDied;
    }

    private void OnDisable()
    {
        if (health == null)
            return;

        health.HealthChanged -= OnHealthChanged;
        health.Died -= OnDied;
    }

    private void Start()
    {
        Debug.Log($"Vida inicial: {health.CurrentHealth}/{health.MaxHealth}");
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        // J: recibir un punto de daño
        if (keyboard.jKey.wasPressedThisFrame)
        {
            bool damageApplied = health.TakeDamage(1);

            if (!damageApplied)
                Debug.Log("Daño ignorado.");
        }

        // K: muerte instantánea
        if (keyboard.kKey.wasPressedThisFrame)
            health.KillInstantly();

        // R: recuperar toda la vida
        if (keyboard.rKey.wasPressedThisFrame)
            health.RestoreFullHealth();
    }

    private void OnHealthChanged(int current, int max)
    {
        Debug.Log($"Vida actual: {current}/{max}");
    }

    private void OnDied()
    {
        Debug.Log("¡El jugador ha muerto!");
    }
}
