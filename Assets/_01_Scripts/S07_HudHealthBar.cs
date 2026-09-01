using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class S07_HudHealthBar : MonoBehaviour
{
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text defenseText;

    // 🔧 Links directly to S04_HealthComponent
    [SerializeField] private S04_HealthComponent playerHealth;

    private void Start()
    {
        if (playerHealth == null) playerHealth = FindFirstObjectByType<S04_HealthComponent>();
        if (playerHealth == null) return;

        playerHealth.OnHealthChanged.AddListener(RefreshBar);
        RefreshBar(playerHealth.CurrentHp, playerHealth.MaxHp);
    }

    private void OnDestroy()
    {
        if (playerHealth != null) playerHealth.OnHealthChanged.RemoveListener(RefreshBar);
    }

    private void RefreshBar(float current, float max)
    {
        hpSlider.value = max > 0f ? current / max : 0f;
        hpText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        if (defenseText != null) defenseText.text = $"DEF: {Mathf.RoundToInt(playerHealth.Defense)}";
    }
}