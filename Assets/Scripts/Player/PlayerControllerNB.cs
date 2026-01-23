using HathoraCloud.Models.Operations;
using System;
using System.Collections;
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
    //... Player Side
    private NetworkVariable<Players> nv_Player = new(
        Players.PlayerA,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    //... Visual Type
    private NetworkVariable<PlayerVisualType> nv_VisualType = new(
        PlayerVisualType.Blue,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    //... Is Ready
    private NetworkVariable<bool> nv_IsReady = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    


    public void SetReadyState(bool isReady)
    {
        if (IsServer)
        {
            nv_IsReady.Value = isReady;
        }
    }

    public bool IsReady()
    {
        return nv_IsReady.Value;
    }

    public void AsignPlayer(Players player)
    {
        if (IsServer)
        {
            nv_Player.Value = player;
        }
    }

    public void AsignVisual(PlayerVisualType visualType)
    {
        if (IsServer)
        {
            // Assign visual based on player side
            if (nv_Player.Value == Players.PlayerA)
            {
                nv_VisualType.Value = PlayerVisualType.Blue;
            }
            else
            {
                nv_VisualType.Value = PlayerVisualType.Red;
            }
        }
    }

    // static events
    public static event Action<Players> OnLocalPlayerIdentified;

    // variables
    private Vector3 verticalVelocity;
    private Transform cameraTransform;
    private InputController inputController;
    private CharacterController characterController;
    private LocalPlayerPredictionMoventController localPlayerPredictionMovent;
    private BallController ballController => matchManagerSO.GetCurrentBallController();

    // properties, getters
    public float MaxMoveSpeed { get => maxMoveSpeed; }
    public Players Player { get => nv_Player.Value; }


    private void Awake()
    {
        inputController = GetComponent<InputController>();
        localPlayerPredictionMovent = GetComponent<LocalPlayerPredictionMoventController>();
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
    }
    private void OnDisable()
    {
        inputController.OnShot -= HandleOnShot;
    }

    // Update is called once per frame
    void Update()
    {
        // Only the owner of this object should control its movement
        if (!IsOwner) return;

        // Get input
        Vector2 input = inputController.MoveInput;

        // Send movement to server
        MovePlayerServerRpc(input);

        // Local prediction
        if (matchManagerSO.GetServerGameState() == GameState.PlayingRally)
        {
            if (localPlayerPredictionMovent != null)
            {
                localPlayerPredictionMovent.PredictMove(input);
            }
            else
            {
                Debug.LogWarning("LocalPlayerPredictionMoventController component is missing.");
            }
        }
    }

    // This method is called when the object is spawned on the network
    public override void OnNetworkSpawn()
    {
        // Subscribe to visual type changes
        nv_VisualType.OnValueChanged += OnVisualChanged;
        nv_Player.OnValueChanged += OnPlayerChanged;
        nv_IsReady.OnValueChanged += HandleReadyChanged;

        OnVisualChanged(nv_VisualType.Value, nv_VisualType.Value);
    }

    public override void OnNetworkDespawn()
    {
        // Unsubscribe from visual type changes
        nv_VisualType.OnValueChanged -= OnVisualChanged;
        nv_Player.OnValueChanged -= OnPlayerChanged;
        nv_IsReady.OnValueChanged -= HandleReadyChanged;
    }



    private void OnPlayerChanged(Players previousValue, Players newValue)
    {
        // If this is the local player, notify via event
        if (IsOwner)
        {
            OnLocalPlayerIdentified?.Invoke(newValue);
        }
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

    

    private void OnVisualChanged(PlayerVisualType oldVal, PlayerVisualType newVal)
    {
        GetComponentInChildren<PlayerVisual>()?.ApplyVisual(newVal);
    }


    [ServerRpc]
    private void MovePlayerServerRpc(Vector2 input, ServerRpcParams rpcParams = default)
    {
        GameState gameState = matchManagerSO.GetServerGameState();

        if (gameState == GameState.WaitingForAllPlayersConnected)
        {
            RotatePlayer(input);
        }
        else if (gameState == GameState.PlayingServe)
        {
            // Don't move during serve
            return;
        }
        else if (gameState == GameState.PlayingRally)
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

    // FIXME : refactor this method to separate concerns SERVER/CLIENT
    private void HandleOnShot()
    {

        // Only the owner of this object should handle shots
        if (!IsOwner) return;
        
        GameState gameState = matchManagerSO.GetServerGameState();

        // When not playing, set player as ready
        if (gameState == GameState.WaitingForAllPlayersReady && nv_IsReady.Value == false)
        {
            SendPlayerReadyToServerRpc();
            return;
        }

        if (gameState == GameState.PlayingServe)
        {
            TryHitBall();
        }
    }

    // FIXME : Refactor this method to separate concerns SERVER/CLIENT
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
    private void SendPlayerReadyToServerRpc(ServerRpcParams rpcParams = default)
    {
        Debug.Log("Player requested to be ready.");
        matchManagerSO.NotifyPlayerReady(OwnerClientId);
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
