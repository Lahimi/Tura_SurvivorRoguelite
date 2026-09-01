using System.Collections.Generic;
using UnityEngine;

public class WP_SwordHitbox : MonoBehaviour
{
    [Header("Custom Hitbox Balancing")]
    [Tooltip("Multiplies the weapon's base damage if you want the slash to be stronger/weaker than a default proximity burst.")]
    [SerializeField] private float damageMultiplier = 1.0f;

    [Tooltip("How long (in seconds) before this specific slash object can damage the exact same enemy again.")]
    [SerializeField] private float hitCooldown = 0.5f;

    private S15_WeaponBase _weaponBase;
    
    // Tracks individual enemy colliders and the timestamp when they can be hit again
    private readonly Dictionary<Collider2D, float> _enemyCooldownTimers = new Dictionary<Collider2D, float>();
    
    // A list used to safely clean up old dictionary data without breaking loop iterations
    private readonly List<Collider2D> _cleanupList = new List<Collider2D>();

    private void Awake()
    {
        // Automatically finds S15_WeaponBase sitting on the root player entity
        _weaponBase = GetComponentInParent<S15_WeaponBase>();
    }

    private void Update()
    {
        HandleCooldownTimers();
    }

    /// <summary>
    /// Processes active tracking timers and removes enemies whose cooldowns have expired.
    /// </summary>
    private void HandleCooldownTimers()
    {
        if (_enemyCooldownTimers.Count == 0) return;

        _cleanupList.Clear();
        float currentTime = Time.time;

        foreach (var kvp in _enemyCooldownTimers)
        {
            if (currentTime >= kvp.Value)
            {
                _cleanupList.Add(kvp.Key);
            }
        }

        for (int i = 0; i < _cleanupList.Count; i++)
        {
            if (_cleanupList[i] == null) continue;
            _enemyCooldownTimers.Remove(_cleanupList[i]);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Console pipeline logging to debug hidden physics layer/matrix compilation errors
        Debug.Log($"[PHYSICS CONTACT] '{gameObject.name}' triggered contact with object: '{collision.name}' on Layer: {LayerMask.LayerToName(collision.gameObject.layer)}");
        
        EvaluateHit(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // Catches any fast-moving enemies entering the swing area mid-animation
        EvaluateHit(collision);
    }

    private void EvaluateHit(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy")) return;

        float currentTime = Time.time;

        // Check internal cooldown vulnerabilities
        if (_enemyCooldownTimers.ContainsKey(collision))
        {
            if (currentTime < _enemyCooldownTimers[collision])
            {
                return; // Target is currently invulnerable to this slice swing pass
            }
        }

        // Apply hit cooldown lock stamp
        _enemyCooldownTimers[collision] = currentTime + hitCooldown;

        // Pull dynamic calculations natively out of your progression engine
        float actualCalculatedDamage = 10f;
        if (_weaponBase != null)
        {
            actualCalculatedDamage = _weaponBase.CurrentDamage * damageMultiplier;
        }

        if (collision.TryGetComponent<S04_HealthComponent>(out var enemyHealth))
        {
            enemyHealth.TakeDamage(actualCalculatedDamage); 
            Debug.Log($"[SUCCESSFUL HIT] '{gameObject.name}' dealt {actualCalculatedDamage} damage to '{collision.name}'!");
        }
    }

    private void OnDisable()
    {
        // Clear tracking registry memory arrays instantly when animation cycles complete
        _enemyCooldownTimers.Clear();
    }
}