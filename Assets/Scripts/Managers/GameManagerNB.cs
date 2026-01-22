using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
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

    // private Variables
    private int currentPlayerCount = 0;
    private Dictionary<ulong, GameObject> spawnedPlayers = new(); // Track spawned players by their client IDs

    // Network Variables
    private NetworkVariable<GameState> nv_GameState =
    new(
        GameState.WaitingForAllPlayersConnected,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    private NetworkVariable<Players> nv_ServerSide =
    new(
        Players.PlayerA,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // --- Networked Score ---
    private NetworkVariable<int> nv_PointsPlayerA = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    ); // 0,1,2,3 = 0,15,30,40
    private NetworkVariable<int> nv_PointsPlayerB = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    private NetworkVariable<int> nv_GamesPlayerA = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    private NetworkVariable<int> nv_GamesPlayerB = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );


    // Getters for network variables
    public GameState CurrentGameState { get => nv_GameState.Value; }



    // static events
    public static event Action<GameState> OnGameStateChanged;
    public static event Action<int> OnPointsPlayerAChanged;
    public static event Action<int> OnPointsPlayerBChanged;
    public static event Action<int> OnGamesPlayerAChanged;
    public static event Action<int> OnGamesPlayerBChanged;

    private void Awake()
    {
        // Register this game manager in the match manager scriptable object
        matchManagerSO.RegisterGameManager(this);
    }


    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            // Subscribe to game state changes
            nv_GameState.OnValueChanged += HandleMatchStateChanged;
            nv_PointsPlayerA.OnValueChanged += HandlePointsPlayerAChanged;
            nv_PointsPlayerB.OnValueChanged += HandlePointsPlayerBChanged;
            nv_GamesPlayerA.OnValueChanged += HandleGamesPlayerAChanged;
            nv_GamesPlayerB.OnValueChanged += HandleGamesPlayerBChanged;

            // Immediately notify local systems on spawn
            HandleMatchStateChanged(nv_GameState.Value, nv_GameState.Value);
        }

        if (IsServer)
        {
            // Only the server should handle client connections
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            nv_GameState.OnValueChanged -= HandleMatchStateChanged;
            nv_PointsPlayerA.OnValueChanged -= HandlePointsPlayerAChanged;
            nv_PointsPlayerB.OnValueChanged -= HandlePointsPlayerBChanged;
            nv_GamesPlayerA.OnValueChanged -= HandleGamesPlayerAChanged;
            nv_GamesPlayerB.OnValueChanged -= HandleGamesPlayerBChanged;
        }

        if (IsServer)
        {
            spawnedPlayers.Clear();

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }
    }

    private void OnEnable()
    {
        matchManagerSO.OnPlayerIsReady += HandleOnPlayerIsReady;
    }

    private void OnDisable()
    {
        matchManagerSO.OnPlayerIsReady -= HandleOnPlayerIsReady;
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (!spawnedPlayers.TryGetValue(clientId, out var playerObj))
            return;

        // Remove the player object from the dictionary and decrement player count
        spawnedPlayers.Remove(clientId);
        currentPlayerCount--;
    }


    private void HandleMatchStateChanged(GameState oldState, GameState newState)
    {
        // Client-side handler for game state changes
        if(IsClient)
        {
            OnGameStateChanged?.Invoke(newState);
        }
    }

    private void HandlePointsPlayerAChanged(int previousValue, int newValue)
    {
        if (IsClient)
        {
            OnPointsPlayerAChanged?.Invoke(newValue);
        }
    }
    private void HandlePointsPlayerBChanged(int previousValue, int newValue)
    {
        if (IsClient)
        {
            OnPointsPlayerBChanged?.Invoke(newValue);
        }
    }
    private void HandleGamesPlayerAChanged(int previousValue, int newValue)
    {
        if (IsClient)
        {
            OnGamesPlayerAChanged?.Invoke(newValue);
        }
    }
    private void HandleGamesPlayerBChanged(int previousValue, int newValue)
    {
        if(IsClient)
        {
            OnGamesPlayerBChanged?.Invoke(newValue);
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
        Players player = spawnedPlayers.Count == 0 ? Players.PlayerA : Players.PlayerB;
        PlayerVisualType visualType = spawnedPlayers.Count == 0 ? PlayerVisualType.Blue : PlayerVisualType.Red;
        CourtSides courtSide = spawnedPlayers.Count == 0 ? CourtSides.North : CourtSides.South;

        // Instantiate and spawn the player object
        Vector3 spawnPosition = new Vector3(spawnPoint.position.x, playerPrefab.GetComponent<CharacterController>().height / 2f, spawnPoint.position.z);
        GameObject playerObj = Instantiate(playerPrefab, spawnPosition, spawnPoint.rotation);
        playerObj.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

        PlayerControllerNB playerController = playerObj.GetComponent<PlayerControllerNB>();
        // Set the player's value
        playerController.AsignPlayer(player);
        // Set the player's visual type
        playerController.AsignVisual(visualType);

        // Track the spawned player
        spawnedPlayers.Add(clientId, playerObj);

        // Increment player count
        currentPlayerCount++;


        if (currentPlayerCount == MAX_PLAYERS)
        {
            ChangeGameState(GameState.WaitingForAllPlayersReady);
        }
    }


    public void HandleOnPlayerIsReady(ulong clientId)
    {
        if (!IsServer)
            return;

        // Set the player's ready state to true
        spawnedPlayers[clientId].GetComponent<PlayerControllerNB>().SetReadyState(true);

        // Check if all players are ready
        CheckAllPlayersReady();
    }

    private void CheckAllPlayersReady()
    {
        if (!IsServer)
            return;

        // Only proceed if we are in the waiting for all players ready state
        if (nv_GameState.Value != GameState.WaitingForAllPlayersReady)
            return;

        // Check if all players are ready
        foreach (var player in spawnedPlayers.Values)
        {
            if (player.GetComponent<PlayerControllerNB>().IsReady() == false)
                return;
        }

        // Randomly assign server side player
        nv_ServerSide.Value = UnityEngine.Random.value > 0.5f ? Players.PlayerA : Players.PlayerB;

        // Reset points and games
        nv_PointsPlayerA.Value = 0;
        nv_PointsPlayerB.Value = 0;
        nv_GamesPlayerA.Value = 0;
        nv_GamesPlayerB.Value = 0;

        // Reposition players to server spawn points
        foreach (var player in spawnedPlayers.Values)
        {
            var playerController = player.GetComponent<PlayerControllerNB>();
            if (playerController.Player == nv_ServerSide.Value)
            {
                // Server side player
                player.transform.position = new Vector3(serverNorthSpawn.position.x, playerPrefab.GetComponent<CharacterController>().height / 2f, serverNorthSpawn.position.z);
                player.transform.rotation = serverNorthSpawn.rotation;
            }
            else
            {
                // Receiver side player
                player.transform.position = new Vector3(serverSouthSpawn.position.x, playerPrefab.GetComponent<CharacterController>().height / 2f, serverSouthSpawn.position.z);
                player.transform.rotation = serverSouthSpawn.rotation;
            }
        }

        // All players are ready, start the game
        ChangeGameState(GameState.PlayingServe);
    }

    private void ChangeGameState(GameState newState)
    {
        if (!IsServer)
            return;

        Debug.Log($"Game State changed to: {newState}");
        nv_GameState.Value = newState;
    }

    private void OnGameWon(Players winner)
    {
        if (!IsServer)
            return;

        if (winner == Players.PlayerA)
            nv_GamesPlayerA.Value++;
        else
            nv_GamesPlayerB.Value++;

        SwitchServer();
    }

    private void SwitchServer()
    {
        if (!IsServer)
            return;

        nv_ServerSide.Value =
            nv_ServerSide.Value == Players.PlayerA
                ? Players.PlayerB
                : Players.PlayerA;
    }
}
