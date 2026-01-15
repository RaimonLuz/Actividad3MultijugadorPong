using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(AnticipatedNetworkTransform))]
public class PlayerController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private GameManagerSO gameManagerSO;

    [Header("Settings")]
    [SerializeField][Range(1f, 10f)] float maxMoveSpeed = 5f;
    [SerializeField][Range(1f, 10f)] float hitRange = 5f;

    // variables
    private Vector3 verticalVelocity;
    private Transform cameraTransform;
    private InputController inputController;
    private CharacterController characterController;
    private BallController ballController => gameManagerSO.GetCurrentBallController();

    private void Awake()
    {
        inputController = GetComponent<InputController>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        cameraTransform = Camera.main.transform;
    }

    private void OnEnable()
    {
        inputController.OnShot += OnShot;
    }
    private void OnDisable()
    {
        inputController.OnShot -= OnShot;
    }

    // Update is called once per frame
    void Update()
    {
        // Only the owner of this object should control its movement
        if (!IsOwner) return;

        Vector2 input = inputController.MoveInput;
        MovePlayerServerRpc(input);
    }


    [ServerRpc]
    private void MovePlayerServerRpc(Vector2 input, ServerRpcParams rpcParams = default)
    {
        Vector3 direction = GetInputDirection(input);

        // Movement
        Vector3 move = direction * maxMoveSpeed * Time.deltaTime;
        characterController.Move(move);

        // Gravity
        HandleGravity();

        // Clamp to court
        ClampPlayerToCourt();
    }

    private Vector3 GetInputDirection(Vector2 input)
    {
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        return (camRight * input.x + camForward * input.y).normalized;
    }

    private void HandleGravity()
    {
        if (characterController.isGrounded && verticalVelocity.y < 0)
            verticalVelocity.y = -2f; // Mantener al jugador pegado al suelo
        else
            verticalVelocity.y += Physics.gravity.y * Time.deltaTime;

        characterController.Move(verticalVelocity * Time.deltaTime);
    }

    private void ClampPlayerToCourt()
    {
        Vector3 clampedPos = transform.position;

        clampedPos.x = Mathf.Clamp(clampedPos.x, GameManagerSO.COURT_GROUND_MIN_X, GameManagerSO.COURT_GROUND_MAX_X);

        if (GameManagerSO.GetCourtSideFromPosition(transform.position) == GameManagerSO.CourtSides.North)
            clampedPos.z = Mathf.Clamp(clampedPos.z, GameManagerSO.COURT_GROUND_CENTER_Z, GameManagerSO.COURT_GROUND_MAX_Z);
        else
            clampedPos.z = Mathf.Clamp(clampedPos.z, GameManagerSO.COURT_GROUND_MIN_Z, GameManagerSO.COURT_GROUND_CENTER_Z);

        // Apply correction
        Vector3 correction = clampedPos - transform.position;
        if (correction.magnitude > 0f)
            characterController.Move(correction);
    }

    private void OnShot()
    {
        if (Vector3.Distance(transform.position, ballController.gameObject.transform.position) < hitRange)
        {
            ballController.ApplyShot(GetTargetPoint());
        }
    }

    Vector3 GetTargetPoint()
    {
        GameManagerSO.CourtSides playerCourtSide = GameManagerSO.GetCourtSideFromPosition(transform.position);
        GameManagerSO.CourtSides targetCourtSide = GameManagerSO.GetOppositeCourtSide(playerCourtSide);
        float internalMargin = 1f;

        return GameManagerSO.GetRandomPositionInsideCourtSide(targetCourtSide, internalMargin);

    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(transform.position, hitRange);
    }
    #endif

}
