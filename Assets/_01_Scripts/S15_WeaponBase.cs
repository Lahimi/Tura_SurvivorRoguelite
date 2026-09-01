using System;
using UnityEngine;

public enum WeaponType { Proximity, Projectile }

public class S15_WeaponBase : MonoBehaviour
{
    [Header("Data")]
    public SO_GameSession gameSession;
    public GameObject projectilePrefab;
    public GameObject proximityVfxPrefab;

    [Header("Progression Unlocks")]
    [Tooltip("Toggle this via your Level-Up / Power-up system when the player chooses the Mole Mitts!")]
    public bool UnlockMoleMitts = false;

    [Header("Animations")]
    [Tooltip("Drag the Animator component sitting on your WeaponVisuals child object here!")]
    [SerializeField] private Animator weaponAnimator;

    [Header("Weapon Child References")]
    [Tooltip("Drag your child 'Slash_Primary' object here")]
    [SerializeField] private GameObject slashPrimaryObject;
    [Tooltip("Drag your child 'Slash_Secondary' object here")]
    [SerializeField] private GameObject slashSecondaryObject;
    [Tooltip("Drag your child 'MoleMitts_Slam' object here")]
    [SerializeField] private GameObject moleMittsSlamObject;

    [Header("Testing & Debugging")]
    [Tooltip("Check this to force Mole Mitts even if UnlockMoleMitts is false (handy for sandbox testing).")]
    [SerializeField] private bool forceMoleMittsTesting = false;

    [Header("Inspector Fallback")]
    [SerializeField] WeaponType fallbackWeaponType = WeaponType.Proximity;
    [SerializeField] float fallbackDamage = 20f;
    [SerializeField] float fallbackAttackInterval = 1f;

    // --- EXPOSED PROPERTY FOR HITBOXES ---
    public float CurrentDamage => _damage; 

    float _damage;
    float _attackInterval;
    float _timer;
    WeaponType _weaponType;

    int _projectileCount = 1;
    float _projectileSpeed = 8f;
    [SerializeField] float _proximityRadius = 2f;

    void Awake()
    {
        // Absolute safety rule: ensure everything is hidden and scaled away on frame zero
        DisableAllWeaponObjects();

        if (gameSession != null && gameSession.selectedCharacter != null)
        {
            var stats = gameSession.selectedCharacter.stats;
            _damage = stats.attackPower;
            _attackInterval = 1f;
            if (!Enum.TryParse(stats.weaponType, out _weaponType))
                _weaponType = WeaponType.Proximity;
        }
        else
        {
            _damage = fallbackDamage;
            _attackInterval = fallbackAttackInterval;
            _weaponType = fallbackWeaponType;
        }
    }

    private void OnValidate()
    {
        // Keeps the workspace clean inside the Unity editor before clicking Play
        if (!Application.isPlaying)
        {
            DisableAllWeaponObjects();
        }
    }

    void Update()
    {
        // If the game pauses (e.g., Level Up screen open), instantly suppress 
        // frozen visual objects and halt timing calculations.
        if (Time.timeScale == 0f)
        {
            DisableAllWeaponObjects();
            return;
        }

        _timer += Time.deltaTime;
        if (_timer >= _attackInterval)
        {
            _timer = 0f;
            PerformAttack();
        }
    }

    private void OnDisable()
    {
        // Safety cleanup if the player or weapon script gets toggled off mid-game
        DisableAllWeaponObjects();
    }

    void PerformAttack()
    {
        switch (_weaponType)
        {
            case WeaponType.Projectile: FireProjectiles(); break;
            case WeaponType.Proximity: ProximityBurst(); break;
        }
    }

    void FireProjectiles()
    {
        var enemies = FindObjectsByType<S14_EnemyBase>(FindObjectsSortMode.None);
        if (enemies.Length == 0) return;

        S14_EnemyBase nearest = null;
        float minDist = float.MaxValue;
        foreach (var e in enemies)
        {
            float d = Vector2.Distance(transform.position, e.transform.position);
            if (d < minDist) { minDist = d; nearest = e; }
        }
        if (nearest == null || projectilePrefab == null) return;

        Vector2 baseDir = ((Vector2)nearest.transform.position - (Vector2)transform.position).normalized;
        float spread = 15f;
        float startAngle = -((_projectileCount - 1) * spread) / 2f;

        for (int i = 0; i < _projectileCount; i++)
        {
            float angle = startAngle + i * spread;
            Vector2 dir = Quaternion.Euler(0, 0, angle) * baseDir;
            var proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            
            proj.GetComponent<S16_Projectile>()?.Init(dir, _projectileSpeed, _damage);
        }
    }

    void ProximityBurst()
    {
        if (weaponAnimator != null)
        {
            // Determine if the player has chosen or forced the Mole Mitts progression path
            bool isMoleMittsActive = forceMoleMittsTesting || UnlockMoleMitts;

            // Instantly collapse all tracking states to clean old frame artifacts
            DisableAllWeaponObjects();

            if (isMoleMittsActive) 
            {
                // 1. Activate and restore sizing for Mole Mitts child element
                if (moleMittsSlamObject != null)
                {
                    moleMittsSlamObject.SetActive(true);
                    moleMittsSlamObject.transform.localScale = Vector3.one * (_proximityRadius / 2f);
                }

                // 2. Direct-play the clip explicitly bypassing state machines
                weaponAnimator.Play("Anim_MoleMitts_Slam", 0, 0f);
            }
            else
            {
                // 1. Activate and restore sizing for Sword child structures
                if (slashPrimaryObject != null)
                {
                    slashPrimaryObject.SetActive(true);
                    slashPrimaryObject.transform.localScale = Vector3.one * (_proximityRadius / 2f);
                }
                if (slashSecondaryObject != null)
                {
                    slashSecondaryObject.SetActive(true);
                    slashSecondaryObject.transform.localScale = Vector3.one * (_proximityRadius / 2f);
                }

                // 2. Direct-play the standard sword swing clip
                weaponAnimator.Play("Anim_SmithSword_Slash", 0, 0f);
            }
        }

        if (proximityVfxPrefab != null)
            Destroy(Instantiate(proximityVfxPrefab, transform.position, Quaternion.identity), 0.5f);
    }

    public void DisableAllWeaponObjects()
    {
        // 1. Structural deactivation via active states
        if (slashPrimaryObject != null) slashPrimaryObject.SetActive(false);
        if (slashSecondaryObject != null) slashSecondaryObject.SetActive(false);
        if (moleMittsSlamObject != null) moleMittsSlamObject.SetActive(false);

        // 2. Scale collapse to absolute zero to prevent visual bleeding or animator caching glitches
        if (slashPrimaryObject != null) slashPrimaryObject.transform.localScale = Vector3.zero;
        if (slashSecondaryObject != null) slashSecondaryObject.transform.localScale = Vector3.zero;
        if (moleMittsSlamObject != null) moleMittsSlamObject.transform.localScale = Vector3.zero;
    }

    // TARGET FUNCTION FOR ANIMATION EVENTS: Place this event keyframe at the end of your clips!
    public void TurnOffActiveWeaponVisuals()
    {
        DisableAllWeaponObjects();
    }

    // --- Power-up Hooks ---
    public void AddDamagePercent(float percent) => _damage *= (1f + percent / 100f);
    public void ReduceAttackInterval(float amount) => _attackInterval = Mathf.Max(0.1f, _attackInterval - amount);
    public void AddProjectileCount(int count) => _projectileCount += count;
    public void AddAttackRadius(float amount) => _proximityRadius += amount;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _proximityRadius);
    }
}