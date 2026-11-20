using UnityEngine;

public class RecogidaDobleSalto : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start(){
    if (CheckpointRespawn.DobleSaltoGuardado())
    {
        gameObject.SetActive(false);
    }
}
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerMovement movimiento =
            other.GetComponent<PlayerMovement>();

        if (movimiento == null)
            return;

        movimiento.DesbloquearDobleSalto();

        gameObject.SetActive(false);
    }
}
