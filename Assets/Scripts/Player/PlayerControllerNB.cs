using HathoraCloud.Models.Operations;
using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor;
using UnityEngine;
using static MatchManagerSO;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AnticipatedNetworkTransform))]
public class PlayerControllerNB : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private MatchManagerSO matchManagerSO;

    [Header("Settings")]
    [SerializeField][Range(1f, 10f)] float maxMoveSpeed = 5f;
    [SerializeField][Range(1f, 10f)] float hitRange = 5f;

    // Networked variables
    //... Visual Type
    public NetworkVariable<PlayerVisualType> NV_VisualType = new(
        PlayerVisualType.Blue,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    //... Court Side
    public NetworkVariable<CourtSides> NV_playerCourtSide = new(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    //... Player is ready?
    public NetworkVariable<bool> NV_IsReady = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );


    // variables
    private GameState cachedGameState;
    private Vector3 verticalVelocity;
    private Transform cameraTransform;
    private InputController inputController;
    private CharacterController characterController;
    private BallController ballController => matchManagerSO.GetCurrentBallController();

    public float MaxMoveSpeed { get => maxMoveSpeed; }

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
        inputController.OnShot += HandleOnShot;
        GameManagerNB.OnGameStateChanged += HandleOnGameStateChanged;
    }
    private void OnDisable()
    {
        inputController.OnShot -= HandleOnShot;
        GameManagerNB.OnGameStateChanged -= HandleOnGameStateChanged;
    }

    private void HandleOnGameStateChanged(GameState newState)
    {
        cachedGameState = newState;
    }

    // Update is called once per frame
    void Update()
    {
        // Only the owner of this object should control its movement
        if (!IsOwner) return;

        Vector2 input = inputController.MoveInput;
        MovePlayerServerRpc(input);
    }

    // This method is called when the object is spawned on the network
    public override void OnNetworkSpawn()
    {
        // Subscribe to visual type changes and apply the initial visual
        NV_VisualType.OnValueChanged += OnVisualChanged;
        OnVisualChanged(NV_VisualType.Value, NV_VisualType.Value);

        
    }

    private void OnVisualChanged(PlayerVisualType oldVal, PlayerVisualType newVal)
    {
        GetComponentInChildren<PlayerVisual>()?.ApplyVisual(newVal);
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

        clampedPos.x = Mathf.Clamp(clampedPos.x, MatchManagerSO.COURT_GROUND_MIN_X, MatchManagerSO.COURT_GROUND_MAX_X);

        if (MatchManagerSO.GetCourtSideFromPosition(transform.position) == CourtSides.North)
            clampedPos.z = Mathf.Clamp(clampedPos.z, MatchManagerSO.COURT_GROUND_CENTER_Z, MatchManagerSO.COURT_GROUND_MAX_Z);
        else
            clampedPos.z = Mathf.Clamp(clampedPos.z, MatchManagerSO.COURT_GROUND_MIN_Z, MatchManagerSO.COURT_GROUND_CENTER_Z);

        // Apply correction
        Vector3 correction = clampedPos - transform.position;
        if (correction.magnitude > 0f)
            characterController.Move(correction);
    }

    private void HandleOnShot()
    {
        // Only the owner of this object should handle shots
        if (!IsOwner) return;

        // When not playing, set player as ready
        if (cachedGameState != GameState.Playing)
        {
            RequestReadyServerRpc();
            return;
        }

        // When playing, try to hit the ball
        if (Vector3.Distance(transform.position, ballController.gameObject.transform.position) < hitRange)
        {
            ballController.ApplyShot(GetTargetPoint());
        }
    }

    [ServerRpc]
    private void RequestReadyServerRpc(ServerRpcParams rpcParams = default)
    {
        if (NV_IsReady.Value)
            return;

        NV_IsReady.Value = true;
    }

    Vector3 GetTargetPoint()
    {
        CourtSides playerCourtSide = MatchManagerSO.GetCourtSideFromPosition(transform.position);
        CourtSides targetCourtSide = MatchManagerSO.GetOppositeCourtSide(playerCourtSide);
        float internalMargin = 1f;

        return MatchManagerSO.GetRandomPositionInsideCourtSide(targetCourtSide, internalMargin);

    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(transform.position, hitRange);
    }
    #endif

}
