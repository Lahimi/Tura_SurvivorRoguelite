using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(S08_PlayerXP))]
[RequireComponent(typeof(S04_HealthComponent))]
[RequireComponent(typeof(S02_PlayerController))]
[RequireComponent(typeof(S15_WeaponBase))]
public class S11_PlayerPowerUps : MonoBehaviour
{
    [System.Serializable]
    public struct EvolutionRecipe
    {
        public string recipeName;
        public SO_PowerUp baseWeaponAsset;   
        public SO_PowerUp requiredPassiveAsset; 
        public SO_PowerUp evolvedResultAsset;   
    }

    [Header("Registry Linked Asset Pool")]
    [SerializeField] private SO_PowerUpRegistry registry;

    [Header("Evolution System Configuration")]
    [Tooltip("Add your 5 weapon evolution pairs here and assign their respective ScriptableObject assets!")]
    [SerializeField] private List<EvolutionRecipe> evolutionRecipes = new List<EvolutionRecipe>();

    private readonly Dictionary<SO_PowerUp, int> _acquired = new Dictionary<SO_PowerUp, int>();
    public IReadOnlyDictionary<SO_PowerUp, int> Acquired => _acquired;

    private S04_HealthComponent _health;
    private S08_PlayerXP _xp;
    private S02_PlayerController _movement;
    private S15_WeaponBase _weapon;

    // 🛡️ System Link: Orbiting Shield Component Engine
    private WP_OrbitingShields _shieldSystem;

    // 🪃 System Link: Survivor.io Style Boomerang Component Engine
    private WP_BoomerangWeapon _boomerangSystem;

    // ⚔️ System Link: Vampire Survivors Whip Style Sword Component Engine
    private WP_SwordWeapon _swordSystem;

    // 🧤 System Link: Mole Mitts Weapon System
    private WP_MoleMittsWeapon _moleMittsSystem;

    // 🌪️ System Link: Roc's Cape Tornado Weapon System
    private WP_RocsCapeWeapon _rocsCapeSystem;

    // Track which evolved weapons the player actually owns now
    private HashSet<SO_PowerUp> _evolvedWeaponsOwned = new HashSet<SO_PowerUp>();

    private void Awake()
    {
        _health = GetComponent<S04_HealthComponent>();
        _xp = GetComponent<S08_PlayerXP>();
        _movement = GetComponent<S02_PlayerController>();
        _weapon = GetComponent<S15_WeaponBase>();

        // 🛡️ Component lookup for your custom orbiting shields engine script structure
        _shieldSystem = GetComponent<WP_OrbitingShields>();
        if (_shieldSystem == null) _shieldSystem = GetComponentInChildren<WP_OrbitingShields>();

        // 🪃 Component lookup for your custom boomerang weapons engine script structure
        _boomerangSystem = GetComponent<WP_BoomerangWeapon>();
        if (_boomerangSystem == null) _boomerangSystem = GetComponentInChildren<WP_BoomerangWeapon>();

        // ⚔️ Component lookup for your custom whip style sword weapons engine script structure
        _swordSystem = GetComponent<WP_SwordWeapon>();
        if (_swordSystem == null) _swordSystem = GetComponentInChildren<WP_SwordWeapon>();

        // 🧤 Component lookup for Mole Mitts weapon system
        _moleMittsSystem = GetComponent<WP_MoleMittsWeapon>();
        if (_moleMittsSystem == null) _moleMittsSystem = GetComponentInChildren<WP_MoleMittsWeapon>();

        // 🌪️ Component lookup for Roc's Cape weapon system
        _rocsCapeSystem = GetComponent<WP_RocsCapeWeapon>();
        if (_rocsCapeSystem == null) _rocsCapeSystem = GetComponentInChildren<WP_RocsCapeWeapon>();
    }

