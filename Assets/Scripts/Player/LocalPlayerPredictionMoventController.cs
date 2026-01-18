using Unity.Netcode;
using UnityEngine;

public class LocalPlayerPredictionMoventController : MonoBehaviour
{
    [SerializeField][Range(1f, 30f)] private float reconciliationLerpSpeed = 15f;

    private PlayerControllerNB playerController;
    private Transform serverRoot;
    private InputController input;

    private void Awake()
    {
        serverRoot = transform.parent;
        input = GetComponentInParent<InputController>();
        playerController = GetComponentInParent<PlayerControllerNB>();
    }

    private void Update()
    {
        // Ensure input is available
        if (!input) return;

        /*
        if(playerController.NV_gameState_Ref.Value != GameState.PlayingServe)
        {
            return;
        }
        */

        // Movement local prediction
        Vector3 move = new Vector3(
            input.MoveInput.x,
            0f,
            input.MoveInput.y
        ) * playerController.MaxMoveSpeed * Time.deltaTime;

        transform.position += move;

        // Reconciliation towards server position
        transform.position = Vector3.Lerp(
            transform.position,
            serverRoot.position,
            reconciliationLerpSpeed * Time.deltaTime
        );
    }
}
