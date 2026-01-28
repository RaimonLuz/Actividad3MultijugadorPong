using UnityEngine;

[CreateAssetMenu(fileName = "MyPlayerVisualProfileSO", menuName = "Tenis Scriptable Objects/PlayerVisualProfileSO")]
public class PlayerVisualProfileSO : ScriptableObject
{
    public PlayerVisualType type;
    public Material bodyMaterial;
    public Mesh bodyMesh;
    public RuntimeAnimatorController animator;
}
