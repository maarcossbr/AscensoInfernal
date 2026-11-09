
using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
public class LavaPlayerDeath : MonoBehaviour
{
    private PlayerHealth health;

    private void Awake()
    {
        health = GetComponent<PlayerHealth>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        LavaController lava =
            other.GetComponentInParent<LavaController>();

        if (lava == null)
            return;

        health.KillInstantly();
    }
}
