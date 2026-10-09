using UnityEngine;

public class LavaController : MonoBehaviour
{
    public float baseSpeed = 2f;
    
    public float acceleration = 0.05f;

    public float currentSpeed;

    private void Start()
    {
        currentSpeed = baseSpeed;
    }

    private void Update()
    {
        currentSpeed += acceleration * Time.deltaTime;

        transform.Translate(Vector3.up * currentSpeed * Time.deltaTime);
    }
    private void OnTriggerEnter(Collider collision)
    {
    if (collision.CompareTag("Player"))
    {
        Destroy(collision.gameObject);
    }
    }
}

