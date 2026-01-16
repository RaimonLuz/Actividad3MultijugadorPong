using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManagerNB : NetworkBehaviour
{
    [Header("Config")]
    [SerializeField] private MatchManagerSO matchManagerSO;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform northSpawn;
    [SerializeField] private Transform southSpawn;
    [SerializeField] private Transform serverNorthSpawn;
    [SerializeField] private Transform serverSouthSpawn;

    // Constants
    private const int MAX_PLAYERS = 2;

    // Variables
    private int currentPlayerCount = 0;
    private Dictionary<ulong, GameObject> spawnedPlayers = new(); // Track spawned players by their client IDs

    // Network Variables
    private NetworkVariable<GameState> NV_GameState =
    new(
        GameState.WaitingForAllPlayersConnected,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // events
    public static event Action<GameState> OnGameStateChanged;




    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            // Subscribe to game state changes
            NV_GameState.OnValueChanged += HandleMatchStateChanged;

            // Immediately notify local systems on spawn
            HandleMatchStateChanged(NV_GameState.Value, NV_GameState.Value);
        }

        if (IsServer)
        {
            // Only the server should handle client connections
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void HandleMatchStateChanged(GameState oldState, GameState newState)
    {
        // Client-side handler for game state changes
        if(IsClient)
        {
            OnGameStateChanged?.Invoke(newState);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // Prevent spawning multiple players for the same client
        if (spawnedPlayers.ContainsKey(clientId)) return;

        // Check if maximum player count is reached
        if (currentPlayerCount >= MAX_PLAYERS) return;

        // Determine spawn point and visual type based on existing players
        Transform spawnPoint = spawnedPlayers.Count == 0 ? northSpawn : southSpawn;
        PlayerVisualType visualType = spawnedPlayers.Count == 0 ? PlayerVisualType.Blue : PlayerVisualType.Red;
        CourtSides courtSide = spawnedPlayers.Count == 0 ? CourtSides.North : CourtSides.South;

        // Instantiate and spawn the player object
        Vector3 spawnPosition = new Vector3(spawnPoint.position.x, playerPrefab.GetComponent<CharacterController>().height / 2f, spawnPoint.position.z);
        GameObject playerObj = Instantiate(playerPrefab, spawnPosition, spawnPoint.rotation);
        playerObj.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

        // Set the player's visual type
        PlayerControllerNB playerController = playerObj.GetComponent<PlayerControllerNB>();
        playerController.NV_VisualType.Value = visualType;
        // Set the player's court side
        playerController.NV_playerCourtSide.Value = courtSide;
        // Set player is not ready initially
        playerController.NV_IsReady.Value = false;
        // Subscribe to player's ready state changes
        playerController.NV_IsReady.OnValueChanged += OnPlayerReadyChanged;

        // Track the spawned player
        spawnedPlayers.Add(clientId, playerObj);

        // Increment player count
        currentPlayerCount++;


        if (currentPlayerCount == MAX_PLAYERS)
        {
            NV_GameState.Value = GameState.WaitingForAllPlayersReady;
        }
    }


    private void OnPlayerReadyChanged(bool oldValue, bool newValue)
    {
        if (!IsServer)
            return;

        if (newValue)
            CheckAllPlayersReady();
    }

    private void CheckAllPlayersReady()
    {
        if (NV_GameState.Value != GameState.WaitingForAllPlayersReady)
            return;

        foreach (var player in spawnedPlayers.Values)
        {
            var pc = player.GetComponent<PlayerControllerNB>();
            if (!pc.NV_IsReady.Value)
                return;
        }

        NV_GameState.Value = GameState.Playing;
    }

}