    public void Apply(SO_PowerUp powerUp)
    {
        if (powerUp == null) return;

        // 1. Track stacks accurately inside the internal collection state
        if (_acquired.ContainsKey(powerUp))
        {
            _acquired[powerUp]++;
        }
        else
        {
            _acquired[powerUp] = 1;
        }

        Debug.Log($"[POWERUP] Applied: {powerUp.displayName} (Current Stack: {_acquired[powerUp]} / {powerUp.maxStacks})");

        // ✅ EVOLUTION CHECK - Check if this power-up is an evolved weapon
        foreach (var recipe in evolutionRecipes)
        {
            if (recipe.evolvedResultAsset == powerUp)
            {
                _evolvedWeaponsOwned.Add(powerUp);
                Debug.Log($"<color=cyan>[EVOLUTION SIGNATURE UNLOCKED]</color> Player unlocked evolved tier weapon: {powerUp.displayName}");

                // 🛡️ Evolution Overwrite Rule A: Mirror Shield (Unholy Vespers)
                if ((powerUp.displayName.Contains("Mirror Shield") || recipe.recipeName.ToLower().Contains("shield")) && _shieldSystem != null)
                {
                    _shieldSystem.isMirrorShieldUnlocked = true;
                    _shieldSystem.SetExplicitShieldCount(6);
                    Debug.Log("<color=cyan>[SHIELD EVOLVED]</color> Mirror Shield unlocked: Permanent orbiting shields.");
                }

                // 🪃 Evolution Overwrite Rule B: Magic Boomerang (Magnetic Rebounder)
                if ((powerUp.displayName.Contains("Magical Boomerang") || recipe.recipeName.ToLower().Contains("boomerang")) && _boomerangSystem != null)
                {
                    _boomerangSystem.isMagneticUnlocked = true;
                    Debug.Log("<color=cyan>[BOOMERANG EVOLVED]</color> Magical Boomerang unlocked: Magnetic Rebounder behavior.");
                }

                // ⚔️ Evolution Overwrite Rule C: Four Sword (Cardinal Slashes)
                if ((powerUp.displayName.Contains("Four Sword") || recipe.recipeName.ToLower().Contains("four")) && _swordSystem != null)
                {
                    _swordSystem.isFourSwordUnlocked = true;
                    _swordSystem.weaponTier = 5;
                    Debug.Log("<color=cyan>[FOUR SWORD EVOLVED]</color> Four Sword unlocked: Cardinal slashes (Up, Down, Left, Right) enabled!");
                }

                // 🧤 Evolution Overwrite Rule D: Mole Gloves
                if ((powerUp.displayName == "Mole Gloves") && _moleMittsSystem != null)
                {
                    _moleMittsSystem.UnlockMoleGloves();
                    Debug.Log("<color=purple>[MOLE GLOVES EVOLVED]</color> Mole Gloves unlocked: Devastating shockwave explosions!");
                }

                // 🌪️ Evolution Overwrite Rule E: Gust Jar
                if ((powerUp.displayName == "Gust Jar") && _rocsCapeSystem != null)
                {
                    _rocsCapeSystem.UnlockGustJar();
                    Debug.Log("<color=cyan>[GUST JAR EVOLVED]</color> Gust Jar unlocked: Devastating tornado swarm!");
                }
            }
        }

        // 2. Map the active choice modifications into recipient functional target systems
        switch (powerUp.effectType)
        {
            case SO_PowerUp.EffectType.AddMaxHp:
                _health?.AddMaxHp(powerUp.value);
                break;

            case SO_PowerUp.EffectType.AddDefense:
                _health?.AddDefense(powerUp.value);
                break;

            case SO_PowerUp.EffectType.AddMoveSpeed:
                if (_movement != null)
                {
                    _movement.moveSpeed += powerUp.value;
                }
                break;

            case SO_PowerUp.EffectType.AddMagnetRadius:
                _xp?.AddMagnetRadius(powerUp.value);
                break;

            case SO_PowerUp.EffectType.AddProjectileCount:
                _weapon?.AddProjectileCount((int)powerUp.value);
                break;

            case SO_PowerUp.EffectType.AddAttackRadius:
                _weapon?.AddAttackRadius(powerUp.value);
                break;

            case SO_PowerUp.EffectType.AddDamagePercent:
                _weapon?.AddDamagePercent(powerUp.value);
                break;

            case SO_PowerUp.EffectType.ReduceAttackInterval:
                _weapon?.ReduceAttackInterval(powerUp.value);
                break;

            case SO_PowerUp.EffectType.UnlockMoleMitts:
                if (_weapon != null)
                {
                    _weapon.UnlockMoleMitts = true;
                }
                if (_moleMittsSystem != null)
                {
                    int currentStack = _acquired[powerUp];
                    _moleMittsSystem.SetWeaponTier(currentStack);
                    Debug.Log($"<color=orange>[MOLE MITTS]</color> Tier {currentStack} unlocked!");
                }
                break;

            case SO_PowerUp.EffectType.UnlockSmallShield:
                if (_shieldSystem != null)
                {
                    int currentStack = _acquired[powerUp];
                    _shieldSystem.SetExplicitShieldCount(currentStack);
                    Debug.Log($"<color=green>[SHIELD UNLOCKED]</color> Small Shield Tier {currentStack}/5 active. Total Shields: {currentStack}");
                }
                break;

            case SO_PowerUp.EffectType.UpgradeCentrifugalFlux:
                if (_shieldSystem != null)
                {
                    int currentStack = _acquired[powerUp];
                    _shieldSystem.SetExplicitShieldCount(currentStack);
                    Debug.Log($"<color=lime>[SHIELD UPGRADE]</color> Small Shield Tier {currentStack}/5 active. Total Shields: {currentStack}");
                }
                break;

            case SO_PowerUp.EffectType.UnlockBoomerang:
                if (_boomerangSystem != null)
                {
                    int currentStack = _acquired[powerUp];
                    _boomerangSystem.weaponTier = currentStack;
                    Debug.Log($"<color=green>[BOOMERANG UPDATE]</color> Boomerang Weapon applied at Tier {currentStack}/5.");
                }
                break;

            case SO_PowerUp.EffectType.UnlockSmithsSword:
                if (_swordSystem != null)
                {
                    int currentStack = _acquired[powerUp];
                    _swordSystem.weaponTier = currentStack;
                    Debug.Log($"<color=green>[SWORD UPDATE]</color> Smith's Sword applied at Tier {currentStack}/5.");
                }
                break;

            case SO_PowerUp.EffectType.UnlockRocsCape:
                if (_rocsCapeSystem != null)
                {
                    int currentStack = _acquired[powerUp];
                    _rocsCapeSystem.SetWeaponTier(currentStack);
                    Debug.Log($"<color=green>[ROC'S CAPE]</color> Tornado Weapon applied at Tier {currentStack}/5.");
                }
                break;

            // Pass-through edge extensions:
            case SO_PowerUp.EffectType.UpgradeEdgeTempering:
            case SO_PowerUp.EffectType.UpgradeAerodynamicTuning:
            case SO_PowerUp.EffectType.UpgradeSeismicResonator:
            case SO_PowerUp.EffectType.UpgradeGaleExtension:
                break;
        }
    }

