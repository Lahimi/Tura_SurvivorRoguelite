using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class S06_StatPickup : MonoBehaviour
{
    public enum StatType { MaxHp, Defense }

    [Header("Pickup Settings")]
    [SerializeField] private StatType statType = StatType.MaxHp;
    [SerializeField] private float amount = 20f;

    private void Start()
    {
        var col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 🔧 Links directly to S04_HealthComponent
        var health = other.GetComponent<S04_HealthComponent>();
        if (health == null) return;

        switch (statType)
        {
            case StatType.MaxHp:
                health.AddMaxHp(amount);
                break;
            case StatType.Defense:
                health.AddDefense(amount);
                break;
        }
        Destroy(gameObject);
    }
}