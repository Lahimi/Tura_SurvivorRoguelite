using UnityEngine;

// =====================================================================================
// ⚠️ GYM-ONLY UTILITY — Not used in the main game loop architecture.
// In the production build, enemies drop XP gems on death (see Enemy Base, GP1-010).
// This utility simulates drop pipelines on a timer loop for sandbox playtesting.
// =====================================================================================

public class S10_GemSpawner : MonoBehaviour
{
    [Header("Prefab Setup")]
    [Tooltip("The PF_XpGem prefab asset containing the S09_XpGem behavior script.")]
    [SerializeField] private GameObject gemPrefab;

    [Header("Spawn Layout Bounds")]
    [Tooltip("How frequently a new experience gem is instantiated into the scene layout (in seconds).")]
    [SerializeField] private float spawnInterval = 2f;

    [Tooltip("The minimum radial boundary distance from the player where gems can appear.")]
    [SerializeField] private float minSpawnRadius = 3f;

    [Tooltip("The maximum radial boundary distance from the player where gems can appear.")]
    [SerializeField] private float maxSpawnRadius = 6f;

    [Header("Scene Performance Ceiling")]
    [Tooltip("The maximum number of live gym gems allowed in the scene simultaneously.")]
    [SerializeField] private int maxGems = 20;

    // Tracking anchors
    private Transform _playerTransform;
    private S08_PlayerXP _playerXp;
    private float _spawnTimer;

    private void Start()
    {
        // 🔧 Runtime Validation Loop: Locates tracking dependencies automatically inside the sandbox
        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            _playerTransform = playerObj.transform;
            _playerXp = playerObj.GetComponent<S08_PlayerXP>();
        }
        else
        {
            // Backup fallback search sweep if tags are unassigned inside the test grid layout
            _playerXp = FindFirstObjectByType<S08_PlayerXP>();
            if (_playerXp != null)
            {
                _playerTransform = _playerXp.transform;
            }
        }

        if (_playerTransform == null || _playerXp == null)
        {
            Debug.LogError("[Gem Spawner GYM] Critical Error: No Player entity containing S08_PlayerXP found in the active hierarchy scene layout!");
        }
    }

    private void Update()
    {
        // Guard: Stop accumulating cycles if core references are missing or unassigned
        if (_playerTransform == null || gemPrefab == null) return;

        _spawnTimer += Time.deltaTime;

        if (_spawnTimer >= spawnInterval)
        {
            _spawnTimer = 0f;

            // Enforce performance ceilings to protect engine performance indexes
            if (GetCurrentGemCount() < maxGems)
            {
                SpawnGemInAnnulus();
            }
        }
    }

    private void SpawnGemInAnnulus()
    {
        // 🧱 Annulus Math Matrix Formulation: Guarantees placement within a clear donut perimeter ring
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float randomDistance = Random.Range(minSpawnRadius, maxSpawnRadius);
        
        // Pivot calculations are offset dynamically from the player's current runtime position anchor
        Vector3 spawnPosition = _playerTransform.position + new Vector3(
            randomDirection.x * randomDistance, 
            randomDirection.y * randomDistance, 
            0f
        );

        // Instantiate the object into the simulation space hierarchy
        GameObject spawnedGemObj = Instantiate(gemPrefab, spawnPosition, Quaternion.identity);
        
        // Inject structural pointer addresses directly into the component memory slot
        S09_XpGem gemScript = spawnedGemObj.GetComponent<S09_XpGem>();
        if (gemScript != null)
        {
            gemScript.Init(_playerTransform, _playerXp);
        }
        else
        {
            Debug.LogError("[Gem Spawner GYM] Error: Spawned gem prefab is missing the required S09_XpGem component script structure!");
        }
    }

    private int GetCurrentGemCount()
    {
        // Counts remaining live gem instances inside the simulation space profile tables
        return FindObjectsByType<S09_XpGem>(FindObjectsSortMode.None).Length;
    }

    private void OnDrawGizmosSelected()
    {
        // 🎨 Editor Visualization Layout: Renders boundary markers inside the scene view for easy tuning
        if (_playerTransform == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_playerTransform.position, minSpawnRadius);
        
        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere(_playerTransform.position, maxSpawnRadius);
    }
}