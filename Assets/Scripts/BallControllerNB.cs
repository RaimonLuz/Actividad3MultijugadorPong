using TMPro;
using UnityEngine;
using Unity.Netcode;

public class BallControllerNB : NetworkBehaviour
{

    [SerializeField] private MatchManagerSO matchManagerSO;

    const float NET_HEIGHT_WITH_MARGIN = MatchManagerSO.NET_HEIGHT + 2f;
    private Rigidbody rb;


    private void Awake()
    {
        matchManagerSO.SetCurrentBallController(this);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        matchManagerSO.OnShotBall += HandleShotBall;
    }
    private void OnDisable()
    {
        matchManagerSO.OnShotBall -= HandleShotBall;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            rb.isKinematic = false; // real physics in server
        }
        else
        {
            rb.isKinematic = true;  // only transform
        }
    }


    private void HandleShotBall(Vector3 targetPosition)
    {
        // Handle the event here
        ApplyShot(targetPosition);

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

    public void PlaceForServe(Transform serverPlayerTransform)
    {
        if (!IsServer) return;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Vector3 offset = serverPlayerTransform.forward * 1.2f;
        offset.y = 1.2f;

        transform.position = serverPlayerTransform.position + offset;
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;

        // cayó al suelo
        if (transform.position.y <= MatchManagerSO.COURT_GROUND_Y - 1f)
        {
            CourtSides side = MatchManagerSO.GetCourtSideFromPosition(transform.position);
            Players winner =
                side == CourtSides.North ? Players.PlayerB : Players.PlayerA;

            matchManagerSO.NotifyPoint(winner);
        }
    }
}
