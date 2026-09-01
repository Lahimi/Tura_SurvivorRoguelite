using System.Collections.Generic;
using UnityEngine;

public class WP_Tornado : MonoBehaviour
{
    [Header("Tornado Settings")]
    [SerializeField] private float hitCooldown = 0.3f;
    [SerializeField] private float tickDamageInterval = 0.2f;
    
    private float _damage;
    private float _lifetime;
    private float _speed;
    private Vector2 _direction;
    private bool _trackEnemy;
    private Transform _targetEnemy;
    private float _lifeTimer;
    private float _damageTimer;
    
    private readonly Dictionary<Collider2D, float> _enemyCooldownTimers = new Dictionary<Collider2D, float>();
    private readonly List<Collider2D> _cleanupList = new List<Collider2D>();
    
    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;
    private Color _startColor;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null) _startColor = _spriteRenderer.color;
    }

    public void Initialize(float damage, float lifetime, Vector2 direction, float speed, bool trackEnemy)
    {
        _damage = damage;
        _lifetime = lifetime;
        _direction = direction.normalized;
        _speed = speed;
        _trackEnemy = trackEnemy;
        _lifeTimer = 0f;
        _damageTimer = 0f;
        
        if (_rb != null)
        {
            _rb.linearVelocity = _direction * _speed;
        }
        
        // Find nearest enemy if tracking is enabled
        if (_trackEnemy)
        {
            _targetEnemy = GetNearestEnemy();
        }
        
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        _lifeTimer += Time.deltaTime;
        _damageTimer += Time.deltaTime;
        
        // Track enemy if enabled
        if (_trackEnemy && _targetEnemy != null)
        {
            Vector2 newDirection = (_targetEnemy.position - transform.position).normalized;
            if (_rb != null)
            {
                _rb.linearVelocity = newDirection * _speed;
            }
        }
        
        // Fade out over time
        if (_spriteRenderer != null)
        {
            float alpha = 1f - (_lifeTimer / _lifetime);
            _spriteRenderer.color = new Color(_startColor.r, _startColor.g, _startColor.b, alpha);
        }
        
        // Scale up slightly over time
        float scale = 0.5f + (_lifeTimer / _lifetime) * 0.5f;
        transform.localScale = new Vector3(scale, scale, 1f);
        
        HandleCooldownTimers();
    }

    private void FixedUpdate()
    {
        if (_damageTimer >= tickDamageInterval)
        {
            _damageTimer = 0f;
            DamageNearbyEnemies();
        }
    }

    private void DamageNearbyEnemies()
    {
        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, 1f);
        
        foreach (var enemy in nearbyEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                if (_enemyCooldownTimers.ContainsKey(enemy))
                {
                    if (Time.time < _enemyCooldownTimers[enemy]) continue;
                }
                
                _enemyCooldownTimers[enemy] = Time.time + hitCooldown;
                
                if (enemy.TryGetComponent<S04_HealthComponent>(out var health))
                {
                    health.TakeDamage(_damage);
                }
            }
        }
    }

    private Transform GetNearestEnemy()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, 10f);
        Transform nearest = null;
        float closestDistance = Mathf.Infinity;
        
        foreach (var enemy in enemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    nearest = enemy.transform;
                }
            }
        }
        return nearest;
    }

    private void HandleCooldownTimers()
    {
        if (_enemyCooldownTimers.Count == 0) return;
        
        _cleanupList.Clear();
        float currentTime = Time.time;
        
        foreach (var kvp in _enemyCooldownTimers)
        {
            if (currentTime >= kvp.Value) _cleanupList.Add(kvp.Key);
        }
        
        foreach (var col in _cleanupList)
        {
            if (col != null) _enemyCooldownTimers.Remove(col);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Immediate damage on contact
        if (other.CompareTag("Enemy"))
        {
            if (other.TryGetComponent<S04_HealthComponent>(out var health))
            {
                health.TakeDamage(_damage);
            }
        }
    }

    private void OnDisable()
    {
        _enemyCooldownTimers.Clear();
    }
}