using UnityEngine;

// Fired by S15_WeaponBase (Projectile mode). Moves in a straight line.
// Call Init() immediately after Instantiate to set direction and damage.
//
// Prefab setup:
//   - Rigidbody2D: gravityScale=0, Freeze Rotation Z, Collision Detection=Continuous
//   - CircleCollider2D: IsTrigger=true (detects enemy contact without physics push)
[RequireComponent(typeof(Rigidbody2D))]
public class S16_Projectile : MonoBehaviour
{
    [SerializeField] float lifetime = 3f;

    float _damage;
    Rigidbody2D _rb;

    void Awake() => _rb = GetComponent<Rigidbody2D>();

    // Called by S15_WeaponBase right after Instantiate.
    public void Init(Vector2 direction, float speed, float damage)
    {
        _damage = damage;
        _rb.linearVelocity = direction.normalized * speed;

        // Rotate the sprite to face the direction of travel.
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // Safety net: destroy stray projectiles that miss every enemy.
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        other.GetComponent<S04_HealthComponent>()?.TakeDamage(_damage);
        Destroy(gameObject);
    }
}