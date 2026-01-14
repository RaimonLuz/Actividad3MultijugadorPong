using UnityEngine;

public class BallController : MonoBehaviour
{

    const float NET_HEIGHT_WITH_MARGIN = GameManagerSO.NET_HEIGHT + 2f;
    private Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void ApplyShot(Vector3 targetPosition) 
    {

        rb.linearVelocity = CalculateBallisticVelocity(transform.position, targetPosition, NET_HEIGHT_WITH_MARGIN);
    }

    Vector3 CalculateBallisticVelocity(
    Vector3 start,
    Vector3 target,
    float maxHeight)
    {
        float gravity = Physics.gravity.y;

        float displacementY = target.y - start.y;
        Vector3 displacementXZ = new Vector3(
            target.x - start.x,
            0,
            target.z - start.z
        );

        float timeUp = Mathf.Sqrt(-2 * maxHeight / gravity);
        float timeDown = Mathf.Sqrt(2 * (displacementY - maxHeight) / gravity);
        float totalTime = timeUp + timeDown;

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * maxHeight);
        Vector3 velocityXZ = displacementXZ / totalTime;

        return velocityXZ + velocityY;
    }
}
