using UnityEngine;

[CreateAssetMenu(fileName = "MyPlayerVisualDatabaseSO", menuName = "Tenis Scriptable Objects/PlayerVisualDatabaseSO")]
public class PlayerVisualDatabaseSO : ScriptableObject
{
    public PlayerVisualProfileSO[] profiles;

    public PlayerVisualProfileSO GetProfile(PlayerVisualType type)
    {
        foreach (var p in profiles)
            if (p.type == type)
                return p;

        return null;
    }
}