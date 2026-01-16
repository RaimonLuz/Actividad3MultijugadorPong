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

    private Dictionary<ulong, GameObject> spawnedPlayers = new();

    public override void OnNetworkSpawn()
    {
        // Only the server should handle player spawning
        if (!IsServer) return;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        // Prevent spawning multiple players for the same client
        if (spawnedPlayers.ContainsKey(clientId)) return;

        // Determine spawn point and visual type based on existing players
        Transform spawnPoint = spawnedPlayers.Count == 0 ? northSpawn : southSpawn;
        PlayerVisualType visualType = spawnedPlayers.Count == 0 ? PlayerVisualType.Blue : PlayerVisualType.Red;
        CourtSides courtSide = spawnedPlayers.Count == 0 ? CourtSides.North : CourtSides.South;


        // Instantiate and spawn the player object
        GameObject playerObj = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        playerObj.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

        // Set the player's visual type
        PlayerControllerNB playerController = playerObj.GetComponent<PlayerControllerNB>();
        playerController.NV_VisualType.Value = visualType;

        // Set the player's court side for camera control
        PlayerCameraControllerNB playerCameraController = playerObj.GetComponent<PlayerCameraControllerNB>();
        playerCameraController.playerCourtSide.Value = courtSide;

        // Track the spawned player
        spawnedPlayers.Add(clientId, playerObj);
    }
}
