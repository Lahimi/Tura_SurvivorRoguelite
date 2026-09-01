using UnityEngine;

public class WP_BoomerangWeapon : MonoBehaviour
{
    [Header("Prefab Projections")]
    [SerializeField] private GameObject standardBoomerangPrefab;
    [SerializeField] private GameObject magneticRebounderPrefab;

    [Header("Firing Interval Configurations")]
    [SerializeField] private float baseAttackInterval = 2.5f;
    [SerializeField] private float weaponDamage = 18f;

    [Header("Runtime Progression States")]
    public int weaponTier = 0; 
    public bool isMagneticUnlocked = false;

    private float _attackCooldownTimer;

    private void Update()
    {
        if (Time.timeScale == 0f || (weaponTier <= 0 && !isMagneticUnlocked)) return;

        _attackCooldownTimer += Time.deltaTime;
        if (_attackCooldownTimer >= baseAttackInterval)
        {
            _attackCooldownTimer = 0f;
            FireBoomerangWeapon();
        }
    }

    private void FireBoomerangWeapon()
    {
        Vector3 fireDirection = Vector3.up; 

        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, 8f);
        float closestDistance = Mathf.Infinity;
        
        foreach (var col in nearbyEnemies)
        {
            if (col.CompareTag("Enemy"))
            {
                float dist = Vector3.Distance(transform.position, col.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    fireDirection = (col.transform.position - transform.position).normalized;
                }
            }
        }

        if (isMagneticUnlocked)
        {
            // Mode B: Force MagneticRebounder mode explicitly on launch
            SpawnProjectile(magneticRebounderPrefab, fireDirection, WP_BoomerangProjectile.BoomerangMode.MagneticRebounder);
            SpawnProjectile(magneticRebounderPrefab, -fireDirection, WP_BoomerangProjectile.BoomerangMode.MagneticRebounder);
        }
        else
        {
            int projectileCount = 1;
            if (weaponTier >= 3) projectileCount++;
            if (weaponTier >= 5) projectileCount++;

            for (int i = 0; i < projectileCount; i++)
            {
                float spreadAngle = (i - (projectileCount - 1) / 2f) * 15f;
                Vector3 spreadDirection = Quaternion.Euler(0, 0, spreadAngle) * fireDirection;
                
                // Mode A: Force Standard mode explicitly on launch
                SpawnProjectile(standardBoomerangPrefab, spreadDirection, WP_BoomerangProjectile.BoomerangMode.Standard);
            }
        }
    }

    private void SpawnProjectile(GameObject prefab, Vector3 direction, WP_BoomerangProjectile.BoomerangMode launchMode)
    {
        if (prefab == null) return;
        GameObject proj = Instantiate(prefab, transform.position, Quaternion.identity);
        if (proj.TryGetComponent<WP_BoomerangProjectile>(out var component))
        {
            // Passes the explicit launchMode down directly
            component.Initialize(transform, direction, weaponDamage, launchMode);
        }
    }
}