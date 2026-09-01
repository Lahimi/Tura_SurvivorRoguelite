using System.Collections.Generic;
using UnityEngine;

public class WP_OrbitingShields : MonoBehaviour
{
    [Header("Shield Prefab References")]
    [Tooltip("The visual prefab for the Small Shield (Must have a Collider2D set to Is Trigger).")]
    [SerializeField] private GameObject smallShieldPrefab;
    [Tooltip("The visual prefab for the Mirror Shield evolution upgrade.")]
    [SerializeField] private GameObject mirrorShieldPrefab;

    [Header("Orbit Balancing")]
    [SerializeField] private float rotationSpeed = 150f;
    [SerializeField] private float orbitRadius = 2.2f;
    [SerializeField] private float baseDamage = 15f;

    [Header("Small Shield Cycle Timers")]
    [Tooltip("How long the small shields stay active before disappearing.")]
    [SerializeField] private float activeDuration = 4.0f;
    [Tooltip("Time between small shield manifestations.")]
    [SerializeField] private float attackInterval = 8.0f;

    [Header("Runtime Progression States (Controlled by Power-Ups)")]
    public int shieldCount = 0;
    public bool isMirrorShieldUnlocked = false;

    private readonly List<GameObject> _activeShieldInstances = new List<GameObject>();
    private float _intervalTimer;
    private float _durationTimer;
    private bool _isShieldActive;
    private float _currentAngle;

    void Update()
    {
        // Don't calculate or process anything if the game is paused
        if (Time.timeScale == 0f) return;

        // Mode A: Mirror Shield (Indefinite rotation loop)
        if (isMirrorShieldUnlocked)
        {
            // If we don't have shields spawned, or the count doesn't match, rebuild instantly
            if (!_isShieldActive || _activeShieldInstances.Count != shieldCount)
            {
                SpawnShieldRing(mirrorShieldPrefab);
            }
            UpdateShieldPositions();
            return; 
        }

        // Mode B: Small Shield (Periodic activation cycle)
        if (shieldCount <= 0) return;

        if (!_isShieldActive)
        {
            _intervalTimer += Time.deltaTime;
            if (_intervalTimer >= attackInterval)
            {
                _intervalTimer = 0f;
                SpawnShieldRing(smallShieldPrefab);
            }
        }
        else
        {
            UpdateShieldPositions();

            _durationTimer += Time.deltaTime;
            if (_durationTimer >= activeDuration)
            {
                ClearShields();
            }
        }
    }

    private void SpawnShieldRing(GameObject prefabToSpawn)
    {
        ClearShields();
        if (prefabToSpawn == null || shieldCount <= 0) return;

        _isShieldActive = true;
        _durationTimer = 0f;

        // Mathematically spread the shields around the player
        for (int i = 0; i < shieldCount; i++)
        {
            GameObject shieldInstance = Instantiate(prefabToSpawn, transform.position, Quaternion.identity, transform);
            
            // Initialize damage output inside its tracking collider component
            if (shieldInstance.TryGetComponent<WP_ShieldHitbox>(out var hitbox))
            {
                hitbox.Initialize(baseDamage);
            }
            else
            {
                // Fallback auto-hook if you forgot to add it to the prefab
                shieldInstance.AddComponent<WP_ShieldHitbox>().Initialize(baseDamage);
            }

            _activeShieldInstances.Add(shieldInstance);
        }

        UpdateShieldPositions();
    }

    private void UpdateShieldPositions()
    {
        // Update the master angle tracking state using frame timing delta values
        _currentAngle += rotationSpeed * Time.deltaTime;
        if (_currentAngle >= 360f) _currentAngle -= 360f;

        for (int i = 0; i < _activeShieldInstances.Count; i++)
        {
            if (_activeShieldInstances[i] == null) continue;

            // Compute precise positioning per instance using polar-to-cartesian conversions
            float individualAngle = _currentAngle + (i * (360f / _activeShieldInstances.Count));
            float radians = individualAngle * Mathf.Deg2Rad;

            Vector3 offsetVector = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f) * orbitRadius;
            _activeShieldInstances[i].transform.position = transform.position + offsetVector;

            // Keeps the shields facing outward/rotating nicely
            _activeShieldInstances[i].transform.right = offsetVector.normalized;
        }
    }

    public void ClearShields()
    {
        foreach (var shield in _activeShieldInstances)
        {
            if (shield != null) Destroy(shield);
        }
        _activeShieldInstances.Clear();
        _isShieldActive = false;
        _durationTimer = 0f;
    }

    /// <summary>
    /// Forces the shield ring to match an exact count ceiling instantly.
    /// Safely clears and respawns active shields so there is no timer delay on purchase.
    /// </summary>
    public void SetExplicitShieldCount(int targetAmount)
    {
        shieldCount = targetAmount;

        if (isMirrorShieldUnlocked) 
        {
            SpawnShieldRing(mirrorShieldPrefab);
        }
        else if (shieldCount > 0)
        {
            _intervalTimer = 0f; // Reset current waiting downtime interval state frame
            SpawnShieldRing(smallShieldPrefab);
        }
    }

    // Progression interface methods to increment or modify stats from S11_PlayerPowerUps
    public void UpgradeShieldCount(int amount)
    {
        shieldCount += amount;
        if (_isShieldActive) 
        {
            // Force a ring layout update instantly when picking an item from the upgrade UI
            SpawnShieldRing(isMirrorShieldUnlocked ? mirrorShieldPrefab : smallShieldPrefab);
        }
    }

    private void OnDisable()
    {
        ClearShields();
    }
}