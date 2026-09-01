using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MenuController : MonoBehaviour
{
    [Header("Session Data")]
    public SO_GameSession gameSession;

    [Header("GYM System")]
    public S_GymRegistry gymRegistry;
    public GameObject gymButtonPrefab;
    public Transform gymContentParent;

    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject creditsPanel;
    public GameObject optionsPanel;
    public GameObject gymPanel;

    private void Start()
    {
        ShowMain(); 
        PopulateGymList();
    }

    // --- NEW: Start Game Button ---
    public void StartGame()
    {
        Debug.Log("[Menu] Starting game flow - Loading Character Selection");
        
        // Clear any previous target gym scene (we're going to gameplay, not gym)
        if (gameSession != null)
        {
            gameSession.targetGymScene = "SC_Gameplay";
        }
        
        // Load character selection scene
        if (SceneTransition.Instance != null)
        {
            SceneTransition.Instance.LoadScene("SC_CharacterSelect");
        }
        else
        {
            SceneManager.LoadScene("SC_CharacterSelect");
        }
    }

    // --- PANEL NAVIGATION ---

    public void ShowMain()
    {
        SetAllPanelsInactive();
        mainPanel.SetActive(true);
    }

    public void ShowCredits()
    {
        SetAllPanelsInactive();
        creditsPanel.SetActive(true);
    }

    public void ShowOptions()
    {
        SetAllPanelsInactive();
        optionsPanel.SetActive(true);
    }

    public void ShowGym()
    {
        SetAllPanelsInactive();
        gymPanel.SetActive(true);
    }

    // --- GYM GENERATION & ROUTING LOGIC ---

    private void PopulateGymList()
    {
        if (gymRegistry == null || gymButtonPrefab == null || gymContentParent == null) 
        {
            Debug.LogWarning("[Menu] Missing Registry, Prefab, or Parent assignments in the Inspector!");
            return;
        }

        foreach (Transform child in gymContentParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var entry in gymRegistry.scenes)
        {
            GameObject go = Instantiate(gymButtonPrefab, gymContentParent);
            GymSceneButton btnScript = go.GetComponent<GymSceneButton>();

            if (btnScript != null)
            {
                string path = entry.scenePath; 

                btnScript.Init(entry.displayName, () => {
                    
                    if (gameSession != null)
                    {
                        gameSession.targetGymScene = path;
                        Debug.Log($"[Menu] Dynamic target registered: {path}. Forwarding to selection.");
                    }

                    if (SceneTransition.Instance != null)
                    {
                        SceneTransition.Instance.LoadScene("SC_CharacterSelect");
                    }
                    else
                    {
                        SceneManager.LoadScene("SC_CharacterSelect");
                    }
                });
            }
        }
    }

    // --- APPLICATION CONTROLS ---

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SetAllPanelsInactive()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (gymPanel != null) gymPanel.SetActive(false);
    }
}