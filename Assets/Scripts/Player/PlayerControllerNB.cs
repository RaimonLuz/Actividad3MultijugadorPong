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
    //... Game State PRIVATE, it is a reference to the GameManagerNB's game state variable
    private NetworkVariable<GameState> nV_gameState_Ref = new(
        GameState.WaitingForAllPlayersConnected,
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
    public GameState CachedGameState { get => cachedGameState; }

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
        // Subscribe to visual type changes
        NV_VisualType.OnValueChanged += OnVisualChanged;
        OnVisualChanged(NV_VisualType.Value, NV_VisualType.Value);

        // Subscribe to ready state changes (only for owner)
        if (IsOwner)
        {
            NV_IsReady.OnValueChanged += HandleReadyChanged;
        }
    }

    public override void OnNetworkDespawn()
    {
        // Unsubscribe from visual type changes
        NV_VisualType.OnValueChanged -= OnVisualChanged;

        // Unsubscribe from ready state changes (only for owner)
        if (IsOwner)
        {
            NV_IsReady.OnValueChanged -= HandleReadyChanged;
        }
    }

    public void InitGameState(NetworkVariable<GameState> gameState)
    {
        if (!IsServer) return;
        nV_gameState_Ref = gameState;
    }

    private void HandleReadyChanged(bool oldValue, bool newValue)
    {
        // Only the owner of this object
        if (!IsOwner)
            return;

        // When set to ready, notify the match manager locally
        if (newValue)
        {
            matchManagerSO.NotifyThisPlayerIsReady();
        }
    }

    private void HandleOnGameStateChanged(GameState newState)
    {
        cachedGameState = newState;
    }

    

    private void OnVisualChanged(PlayerVisualType oldVal, PlayerVisualType newVal)
    {
        GetComponentInChildren<PlayerVisual>()?.ApplyVisual(newVal);
    }


    [ServerRpc]
    private void MovePlayerServerRpc(Vector2 input, ServerRpcParams rpcParams = default)
    {
        if (nV_gameState_Ref.Value == GameState.WaitingForAllPlayersConnected)
        {
            RotatePlayer(input);
        }
        else if (nV_gameState_Ref.Value == GameState.Playing)
        {
            MovePlayer(input);
        }
    }

    private void RotatePlayer(Vector2 input)
    { 
        float threshold = 0.1f;
        float turnSpeed = 100f;

        if (MathF.Abs(input.x) > 0f + threshold)
        {
            transform.Rotate(Vector3.up * Time.deltaTime * turnSpeed * input.x);
        }
        else if (MathF.Abs(input.y) > 0f + threshold)
        {
            transform.Rotate(Vector3.up * Time.deltaTime * turnSpeed * input.y);
        }
    }


    private void MovePlayer(Vector2 input)
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
        if (cachedGameState == GameState.WaitingForAllPlayersReady && NV_IsReady.Value == false)
        {
            RequestReadyServerRpc();
            return;
        }

        if(cachedGameState == GameState.Playing)
        {
            TryHitBall();
        }
    }

    void TryHitBall()
    {

        // When playing, try to hit the ball
        if (Vector3.Distance(transform.position, ballController.gameObject.transform.position) < hitRange)
        {
            ballController.ApplyShot(GetTargetPoint());
        }

        float distanceToBall = Vector3.Distance(transform.position, ballController.transform.position);
        if (distanceToBall <= hitRange)
        {
            Vector3 targetPoint = GetTargetPoint();
            matchManagerSO.NotifyShotBall(targetPoint);
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
