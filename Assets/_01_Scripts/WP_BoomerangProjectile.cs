using UnityEngine;

public class WP_BoomerangProjectile : MonoBehaviour
{
    public enum BoomerangMode { Standard, MagneticRebounder }
    
    [Header("Behavior Selection")]
    [SerializeField] private BoomerangMode mode = BoomerangMode.Standard;

    [Header("Movement Configuration")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float visualRotationSpeed = 720f;

    [Header("Standard Return Parameters")]
    [SerializeField] private float maxFlightDuration = 0.8f;

    [Header("Magnetic Spiral Parameters")]
    [SerializeField] private float spiralExpansionSpeed = 3f;
    [SerializeField] private float spiralRotationSpeed = 5f;

    private Transform _playerTransform;
    private Vector3 _fireDirection;
    private float _damage = 10f;
    
    private float _lifetimeTimer;
    private bool _isReturning;
    private float _currentSpiralRadius;
    private float _spiralAngle;

    // Fixed: Added BoomerangMode parameter so the weapon manager forces the correct behavior profile
    public void Initialize(Transform player, Vector3 direction, float damageValue, BoomerangMode launchMode)
    {
        _playerTransform = player;
        _fireDirection = direction.normalized;
        _damage = damageValue;
        _lifetimeTimer = 0f;
        _isReturning = false;
        mode = launchMode; // Force the mode programmatically on instantiation!
        
        if (mode == BoomerangMode.MagneticRebounder)
        {
            _currentSpiralRadius = 0.5f;
            _spiralAngle = Mathf.Atan2(_fireDirection.y, _fireDirection.x);
        }
    }

    private void Update()
    {
        if (_playerTransform == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.Rotate(Vector3.forward, visualRotationSpeed * Time.deltaTime);
        _lifetimeTimer += Time.deltaTime;

        if (mode == BoomerangMode.Standard)
        {
            ExecuteStandardMovement();
        }
        else
        {
            ExecuteMagneticSpiralMovement();
        }
    }

    private void ExecuteStandardMovement()
    {
        if (!_isReturning)
        {
            transform.position += _fireDirection * speed * Time.deltaTime;

            if (_lifetimeTimer >= maxFlightDuration)
            {
                _isReturning = true;
            }
        }
        else
        {
            Vector3 directionToPlayer = (_playerTransform.position - transform.position).normalized;
            transform.position += directionToPlayer * (speed * 1.2f) * Time.deltaTime;

            if (Vector3.Distance(transform.position, _playerTransform.position) < 0.5f)
            {
                Destroy(gameObject);
            }
        }
    }

    private void ExecuteMagneticSpiralMovement()
    {
        _spiralAngle += spiralRotationSpeed * Time.deltaTime;
        _currentSpiralRadius += spiralExpansionSpeed * Time.deltaTime;

        Vector3 offset = new Vector3(Mathf.Cos(_spiralAngle), Mathf.Sin(_spiralAngle), 0f) * _currentSpiralRadius;
        transform.position = _playerTransform.position + offset;

        if (_lifetimeTimer >= 4.0f)
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
                Debug.Log($"[BOOMERANG HIT] Dealt {_damage} damage to {other.name}!");
            }
        }
    }
}