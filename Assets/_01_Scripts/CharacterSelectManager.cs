using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class CharacterSelectManager : MonoBehaviour
{
    [Header("Data")]
    public SO_GameSession gameSession;

    [Header("Card Row")]
    public Transform cardContainer;
    public GameObject cardPrefab;

    [Header("Preview Panel")]
    public Image previewIcon;
    public TMP_Text previewName;
    public Transform statsContainer;
    public GameObject statRowPrefab;

    [Header("Confirm")]
    public Button confirmButton;
    
    [Header("Scene Names")]
    [Tooltip("The main gameplay scene name")]
    public string gameplaySceneName = "SC_Gameplay";
    [Tooltip("Fallback gym scene if no target is set")]
    public string fallbackGymScene = "SC_GYM_CharacterControl";

    List<CharacterCard> _cards = new();
    int _selectedIndex = -1;
    float _navCooldown;
    const float NAV_DELAY = 0.25f;

    void Start()
    {
        confirmButton?.onClick.AddListener(ConfirmSelection);
        LoadCharacters();
    }

    void LoadCharacters()
    {
        var roster = CharacterLoader.Load();

        for (int i = 0; i < roster.characters.Count; i++)
        {
            var go = Instantiate(cardPrefab, cardContainer);
            var card = go.GetComponent<CharacterCard>();
            card.Init(roster.characters[i]);

            int index = i;
            card.OnClick = () => SelectCard(index);

            _cards.Add(card);
        }

        if (_cards.Count > 0) SelectCard(0);
    }

    void Update()
    {
        _navCooldown -= Time.deltaTime;
        if (_navCooldown > 0f) return;

        float x = Input.GetAxisRaw("Horizontal");
        if (x > 0.5f) { Navigate(1); _navCooldown = NAV_DELAY; }
        else if (x < -0.5f) { Navigate(-1); _navCooldown = NAV_DELAY; }
    }

    void Navigate(int direction)
    {
        int next = Mathf.Clamp(_selectedIndex + direction, 0, _cards.Count - 1);
        SelectCard(next);
    }

    void SelectCard(int index)
    {
        if (_selectedIndex >= 0 && _selectedIndex < _cards.Count)
            _cards[_selectedIndex].SetSelected(false);

        _selectedIndex = index;
        _cards[_selectedIndex].SetSelected(true);

        UpdatePreview(_cards[_selectedIndex].Data);
    }

    void UpdatePreview(CharacterData data)
    {
        previewName.text = data.displayName;

        if (ColorUtility.TryParseHtmlString(data.colorHex, out Color c))
            previewIcon.color = c;

        foreach (Transform child in statsContainer)
            Destroy(child.gameObject);

        SpawnStatRow("Move Speed", data.stats.moveSpeed.ToString("F1"));
        SpawnStatRow("Max Health", data.stats.maxHp.ToString("F0"));
        SpawnStatRow("Defense Tier", data.stats.defense.ToString("F0"));
    }

    void SpawnStatRow(string label, string value)
    {
        var go = Instantiate(statRowPrefab, statsContainer);
        var texts = go.GetComponentsInChildren<TMP_Text>();

        if (texts.Length >= 2)
        {
            texts[0].text = label;
            texts[1].text = value;
        }
    }

    void ConfirmSelection()
    {
        if (_cards.Count == 0 || _selectedIndex < 0) return;

        // Save selected character
        if (gameSession != null)
        {
            gameSession.selectedCharacter = _cards[_selectedIndex].Data;
            Debug.Log($"[Select] Character locked: {gameSession.selectedCharacter.displayName}");
        }

        // Determine which scene to load
        string sceneToLoad;
        
        // Check if we came from a specific gym selection
        if (gameSession != null && !string.IsNullOrEmpty(gameSession.targetGymScene))
        {
            sceneToLoad = gameSession.targetGymScene;
            Debug.Log($"[Select] Loading gym scene: {sceneToLoad}");
        }
        else
        {
            // Default: Load main gameplay scene
            sceneToLoad = gameplaySceneName;
            Debug.Log($"[Select] Loading gameplay scene: {sceneToLoad}");
        }

        // Load the scene
        if (SceneTransition.Instance != null)
        {
            SceneTransition.Instance.LoadScene(sceneToLoad);
        }
        else
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}