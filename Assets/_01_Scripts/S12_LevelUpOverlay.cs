using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class S12_LevelUpOverlay : MonoBehaviour
{
    [Header("Serialized References")]
    [SerializeField] private GameObject overlayRoot;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text hpStatText;
    [SerializeField] private TMP_Text defStatText;
    [SerializeField] private TMP_Text spdStatText;
    [SerializeField] private Transform cardContainer;
    
    [Header("Card Design Prefab")]
    [SerializeField] private GameObject cardPrefab; // Assign your visual PF_PowerUpCard prefab here!

    [Header("Player Tracking Engines")]
    [SerializeField] private S08_PlayerXP playerXp;
    [SerializeField] private S11_PlayerPowerUps playerPowerUps;
    [SerializeField] private S04_HealthComponent playerHealth;
    [SerializeField] private S02_PlayerController playerMovement;

    // Tracker to handle stacked choice screens from overflowing levels
    private int _pendingLevelChoices = 0;

    /// <summary>
    /// Public property exposed so the Pause Menu script can check if this overlay is blocking input.
    /// </summary>
    public bool IsActive => overlayRoot != null && overlayRoot.activeSelf;

    private void Start()
    {
        if (overlayRoot != null)
        {
            overlayRoot.SetActive(false);
        }
    }

    /// <summary>
    /// Called automatically by S08's OnLevelUp UnityEvent.
    /// </summary>
    public void Show(int level)
    {
        _pendingLevelChoices++;
        Debug.Log($"[Level Up UI] Registered level {level}. Total choices pending in queue: {_pendingLevelChoices}");

        // If the overlay panel is already open making a choice, do not interrupt the player.
        if (overlayRoot != null && overlayRoot.activeSelf)
        {
            return;
        }

        ExecuteShowSequence(level);
    }

    /// <summary>
    /// Sets up and displays the visual card options on screen.
    /// </summary>
    private void ExecuteShowSequence(int level)
    {
        Time.timeScale = 0f;
        ClearActiveCards();

        if (levelText != null)
        {
            levelText.text = $"LEVEL {level}";
        }

        RefreshStatPanel();

        if (playerPowerUps != null)
        {
            List<SO_PowerUp> choices = playerPowerUps.GetLevelUpChoices();
            foreach (SO_PowerUp powerUpData in choices)
            {
                BuildCard(powerUpData);
            }
        }

        if (overlayRoot != null)
        {
            overlayRoot.SetActive(true);
        }
    }

    void RefreshStatPanel()
    {
        if (playerXp != null && playerHealth != null && playerMovement != null)
        {
            hpStatText.text  = $"HP    +{playerXp.HpGrowth}    (now {Mathf.CeilToInt(playerHealth.MaxHp)})";
            defStatText.text = $"DEF    +{playerXp.DefGrowth}    (now {Mathf.RoundToInt(playerHealth.Defense)})";

            if (playerXp.SpeedGrowth > 0f)
                spdStatText.text = $"SPD    +{playerXp.SpeedGrowth:F2}    (now {playerMovement.moveSpeed:F2})";
            else
                spdStatText.text = "SPD    —    (unchanged)";
        }
    }

    void BuildCard(SO_PowerUp data)
    {
        // 🛑 SAFEGUARD: Check for null data or baseline template strings to prevent visual layout corruption
        if (data == null) return;
        
        if (string.IsNullOrEmpty(data.displayName) || 
            data.displayName == "Power Up" || 
            data.displayName == "Haste")
        {
            Debug.LogWarning($"[LevelUpOverlay] Skipping generation for placeholder card data target: '{data.displayName}'. Logged safely instead of breaking UI layout structures.");
            return;
        }

        if (cardContainer == null || cardPrefab == null)
        {
            Debug.LogError("[LevelUpOverlay] Visual layout configuration references are missing in Inspector layout fields!");
            return;
        }

        // 1. Instantiate the card layout from your prefab asset folder
        GameObject spawnCardObj = Instantiate(cardPrefab, cardContainer);
        spawnCardObj.name = $"CardUI_{data.displayName}";

        // 2. Fetch the interactive button component from the root frame layout
        Button btn = spawnCardObj.GetComponent<Button>();

        // 3. Deep-scan all children elements to find your exact hierarchy names
        Image iconImg = null;
        TextMeshProUGUI txtName = null;
        TextMeshProUGUI txtDesc = null;
        TextMeshProUGUI txtStack = null;

        foreach (Image img in spawnCardObj.GetComponentsInChildren<Image>(true))
        {
            if (img.gameObject.name == "Icon")
            {
                iconImg = img;
                break;
            }
        }

        foreach (TextMeshProUGUI tmp in spawnCardObj.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (tmp.gameObject.name == "NameText") txtName = tmp;
            else if (tmp.gameObject.name == "DescriptionText") txtDesc = tmp;
            else if (tmp.gameObject.name == "StackText") txtStack = tmp;
        }

        // ====================================================================
        // 4. DATA INJECTION
        // ====================================================================
        if (txtName != null) txtName.text = data.displayName;
        if (txtDesc != null) txtDesc.text = data.description;

        if (iconImg != null)
        {
            if (data.icon != null)
            {
                iconImg.sprite = data.icon;
                iconImg.color = Color.white; 
            }
            else
            {
                iconImg.color = new Color(1f, 1f, 1f, 0f);
            }
        }

        // 5. Calculate stacking metrics (restored with +1 level preview handling tracking specifications)
        int currentStacks = 0;
        if (playerPowerUps != null && playerPowerUps.Acquired != null)
        {
            currentStacks = playerPowerUps.Acquired.ContainsKey(data) ? playerPowerUps.Acquired[data] : 0;
        }

        if (txtStack != null)
        {
            txtStack.text = $"{currentStacks + 1} / {data.maxStacks}";
        }

        // 6. Wire interaction listeners safely
        if (btn != null)
        {
            var captured = data; 
            btn.onClick.RemoveAllListeners(); 
            btn.onClick.AddListener(() => SelectPowerUp(captured));
        }
    }

    public void SelectPowerUp(SO_PowerUp selectedPowerUp)
    {
        if (playerPowerUps != null && selectedPowerUp != null)
        {
            playerPowerUps.Apply(selectedPowerUp);
        }

        _pendingLevelChoices--;

        if (_pendingLevelChoices > 0 && playerXp != null)
        {
            Debug.Log($"[Level Up Chaining] Processing next level choice. Remaining queue slots: {_pendingLevelChoices}");
            ExecuteShowSequence(playerXp.Level);
        }
        else
        {
            Hide();
        }
    }

    void Hide()
    {
        _pendingLevelChoices = 0;

        if (overlayRoot != null)
        {
            overlayRoot.SetActive(false);
        }

        ClearActiveCards();
        Time.timeScale = 1f;
    }

    void ClearActiveCards()
    {
        if (cardContainer == null) return;
        foreach (Transform child in cardContainer)
        {
            Destroy(child.gameObject);
        }
    }
}