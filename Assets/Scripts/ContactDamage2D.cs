
using UnityEngine;

public class ContactDamage2D : MonoBehaviour
{
    [Header("Daño al jugador")]
    [SerializeField] private int damage = 1;

    private void OnTriggerStay2D(Collider2D other)
    {
        ApplyDamage(other);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        ApplyDamage(collision.collider);
    }

    private void ApplyDamage(Collider2D other)
    {
        PlayerHealth health =
            other.GetComponentInParent<PlayerHealth>();

        if (health == null)
            return;

        health.TakeDamage(damage);
    }
}
