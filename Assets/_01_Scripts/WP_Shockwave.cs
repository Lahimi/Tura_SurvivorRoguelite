using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WP_Shockwave : MonoBehaviour
{
    [SerializeField] private float hitCooldown = 0.5f;
    [SerializeField] private float moveSpeed = 10f;
    
    private float _damage;
    private float _duration;
    private float _radius;
    private float _travelDistance;
    private Vector2 _direction;
    private Vector3 _startPosition;
    private float _timer;
    
    private readonly Dictionary<Collider2D, float> _enemyCooldownTimers = new Dictionary<Collider2D, float>();
    private readonly List<Collider2D> _cleanupList = new List<Collider2D>();
    
    private CircleCollider2D _collider;
    private SpriteRenderer _spriteRenderer;
    private Color _startColor;

    private void Awake()
    {
        _collider = GetComponent<CircleCollider2D>();
        if (_collider != null) _collider.isTrigger = true;
        
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null) _startColor = _spriteRenderer.color;
    }

    public void Initialize(float damage, float duration, float radius, Vector2 direction, float travelDistance)
    {
        _damage = damage;
        _duration = duration;
        _radius = radius;
        _direction = direction.normalized;
        _travelDistance = travelDistance;
        _startPosition = transform.position;
        _timer = 0f;
        
        if (_collider != null) _collider.radius = radius;
        
        if (_spriteRenderer != null)
        {
            float visualScale = radius * 2f;
            transform.localScale = new Vector3(visualScale, visualScale, 1f);
        }
        
        // FIXED: Calculate angle so shockwave faces the direction it's moving
        // If moving Right (1,0), angle should be 0° (facing right)
        // If moving Up (0,1), angle should be 90° (facing up)
        // If moving Left (-1,0), angle should be 180° (facing left)
        // If moving Down (0,-1), angle should be -90° or 270° (facing down)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        
        Destroy(gameObject, duration);
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        
        // Move outward in the set direction
        transform.position += (Vector3)_direction * moveSpeed * Time.deltaTime;
        
        // Fade out over time
        if (_spriteRenderer != null)
        {
            float alpha = 1f - (_timer / _duration);
            _spriteRenderer.color = new Color(_startColor.r, _startColor.g, _startColor.b, alpha);
        }
        
        HandleCooldownTimers();
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

    private void OnTriggerEnter2D(Collider2D other) => EvaluateHit(other);
    private void OnTriggerStay2D(Collider2D other) => EvaluateHit(other);

    private void EvaluateHit(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy")) return;
        
        float currentTime = Time.time;
        
        if (_enemyCooldownTimers.ContainsKey(collision))
        {
            if (currentTime < _enemyCooldownTimers[collision]) return;
        }
        
        _enemyCooldownTimers[collision] = currentTime + hitCooldown;
        
        if (collision.TryGetComponent<S04_HealthComponent>(out var enemyHealth))
        {
            enemyHealth.TakeDamage(_damage);
        }
    }

    private void OnDisable() => _enemyCooldownTimers.Clear();
}