using UnityEngine;

// Base enemy behavior: moves toward the player, deals contact damage,
// dies when S04_HealthComponent reaches 0 HP, drops an XP gem.
//
// Prefab setup:
//   - Rigidbody2D: gravityScale=0, Freeze Rotation Z
//   - CircleCollider2D: isTrigger=false (physically pushes the player)
//   - S04_HealthComponent: set baseMaxHp in the Inspector; leave gameSession EMPTY
//   - Tag: "Enemy"
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(S04_HealthComponent))]
public class S14_EnemyBase : MonoBehaviour
{
    [Header("Movement")]
    public float enemySpeed = 3f;

    [Header("Contact Damage")]
    public float contactDamage = 10f;
    public float contactTickInterval = 0.5f; // seconds between damage ticks while touching

    [Header("Drop")]
    // How much XP this enemy gives when killed.
    public float xpDrop = 5f;
    // Drag PF_XpGem here.
    public GameObject gemPrefab;

    Rigidbody2D _rb;
    S04_HealthComponent _health;
    Transform _player;
    S08_PlayerXP _playerXp;
    float _contactTimer;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _health = GetComponent<S04_HealthComponent>();

        // Subscribe Die() to the health component's death event.
        // OnDeath fires exactly once when HP reaches 0 (S04_HealthComponent guarantees this).
        _health.OnDeath.AddListener(Die);
    }

    void Start()
    {
        // FindWithTag in Start (not Awake) so the player has time to initialize first.
        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null) return;

        _player = playerObj.transform;
        _playerXp = playerObj.GetComponent<S08_PlayerXP>();
    }

    void FixedUpdate()
    {
        if (_player == null) return;

        Vector2 dir = ((Vector2)_player.position - _rb.position).normalized;
        _rb.linearVelocity = dir * enemySpeed;
    }

    // OnCollisionStay2D fires every physics frame while this enemy overlaps the player.
    // The timer prevents damage every frame — ticks at contactTickInterval rate instead.
    void OnCollisionStay2D(Collision2D col)
    {
        if (!col.gameObject.CompareTag("Player")) return;

        _contactTimer += Time.fixedDeltaTime;
        if (_contactTimer >= contactTickInterval)
        {
            _contactTimer = 0f;
            col.gameObject.GetComponent<S04_HealthComponent>()?.TakeDamage(contactDamage);
        }
    }

    void OnCollisionExit2D(Collision2D col)
    {
        // Reset the timer when the player separates so the next touch waits a full interval.
        if (col.gameObject.CompareTag("Player"))
            _contactTimer = 0f;
    }

    void Die()
    {
        if (gemPrefab != null && _player != null)
        {
            var gem = Instantiate(gemPrefab, transform.position, Quaternion.identity);
            var xpGem = gem.GetComponent<S09_XpGem>();
            if (xpGem != null)
            {
                xpGem.xpValue = xpDrop;
                xpGem.Init(_player, _playerXp);
            }
        }

        Destroy(gameObject);
    }
}