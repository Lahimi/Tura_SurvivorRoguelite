using UnityEngine;

[CreateAssetMenu(menuName = "GP1/Game Session", fileName = "SO_GameSession")]
public class SO_GameSession : ScriptableObject
{
    [System.NonSerialized]
    public CharacterData selectedCharacter;

    // This stores the destination scene name (e.g., "CameraGYM" or "MovementGYM")
    [System.NonSerialized]
    public string targetGymScene; 

    /// <summary>
    /// 🔧 Developer Fallback: Ensures that running an isolated testing gym scene 
    /// without passing through the character selection screen doesn't break data references.
    /// </summary>
    public void EnsureValidSelection()
    {
        if (selectedCharacter == null)
        {
            Debug.LogWarning("[Session Fallback] No character selected. Direct scene execution detected. Injecting default character data from JSON database.");
            
            var roster = CharacterLoader.Load();
            if (roster != null && roster.characters != null && roster.characters.Count > 0)
            {
                selectedCharacter = roster.characters[0];
                Debug.Log($"[Session Fallback] Successfully assigned fallback profile: {selectedCharacter.displayName}");
            }
            else
            {
                Debug.LogError("[Session Fallback] Critical Error: Failed to retrieve default profiles from characters.json.");
            }
        }
    }
}