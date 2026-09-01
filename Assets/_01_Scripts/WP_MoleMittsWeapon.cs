using UnityEngine;
using System.Collections;

public class WP_MoleMittsWeapon : MonoBehaviour
{
    [Header("Prefab References")]
    [SerializeField] private GameObject shockwavePrefab;

    [Header("Balancing Attributes")]
    [SerializeField] private float baseAttackInterval = 2.5f;
    [SerializeField] private float baseDamage = 35f;
    [SerializeField] private float shockwaveDuration = 0.3f;
    [SerializeField] private float shockwaveRadius = 1.5f;
    [SerializeField] private float shockwaveTravelDistance = 4f;

    [Header("Evolution States")]
    public int weaponTier = 0;
    public bool isMoleGlovesUnlocked = false;

    [Header("Mole Gloves Evolution Settings")]
    [SerializeField] private float evolvedDamageMultiplier = 1.5f;
    [SerializeField] private float evolvedRadiusMultiplier = 1.2f;

    private float _cooldownTimer;
    private float _currentDamage;
    private float _currentRadius;
    private Vector2 _lastMoveDirection = Vector2.down;

    private void Update()
    {
        if (Time.timeScale == 0f || weaponTier <= 0) return;

        // Track last movement direction (WASD)
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");
        
        if (inputX != 0 || inputY != 0)
        {
            _lastMoveDirection = new Vector2(inputX, inputY).normalized;
        }

        _cooldownTimer += Time.deltaTime;
        if (_cooldownTimer >= baseAttackInterval)
        {
            _cooldownTimer = 0f;
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        if (isMoleGlovesUnlocked)
        {
            ExecuteMoleGlovesAttack();
        }
        else
        {
            ExecuteStandardAttack();
        }
    }

    private void ExecuteStandardAttack()
    {
        _currentDamage = baseDamage + (weaponTier - 1) * 8f;
        _currentRadius = shockwaveRadius + (weaponTier - 1) * 0.15f;
        
        Vector3 pos = transform.position;
        
        switch (weaponTier)
        {
            case 1:
                // Single shockwave in facing direction
                SpawnShockwave(pos, _lastMoveDirection, _currentRadius, _currentDamage);
                break;
                
            case 2:
                // Two shockwaves: forward + slight spread
                SpawnShockwave(pos, _lastMoveDirection, _currentRadius, _currentDamage);
                SpawnShockwave(pos, RotateDirection(_lastMoveDirection, 20f), _currentRadius * 0.8f, _currentDamage * 0.7f);
                SpawnShockwave(pos, RotateDirection(_lastMoveDirection, -20f), _currentRadius * 0.8f, _currentDamage * 0.7f);
                break;
                
            case 3:
                // Cone of 3 shockwaves
                SpawnShockwave(pos, _lastMoveDirection, _currentRadius, _currentDamage);
                SpawnShockwave(pos, RotateDirection(_lastMoveDirection, 30f), _currentRadius * 0.8f, _currentDamage * 0.7f);
                SpawnShockwave(pos, RotateDirection(_lastMoveDirection, -30f), _currentRadius * 0.8f, _currentDamage * 0.7f);
                break;
                
            case 4:
                // Forward + left + right
                SpawnShockwave(pos, _lastMoveDirection, _currentRadius, _currentDamage);
                SpawnShockwave(pos, RotateDirection(_lastMoveDirection, 90f), _currentRadius * 0.7f, _currentDamage * 0.6f);
                SpawnShockwave(pos, RotateDirection(_lastMoveDirection, -90f), _currentRadius * 0.7f, _currentDamage * 0.6f);
                break;
                
            case 5:
                // Full spread: forward, diagonal, side
                SpawnShockwave(pos, _lastMoveDirection, _currentRadius, _currentDamage);
                SpawnShockwave(pos, RotateDirection(_lastMoveDirection, 45f), _currentRadius * 0.8f, _currentDamage * 0.7f);
                SpawnShockwave(pos, RotateDirection(_lastMoveDirection, -45f), _currentRadius * 0.8f, _currentDamage * 0.7f);
                SpawnShockwave(pos, RotateDirection(_lastMoveDirection, 90f), _currentRadius * 0.6f, _currentDamage * 0.5f);
                SpawnShockwave(pos, RotateDirection(_lastMoveDirection, -90f), _currentRadius * 0.6f, _currentDamage * 0.5f);
                break;
        }
    }

    private void ExecuteMoleGlovesAttack()
    {
        _currentDamage = (baseDamage + (weaponTier - 1) * 8f) * evolvedDamageMultiplier;
        _currentRadius = (shockwaveRadius + (weaponTier - 1) * 0.15f) * evolvedRadiusMultiplier;
        
        Vector3 pos = transform.position;
        
        // Shockwave in all 4 cardinal directions (like Four Sword!)
        SpawnShockwave(pos, Vector2.up, _currentRadius, _currentDamage);
        SpawnShockwave(pos, Vector2.down, _currentRadius, _currentDamage);
        SpawnShockwave(pos, Vector2.left, _currentRadius, _currentDamage);
        SpawnShockwave(pos, Vector2.right, _currentRadius, _currentDamage);
        
        // Bonus: also spawn in facing direction with extra damage
        SpawnShockwave(pos, _lastMoveDirection, _currentRadius * 1.1f, _currentDamage * 1.2f);
    }

    private Vector2 RotateDirection(Vector2 dir, float angleDegrees)
    {
        float rad = angleDegrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(dir.x * cos - dir.y * sin, dir.x * sin + dir.y * cos);
    }

    private void SpawnShockwave(Vector3 position, Vector2 direction, float radius, float damage)
    {
        if (shockwavePrefab == null || direction == Vector2.zero) return;

        GameObject shockwave = Instantiate(shockwavePrefab, position, Quaternion.identity);
        
        if (shockwave.TryGetComponent<WP_Shockwave>(out var shockwaveComponent))
        {
            shockwaveComponent.Initialize(damage, shockwaveDuration, radius, direction, shockwaveTravelDistance);
        }
    }

    public void SetWeaponTier(int tier)
    {
        weaponTier = tier;
        baseAttackInterval = Mathf.Max(1.2f, 2.5f - (tier - 1) * 0.2f);
    }

    public void UnlockMoleGloves()
    {
        isMoleGlovesUnlocked = true;
        weaponTier = 5;
    }
}