using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class LocalPlayerPredictionMoventController : MonoBehaviour
{
    [SerializeField][Range(1f, 30f)] private float reconciliationLerpSpeed = 15f;

    private PlayerControllerNB playerController;
    private Transform serverRoot;

    private void Awake()
    {
        serverRoot = transform.parent;
        input = GetComponentInParent<InputController>();
        playerController = GetComponentInParent<PlayerControllerNB>();
    }

    private void Update()
    {
        // Reconciliation towards server position
        transform.position = Vector3.Lerp(
            transform.position,
            serverRoot.position,
            reconciliationLerpSpeed * Time.deltaTime
        );
    }

    public void PredictMove(Vector2 input) 
    {
        // Movement local prediction
        Vector3 move = new Vector3(
            input.x,
            0f,
            input.y
        ) * playerController.MaxMoveSpeed * Time.deltaTime;

        transform.position += move;
    }
}
