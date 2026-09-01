using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class S05_DamageZone : MonoBehaviour
{
    [Header("Hazard Configuration")]
    [Tooltip("The amount of raw damage dealt per interval cycle.")]
    [SerializeField] private float damagePerTick = 10f;
    
    [Tooltip("How frequently the hazard applies damage (in seconds).")]
    [SerializeField] private float tickInterval = 0.5f;

    private float _timer;

    private void Start()
    {
        var col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
            Debug.Log($"[DamageZone] Enforced 'Is Trigger' configuration on {gameObject.name}");
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // 🔧 Refactored reference link target to match S04 architecture matrix
        var health = other.GetComponent<S04_HealthComponent>();
        if (health == null) return;

        _timer += Time.fixedDeltaTime;
        
        if (_timer >= tickInterval)
        {
            _timer = 0f;
            health.TakeDamage(damagePerTick);
            Debug.Log($"[Hazard] Applied {damagePerTick} raw damage to target: {other.name}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<S04_HealthComponent>() != null)
        {
            _timer = 0f;
        }
    }
}