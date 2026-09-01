using UnityEngine;
using System.IO; // Essential for File operations

public static class CharacterLoader
{
    public static CharacterRoster Load()
    {
        // 1. Construct the path for any platform (PC, Mac, etc.)
        string path = Path.Combine(Application.streamingAssetsPath, "characters.json");

        // 2. Check if the file actually exists to avoid a crash
        if (!File.Exists(path))
        {
            Debug.LogError($"[CharacterLoader] File not found at: {path}");
            return null;
        }

        // 3. Read the entire text file
        string json = File.ReadAllText(path);

        // 4. Convert the JSON string into our C# CharacterRoster object
        CharacterRoster roster = JsonUtility.FromJson<CharacterRoster>(json);

        if (roster == null || roster.characters == null)
        {
            Debug.LogError("[CharacterLoader] Failed to parse JSON. Check your class field names!");
            return null;
        }

        Debug.Log($"[CharacterLoader] Successfully loaded {roster.characters.Count} characters.");
        return roster;
    }
}