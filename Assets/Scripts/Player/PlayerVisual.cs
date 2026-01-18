using UnityEngine;
using Unity.Netcode;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private PlayerVisualDatabaseSO playerVisualDatabaseSO;
    [SerializeField] private MeshRenderer eyesMeshRenderer;
    //[SerializeField] private SkinnedMeshRenderer skinMeshRenderer;
    //[SerializeField] private Animator animator;

    private MeshRenderer skinMeshRenderer;

    private void Awake()
    {
        skinMeshRenderer = GetComponent<MeshRenderer>();
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
