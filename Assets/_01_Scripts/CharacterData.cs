using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CharacterData
{
    public string id;
    public string displayName;
    public string colorHex; 
    public CharacterStats stats;
}

[Serializable]
public class CharacterStats
{
    [Header("Base Primary Stats")]
    public float moveSpeed;
    public float maxHp = 100f;   
    public float defense = 0f;   

    [Header("Progression Scales (Phase 8)")]
    [Tooltip("Amount of flat max HP added to the pool on a level up event.")]
    public float hpGrowth = 15f;

    [Tooltip("Amount of defense added to the calculations on a level up event.")]
    public float defGrowth = 2f;

    [Tooltip("Amount of flat move velocity added to the controller on a level up event.")]
    public float speedGrowth = 0.1f;

    // ====================================================================
    // ⚔️ COMBAT CONFIGURATION FIELDS (PHASE 11)
    // ====================================================================
    [Header("Combat Stats")]
    [Tooltip("Base damage dealt by the character's weapon system attacks. Read by S15_WeaponBase on Awake.")]
    public float attackPower = 20f;

    [Tooltip("Matches WeaponType enum: 'Projectile' or 'Proximity'. Kept as a string for JSON readability.")]
    public string weaponType = "Proximity";
    
    // ====================================================================
    // 🗡️ STARTING WEAPON CONFIGURATION
    // ====================================================================
    [Header("Starting Weapon")]
    [Tooltip("The starting weapon for this character (Smith's Sword, Small Shield, Roc's Cape, etc.)")]
    public string startingWeapon = "Smith's Sword";
}

[Serializable]
public class CharacterRoster
{
    public List<CharacterData> characters;
}