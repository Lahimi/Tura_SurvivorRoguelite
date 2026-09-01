using UnityEngine;
using UnityEngine.Events;

public class S04_HealthComponent : MonoBehaviour
{
    [Header("Base Stats")]
    public float baseMaxHp = 100f;
    public float baseDefense = 0f;

    [Header("Data Channel")]
    public SO_GameSession gameSession;

    private float _bonusMaxHp;
    private float _bonusDefense;
    private float _currentHp;

    public float MaxHp => baseMaxHp + _bonusMaxHp;
    public float Defense => baseDefense + _bonusDefense;
    public float CurrentHp => _currentHp;
    public bool IsDead => _currentHp <= 0f;

    [Header("Event Channels")]
    public UnityEvent<float, float> OnHealthChanged;
    public UnityEvent OnDeath;

    private void Awake()
    {
        if (gameSession != null) gameSession.EnsureValidSelection();
        if (gameSession != null && gameSession.selectedCharacter != null)
        {
            baseMaxHp = gameSession.selectedCharacter.stats.maxHp;
            baseDefense = gameSession.selectedCharacter.stats.defense;
        }
        _currentHp = MaxHp;
    }

    public void TakeDamage(float raw)
    {
        if (IsDead) return;
        float dmg = Mathf.Max(raw - Defense, 1f);
        _currentHp = Mathf.Max(_currentHp - dmg, 0f);
        OnHealthChanged?.Invoke(_currentHp, MaxHp);
        if (_currentHp <= 0f) OnDeath?.Invoke();
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        _currentHp = Mathf.Min(_currentHp + amount, MaxHp);
        OnHealthChanged?.Invoke(_currentHp, MaxHp);
    }

    // Called by S08_PlayerXP and S06_StatPickup
    public void AddMaxHp(float flat)
    {
        _bonusMaxHp += flat;
        _currentHp = Mathf.Min(_currentHp + flat, MaxHp);
        OnHealthChanged?.Invoke(_currentHp, MaxHp);
    }

    // Called by S11_PlayerPowerUps
    public void IncreaseMaxHp(float flat) 
    {
        AddMaxHp(flat);
    }

    public void AddDefense(float flat)
    {
        _bonusDefense += flat;
        OnHealthChanged?.Invoke(_currentHp, MaxHp);
    }
}