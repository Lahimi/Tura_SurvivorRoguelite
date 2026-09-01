using UnityEngine;
using System.Collections.Generic;

public class WP_RocsCapeWeapon : MonoBehaviour
{
    [Header("Prefab References")]
    [SerializeField] private GameObject tornadoPrefab;  // The tornado projectile

    [Header("Balancing Attributes")]
    [SerializeField] private float baseAttackInterval = 2f;
    [SerializeField] private float baseDamage = 25f;
    [SerializeField] private float tornadoSpeed = 8f;
    [SerializeField] private float tornadoLifetime = 2f;
    [SerializeField] private float detectionRange = 7f;

    [Header("Evolution States")]
    public int weaponTier = 0;
    public bool isGustJarUnlocked = false;

    [Header("Gust Jar Evolution Settings")]
    [SerializeField] private float evolvedDamageMultiplier = 2f;
    [SerializeField] private int evolvedTornadoCount = 3;
    [SerializeField] private float evolvedAttackInterval = 0.5f;
    [SerializeField] private float evolvedLifetime = 3f;

    private float _cooldownTimer;
    private float _currentDamage;
    private float _currentAttackInterval;

    private void Update()
    {
        if (Time.timeScale == 0f || weaponTier <= 0) return;

        _cooldownTimer += Time.deltaTime;
        float interval = isGustJarUnlocked ? evolvedAttackInterval : baseAttackInterval;
        
        if (_cooldownTimer >= interval)
        {
            _cooldownTimer = 0f;
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        if (isGustJarUnlocked)
        {
            ExecuteGustJarAttack();
        }
        else
        {
            ExecuteStandardAttack();
        }
    }

    private void ExecuteStandardAttack()
    {
        _currentDamage = baseDamage + (weaponTier - 1) * 6f;
        
        // Find nearest enemy
        Transform nearestEnemy = GetNearestEnemy();
        if (nearestEnemy == null) return;
        
        // Calculate direction to enemy
        Vector2 direction = (nearestEnemy.position - transform.position).normalized;
        
        // Spawn tornado that tracks the enemy
        SpawnTornado(transform.position, direction, _currentDamage, tornadoLifetime, true);
        
        // Higher tiers spawn additional tornadoes
        if (weaponTier >= 3)
        {
            // Find second nearest enemy
            Transform secondEnemy = GetSecondNearestEnemy(nearestEnemy.position);
            if (secondEnemy != null)
            {
                Vector2 dir2 = (secondEnemy.position - transform.position).normalized;
                SpawnTornado(transform.position, dir2, _currentDamage * 0.7f, tornadoLifetime, true);
            }
        }
        
        if (weaponTier >= 5)
        {
            // Random tornado for extra chaos
            float randomAngle = Random.Range(0f, 360f);
            Vector2 randomDir = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
            SpawnTornado(transform.position, randomDir, _currentDamage * 0.5f, tornadoLifetime, false);
        }
    }

    private void ExecuteGustJarAttack()
    {
        _currentDamage = (baseDamage + (weaponTier - 1) * 6f) * evolvedDamageMultiplier;
        
        // Spawn multiple tornadoes in all directions (Death Ray style)
        for (int i = 0; i < evolvedTornadoCount; i++)
        {
            float angle = i * (360f / evolvedTornadoCount);
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            SpawnTornado(transform.position, direction, _currentDamage, evolvedLifetime, false);
        }
        
        // Additional tornadoes at higher tiers for Gust Jar
        if (weaponTier >= 3)
        {
            for (int i = 0; i < 3; i++)
            {
                float randomAngle = Random.Range(0f, 360f);
                Vector2 randomDir = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
                SpawnTornado(transform.position, randomDir, _currentDamage * 0.6f, evolvedLifetime, false);
            }
        }
    }

    private Transform GetNearestEnemy()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, detectionRange);
        Transform nearest = null;
        float closestDistance = Mathf.Infinity;
        
        foreach (var enemy in enemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    nearest = enemy.transform;
                }
            }
        }
        return nearest;
    }

    private Transform GetSecondNearestEnemy(Vector3 firstEnemyPos)
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, detectionRange);
        Transform secondNearest = null;
        float secondClosestDistance = Mathf.Infinity;
        
        foreach (var enemy in enemies)
        {
            if (enemy.CompareTag("Enemy") && enemy.transform.position != firstEnemyPos)
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance < secondClosestDistance)
                {
                    secondClosestDistance = distance;
                    secondNearest = enemy.transform;
                }
            }
        }
        return secondNearest;
    }

    private void SpawnTornado(Vector3 position, Vector2 direction, float damage, float lifetime, bool trackEnemy)
    {
        if (tornadoPrefab == null) return;
        
        GameObject tornado = Instantiate(tornadoPrefab, position, Quaternion.identity);
        
        if (tornado.TryGetComponent<WP_Tornado>(out var tornadoComponent))
        {
            tornadoComponent.Initialize(damage, lifetime, direction, tornadoSpeed, trackEnemy);
        }
    }

    public void SetWeaponTier(int tier)
    {
        weaponTier = tier;
        baseAttackInterval = Mathf.Max(0.8f, 2f - (tier - 1) * 0.15f);
    }

    public void UnlockGustJar()
    {
        isGustJarUnlocked = true;
        weaponTier = 5;
    }
}