    public List<SO_PowerUp> GetLevelUpChoices()
    {
        if (registry == null)
        {
            Debug.LogError("[S11 PowerUps] Error: Missing required registry asset pointer assignment inside the Inspector view panel layout!");
            return new List<SO_PowerUp>();
        }

        List<SO_PowerUp> availablePowerUps = registry.GetAvailable(_xp != null ? _xp.Level : 1, _acquired);
        List<SO_PowerUp> evolutionChoices = new List<SO_PowerUp>();

        foreach (var recipe in evolutionRecipes)
        {
            if (_evolvedWeaponsOwned.Contains(recipe.evolvedResultAsset)) continue;

            bool weaponMaxed = false;
            if (recipe.baseWeaponAsset != null && _acquired.ContainsKey(recipe.baseWeaponAsset))
            {
                if (_acquired[recipe.baseWeaponAsset] >= recipe.baseWeaponAsset.maxStacks)
                {
                    weaponMaxed = true;
                }
            }

            bool passiveOwned = false;
            if (recipe.requiredPassiveAsset != null && _acquired.ContainsKey(recipe.requiredPassiveAsset))
            {
                passiveOwned = true;
            }

            if (weaponMaxed && passiveOwned)
            {
                Debug.Log($"[EVOLUTION AVAILABLE] Injecting {recipe.evolvedResultAsset.displayName} into level-up choices pool.");
                if (!evolutionChoices.Contains(recipe.evolvedResultAsset))
                {
                    evolutionChoices.Add(recipe.evolvedResultAsset);
                }
            }
        }

        List<SO_PowerUp> choices = new List<SO_PowerUp>();
        
        while (evolutionChoices.Count > 0 && choices.Count < 3)
        {
            choices.Add(evolutionChoices[0]);
            if (availablePowerUps.Contains(evolutionChoices[0]))
            {
                availablePowerUps.Remove(evolutionChoices[0]);
            }
            evolutionChoices.RemoveAt(0);
        }
        
        int remainingSlots = 3 - choices.Count;
        int standardChoiceCount = Mathf.Min(remainingSlots, availablePowerUps.Count);
        
        for (int i = 0; i < standardChoiceCount; i++)
        {
            int randomIndex = Random.Range(0, availablePowerUps.Count);
            choices.Add(availablePowerUps[randomIndex]);
            availablePowerUps.RemoveAt(randomIndex);
        }
        
        return choices;
    }
}