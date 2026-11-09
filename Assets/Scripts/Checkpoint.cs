
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerCheckpoint playerCheckpoint =
            other.GetComponent<PlayerCheckpoint>();

        if (playerCheckpoint == null)
            return;

        Vector3 position = respawnPoint != null
            ? respawnPoint.position
            : transform.position;

        playerCheckpoint.ActivateCheckpoint(position);
    }
}
