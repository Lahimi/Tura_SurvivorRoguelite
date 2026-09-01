using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class S13_HudXpBar : MonoBehaviour
{
    [Header("UI Component References")]
    [Tooltip("The UI Slider component representing the visual fill ratio of the experience track.")]
    [SerializeField] private Slider xpSlider;

    [Tooltip("Optional: Text mesh used to display numeric thresholds (e.g., '15 / 30 XP').")]
    [SerializeField] private TMP_Text xpText;

    [Tooltip("Text mesh used to render the current level integer display (e.g., 'LV 1').")]
    [SerializeField] private TMP_Text levelText;

    [Header("Target Tracking Core")]
    [Tooltip("Reference to the player's progression tracking component.")]
    [SerializeField] private S08_PlayerXP playerXp;

    private void Start()
    {
        // 🔧 Auto-Target Fallback: Locates tracking dependencies within the workspace dynamically
        if (playerXp == null)
        {
            playerXp = FindFirstObjectByType<S08_PlayerXP>();
        }

        if (playerXp == null)
        {
            Debug.LogWarning($"[HUD XP] Missing S08_PlayerXP reference link context on {gameObject.name}. HUD display tracking is suspended.");
            return;
        }

        // 🔧 Observer Pattern Subscriptions: Bind to event streams
        playerXp.OnXpChanged.AddListener(RefreshBar);
        playerXp.OnLevelUp.AddListener(RefreshLevel);

        // Enforce baseline frame-0 initialization to prevent blank visual flashes
        RefreshBar(playerXp.CurrentXp, playerXp.XpToNextLevel);
        RefreshLevel(playerXp.Level);
    }

    private void OnDestroy()
    {
        // 🔧 Safe Lifecycle Cleanups: Detach listeners to guarantee zero memory leaking behavior
        if (playerXp != null)
        {
            playerXp.OnXpChanged.RemoveListener(RefreshBar);
            playerXp.OnLevelUp.RemoveListener(RefreshLevel);
        }
    }

    /// <summary>
    /// Updates the slider fill and progress text display based on current milestone stats.
    /// </summary>
    private void RefreshBar(float current, float max)
    {
        if (xpSlider != null)
        {
            // Direct mathematical division-by-zero structural safeguards
            xpSlider.value = max > 0f ? current / max : 0f;
        }

        if (xpText != null)
        {
            // FloorToInt matching exact progression specifications
            xpText.text = $"{Mathf.FloorToInt(current)} / {Mathf.FloorToInt(max)} XP";
        }
    }

    /// <summary>
    /// Updates the text element mesh indicating the player's structural level.
    /// </summary>
    private void RefreshLevel(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"LV {level}";
        }
    }
}