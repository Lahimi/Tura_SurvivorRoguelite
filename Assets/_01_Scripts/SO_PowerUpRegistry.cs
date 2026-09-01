using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GP1_PowerUpRegistry", menuName = "GP1/Power Up Registry")]
public class SO_PowerUpRegistry : ScriptableObject
{
    public List<SO_PowerUp> allPowerUps = new();

    public List<SO_PowerUp> GetAvailable(int level, Dictionary<SO_PowerUp, int> acquired)
    {
        var available = new List<SO_PowerUp>();
        foreach (var p in allPowerUps)
        {
            if (p == null)            continue;
            if (p.minLevel > level)   continue;   // not unlocked yet

            int stacks = acquired.ContainsKey(p) ? acquired[p] : 0;
            if (stacks >= p.maxStacks) continue;  // fully stacked

            available.Add(p);
        }
        return available;
    }
}