using UnityEngine;

public class BallController : MonoBehaviour
{

    private Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void ApplyShot(Vector3 playerPosition, float hitForce) 
    {
        Vector3 direction = (transform.position-playerPosition).normalized;

        rb.AddForce(direction * hitForce, ForceMode.Impulse);
    }
}
