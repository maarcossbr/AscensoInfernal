using UnityEngine;

public class EnredaderaCheckpoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        DevolverAlCheckpoint(other.gameObject);
    }

    private void DevolverAlCheckpoint(GameObject objetivo)
    {
        PlayerCheckpoint checkpoint = objetivo.GetComponentInParent<PlayerCheckpoint>();
        if (checkpoint == null || !checkpoint.HasCheckpoint)
            return;

        Rigidbody2D rb = checkpoint.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.position = new Vector2(
                checkpoint.LastCheckpointPosition.x,
                checkpoint.LastCheckpointPosition.y
            );
        }
        else
        {
            checkpoint.transform.position = checkpoint.LastCheckpointPosition;
        }

        Physics2D.SyncTransforms();
    }
}