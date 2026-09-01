using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(S04_HealthComponent))]
[RequireComponent(typeof(S02_PlayerController))]
public class S08_PlayerXP : MonoBehaviour
{
    [Header("Data")]
    public SO_GameSession gameSession;

    [Header("XP Curve")]
    [SerializeField] private float baseXpThreshold = 30f;
    [SerializeField] private float xpCurveExponent = 1.2f;

    [Header("Stat Growth Fallbacks")]
    // Used when no character is selected (direct GYM scene launch)
    [SerializeField] private float fallbackHpGrowth    = 15f;
    [SerializeField] private float fallbackDefGrowth   = 2f;
    [SerializeField] private float fallbackSpeedGrowth = 0f;

    [Header("Magnet Radius")]
    [SerializeField] private float baseMagnetRadius = 2f;

    [Header("Events")]
    public UnityEvent<int>          OnLevelUp;   // passes new level
    public UnityEvent<float, float> OnXpChanged; // passes (current, max)

    // Public properties for other scripts to read
    public int   Level        { get; private set; } = 1;
    public float CurrentXp    { get; private set; }
    public float MagnetRadius => baseMagnetRadius + _bonusMagnetRadius;
    public float XpToNextLevel => Mathf.Floor(baseXpThreshold * Mathf.Pow(Level, xpCurveExponent));

    // Exposed so the overlay can display the per-level delta
    public float HpGrowth    => _hpGrowth;
    public float DefGrowth   => _defGrowth;
    public float SpeedGrowth => _speedGrowth;

    // Private runtime tracking values
    private float _hpGrowth;
    private float _defGrowth;
    private float _speedGrowth;
    private float _bonusMagnetRadius = 0f;

    // Cached references
    private S04_HealthComponent _health;
    private S02_PlayerController _movement;

    private void Awake()
    {
        _health   = GetComponent<S04_HealthComponent>();
        _movement = GetComponent<S02_PlayerController>();

        // Load specific growth traits from session data if configured, otherwise fall back
        if (gameSession != null && gameSession.selectedCharacter != null)
        {
            var s        = gameSession.selectedCharacter.stats;
            _hpGrowth    = s.hpGrowth;
            _defGrowth   = s.defGrowth;
            _speedGrowth = s.speedGrowth;
        }
        else
        {
            _hpGrowth    = fallbackHpGrowth;
            _defGrowth   = fallbackDefGrowth;
            _speedGrowth = fallbackSpeedGrowth;
        }
    }

    private void Start()
    {
        // Broadcast initialization numbers so HUD elements align perfectly on frame 0
        OnXpChanged?.Invoke(CurrentXp, XpToNextLevel);
    }

    /// <summary>
    /// Adds raw experience points to the player's progression pool and handles cascading level threshold crossings.
    /// </summary>
    public void AddXP(float amount)
    {
        if (amount <= 0f) return;

        CurrentXp += amount;

        // Loop runs continuously as long as your current XP overflows the dynamic target requirement of the current level
        while (CurrentXp >= XpToNextLevel)
        {
            // 1. Capture the exact target threshold of the level we are processing before incrementing
            float requiredXpForThisLevel = XpToNextLevel;

            // 2. Drain that completed milestone amount out of your total pool 
            CurrentXp -= requiredXpForThisLevel;

            // 3. Step up the numeric level value (which automatically scales up the next XpToNextLevel value)
            Level++;

            // 4. Apply structural stat increases directly to core components
            if (_health != null)
            {
                _health.AddMaxHp(_hpGrowth);
                _health.AddDefense(_defGrowth);
            }

            if (_movement != null)
            {
                _movement.moveSpeed += _speedGrowth;
            }

            Debug.Log($"🔥 LEVEL UP! Player reached Level {Level}! Remaining local overflow pool: {CurrentXp}, Next target threshold is: {XpToNextLevel}");

            // 5. Invoke the level up event AFTER stats update so observers (like S12 overlay) capture accurate data
            OnLevelUp?.Invoke(Level);
        }

        // Always notify UI updates across standard experience adjustments with final remainder state readings
        OnXpChanged?.Invoke(CurrentXp, XpToNextLevel);
    }

    /// <summary>
    /// Expands the active area threshold where pickup objects begin tracking toward the player.
    /// Called externally by the S11_PlayerPowerUps system.
    /// </summary>
    public void AddMagnetRadius(float amount)
    {
        _bonusMagnetRadius += amount;
    }
}