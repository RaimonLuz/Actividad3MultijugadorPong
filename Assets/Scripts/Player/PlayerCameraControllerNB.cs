using Unity.Netcode;
using UnityEngine;

public class PlayerCameraControllerNB : NetworkBehaviour
{
    [SerializeField] private MatchManagerSO matchManagerSO;

    // Networked variable to store the player's court side
    public NetworkVariable<CourtSides> playerCourtSide = new(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return; // Only activate camera for the owning client

        // Subscribe to NetworkVariable changes (in case server sets it after spawn)
        playerCourtSide.OnValueChanged += OnCourtSideChanged;

        // Immediately apply current value
        matchManagerSO.NotifyActivateCamera(playerCourtSide.Value);
    }

    private void OnCourtSideChanged(CourtSides oldSide, CourtSides newSide)
    {
        matchManagerSO.NotifyActivateCamera(newSide);
    }

    private void OnDestroy()
    {
        playerCourtSide.OnValueChanged -= OnCourtSideChanged;
    }
}
