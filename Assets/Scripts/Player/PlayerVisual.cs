using UnityEngine;
using Unity.Netcode;

public class PlayerVisual : NetworkBehaviour
{
    [SerializeField] private PlayerVisualDatabaseSO playerVisualDatabaseSO;
    [SerializeField] private MeshRenderer eyesMeshRenderer;
    //[SerializeField] private SkinnedMeshRenderer skinMeshRenderer;
    //[SerializeField] private Animator animator;

    private PlayerControllerNB playerController;
    private MeshRenderer skinMeshRenderer;

    private void Awake()
    {
        playerController = GetComponentInParent<PlayerControllerNB>();
        skinMeshRenderer = GetComponent<MeshRenderer>();
    }

    public override void OnNetworkSpawn()
    {
        ApplyVisual(playerController.NV_VisualType.Value);
        playerController.NV_VisualType.OnValueChanged += OnVisualChanged;
    }

    private void OnVisualChanged(PlayerVisualType oldType, PlayerVisualType newType)
    {
        ApplyVisual(newType);
    }

    public void ApplyVisual(PlayerVisualType type)
    {
        PlayerVisualProfileSO profile = playerVisualDatabaseSO.GetProfile(type);
        if (profile == null) { 
            Debug.LogWarning($"PlayerVisual: No profile found for type {type}");
            return;
        }

        // Apply mesh if available
        /*
        if (profile.bodyMesh != null)
        {
            skinMeshRenderer.sharedMesh = profile.bodyMesh;
        }
        */

        // Apply material
        skinMeshRenderer.material = profile.bodyMaterial;
        eyesMeshRenderer.material = profile.bodyMaterial;
        /*
        // Apply animator if available
        if (animator != null && profile.animator != null)
            animator.runtimeAnimatorController = profile.animator;
        */
    }
}
