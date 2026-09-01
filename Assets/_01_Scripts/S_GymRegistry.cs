using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class GymEntry
{
    public string displayName;
    public string scenePath;
}

[CreateAssetMenu(menuName = "GP1/GYM Registry", fileName = "S_GymRegistry")]
public class S_GymRegistry : ScriptableObject
{
    public List<GymEntry> scenes = new List<GymEntry>();
}