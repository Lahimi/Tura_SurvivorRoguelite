using UnityEngine;

public class WP_SwordSlash : MonoBehaviour
{
    private float _damage = 10f;
    private float _duration = 0.3f; // Lifespan of the visual slash
    private float _timer;

    public void Initialize(float damageValue, float durationValue, Vector3 scaleMultiplier)
    {
        _damage = damageValue;
        _duration = durationValue;
        transform.localScale = Vector3.Scale(transform.localScale, scaleMultiplier);
        _timer = 0f;
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _duration)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (other.TryGetComponent<S04_HealthComponent>(out var enemyHealth))
            {
                enemyHealth.TakeDamage(_damage);
                Debug.Log($"[SWORD HIT] Dealt {_damage} damage to {other.name}!");
            }
        }
    }
}