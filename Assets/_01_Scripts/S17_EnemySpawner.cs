using UnityEngine;
using System.Collections.Generic;

// Spawns enemies continuously from the screen edges on a timer.
// Authentic to the roguelike genre: enemies always approach from off-screen.
// Place as MGR_EnemySpawner in the scene hierarchy.
public class S17_EnemySpawner : MonoBehaviour
{
    [Header("Spawning")]
    [Tooltip("List of enemy prefabs to spawn. Will randomly pick from this list.")]
    public List<GameObject> enemyPrefabs = new List<GameObject>();
    
    [Tooltip("If true, enemies are spawned in sequence. If false, random selection.")]
    public bool spawnInSequence = false;
    
    [Tooltip("Weighted spawn chances (leave empty for equal chance). Size must match enemyPrefabs list.")]
    public List<float> spawnWeights = new List<float>();
    
    public float spawnInterval = 2f;
    public int maxEnemies = 20;

    // How far outside the camera edge enemies appear (world units).
    // Keep >= 1 so enemies are not visible the frame they spawn.
    [SerializeField] float spawnMargin = 1.5f;

    private float _timer;
    private int _currentSpawnIndex = 0;
    private float _totalWeight;

    private void Start()
    {
        // Calculate total weight for weighted random selection
        if (spawnWeights != null && spawnWeights.Count == enemyPrefabs.Count)
        {
            foreach (float weight in spawnWeights)
            {
                _totalWeight += weight;
            }
        }
        else
        {
            // Equal weight for all enemies
            _totalWeight = enemyPrefabs.Count;
        }
    }

    void Update()
    {
        // Don't spawn if no enemies in the list
        if (enemyPrefabs == null || enemyPrefabs.Count == 0) return;

        // Pause spawning when the active enemy count hits the cap.
        if (FindObjectsByType<S14_EnemyBase>(FindObjectsSortMode.None).Length >= maxEnemies) return;

        _timer += Time.deltaTime;
        if (_timer >= spawnInterval)
        {
            _timer = 0f;
            
            GameObject selectedPrefab = GetNextEnemyPrefab();
            if (selectedPrefab != null)
            {
                Instantiate(selectedPrefab, GetSpawnPosition(), Quaternion.identity);
            }
        }
    }

    private GameObject GetNextEnemyPrefab()
    {
        if (enemyPrefabs == null || enemyPrefabs.Count == 0) return null;
        
        if (spawnInSequence)
        {
            // Sequence mode: spawn in order
            GameObject selected = enemyPrefabs[_currentSpawnIndex];
            _currentSpawnIndex = (_currentSpawnIndex + 1) % enemyPrefabs.Count;
            return selected;
        }
        else
        {
            // Random mode: pick random enemy based on weights
            return GetRandomWeightedEnemy();
        }
    }

    private GameObject GetRandomWeightedEnemy()
    {
        if (spawnWeights != null && spawnWeights.Count == enemyPrefabs.Count && _totalWeight > 0)
        {
            // Weighted random selection
            float randomValue = Random.Range(0f, _totalWeight);
            float cumulativeWeight = 0f;
            
            for (int i = 0; i < enemyPrefabs.Count; i++)
            {
                cumulativeWeight += spawnWeights[i];
                if (randomValue <= cumulativeWeight)
                {
                    return enemyPrefabs[i];
                }
            }
        }
        
        // Equal chance fallback
        return enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
    }

    // Returns a random point just outside one of the four camera edges.
    Vector2 GetSpawnPosition()
    {
        Camera cam = Camera.main;
        if (cam == null) return Vector2.zero;
        
        float camZ = Mathf.Abs(cam.transform.position.z);

        // Convert viewport corners (0,0) and (1,1) to world space.
        Vector3 min = cam.ViewportToWorldPoint(new Vector3(0, 0, camZ));
        Vector3 max = cam.ViewportToWorldPoint(new Vector3(1, 1, camZ));

        float m = spawnMargin;
        return Random.Range(0, 4) switch
        {
            0 => new Vector2(Random.Range(min.x, max.x), max.y + m), // top
            1 => new Vector2(Random.Range(min.x, max.x), min.y - m), // bottom
            2 => new Vector2(min.x - m, Random.Range(min.y, max.y)), // left
            _ => new Vector2(max.x + m, Random.Range(min.y, max.y))  // right
        };
    }

    /// <summary>
    /// Optional: Add an enemy prefab to the spawn list at runtime
    /// </summary>
    public void AddEnemyPrefab(GameObject newEnemyPrefab, float weight = 1f)
    {
        if (newEnemyPrefab == null) return;
        
        enemyPrefabs.Add(newEnemyPrefab);
        
        // Add corresponding weight
        if (spawnWeights != null && spawnWeights.Count == enemyPrefabs.Count - 1)
        {
            spawnWeights.Add(weight);
            _totalWeight += weight;
        }
    }

    /// <summary>
    /// Optional: Remove an enemy prefab from the spawn list at runtime
    /// </summary>
    public void RemoveEnemyPrefab(int index)
    {
        if (index >= 0 && index < enemyPrefabs.Count)
        {
            if (spawnWeights != null && index < spawnWeights.Count)
            {
                _totalWeight -= spawnWeights[index];
                spawnWeights.RemoveAt(index);
            }
            enemyPrefabs.RemoveAt(index);
        }
    }
}