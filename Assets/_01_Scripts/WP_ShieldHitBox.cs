using System.Collections.Generic;
using UnityEngine;

public class WP_ShieldHitbox : MonoBehaviour
{
    [Header("Shield Balancing")]
    [Tooltip("How long (in seconds) before this specific shield can damage the exact same enemy again.")]
    [SerializeField] private float hitCooldown = 0.5f;

    private float _damage = 10f;
    
    // Tracks individual enemy colliders and the timestamp when they can be hit again[cite: 31]
    private readonly Dictionary<Collider2D, float> _enemyCooldownTimers = new Dictionary<Collider2D, float>();
    
    // A list used to safely clean up old dictionary data without breaking loop iterations[cite: 31]
    private readonly List<Collider2D> _cleanupList = new List<Collider2D>();

    public void Initialize(float damageValue)
    {
        _damage = damageValue;
    }

    private void Update()
    {
        HandleCooldownTimers();
    }

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
        EvaluateHit(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        EvaluateHit(collision);
    }

    private void EvaluateHit(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy")) return;

        float currentTime = Time.time;

        // Check internal cooldown vulnerabilities[cite: 31]
        if (_enemyCooldownTimers.ContainsKey(collision))
        {
            if (currentTime < _enemyCooldownTimers[collision])
            {
                return; // Target is currently invulnerable to this specific shield[cite: 31]
            }
        }

        // Apply hit cooldown lock stamp[cite: 31]
        _enemyCooldownTimers[collision] = currentTime + hitCooldown;

        if (collision.TryGetComponent<S04_HealthComponent>(out var enemyHealth))
        {
            enemyHealth.TakeDamage(_damage);
            Debug.Log($"[SHIELD HIT] '{gameObject.name}' dealt {_damage} damage to '{collision.name}'!");
        }
    }

    private void OnDisable()
    {
        _enemyCooldownTimers.Clear(); //[cite: 31]
    }
}