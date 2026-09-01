using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(CircleCollider2D))]
public class S09_XpGem : MonoBehaviour
{
    [Header("Gem Configuration")]
    [Tooltip("The flat amount of experience points awarded to the player upon collection.")]
    [SerializeField] public float xpValue = 10f;

    // ✨ public property wrapper allows outside scripts (like S14_EnemyBase) 
    // to read or overwrite the xp amount dynamically at instantiation.
    public float XpValue
    {
        get => xpValue;
        set => xpValue = value;
    }

    [Tooltip("The constant velocity speed (units per second) at which the gem travels toward the player when magnetized.")]
    [SerializeField] private float pullSpeed = 6f;

    // Interior structural dependencies injected at spawn time
    private Transform _player;
    private S08_PlayerXP _playerXp;
    
    // State machine lock
    private bool _magnetized = false;

    private void Start()
    {
        // Enforce physical boundary safety guarantees
        var col = GetComponent<CircleCollider2D>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
        }
    }

    /// <summary>
    /// Injects the tracking references upon creation. 
    /// Replaces expensive runtime scene graph searches with clean memory pointers.
    /// </summary>
    public void Init(Transform playerTransform, S08_PlayerXP xpComponent)
    {
        _player = playerTransform;
        _playerXp = xpComponent;
    }

    private void Update()
    {
        // Guard: If the gem hasn't been initialized by a spawner yet, sit idle
        if (_player == null || _playerXp == null) return;

        // Step 1: Distance evaluation & permanent state latching
        if (!_magnetized)
        {
            float distance = Vector2.Distance(transform.position, _player.position);
            
            // Evaluates directly against the player's live property matrix
            if (distance <= _playerXp.MagnetRadius)
            {
                _magnetized = true;
                Debug.Log($"[Gem Channel] {gameObject.name} locked onto player magnet field.");
            }
        }

        // Step 2: Linear vector tracking execution
        if (_magnetized)
        {
            transform.position = Vector2.MoveTowards(
                transform.position, 
                _player.position, 
                pullSpeed * Time.deltaTime
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verify collision intersection targets the active player entity
        if (_playerXp != null && other.gameObject == _playerXp.gameObject)
        {
            // Award experience into the progression track pipeline
            _playerXp.AddXP(xpValue);
            
            // Clean up the object from the rendering loop hierarchy immediately
            Destroy(gameObject);
        }
    }
}