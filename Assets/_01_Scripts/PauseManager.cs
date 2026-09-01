using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PauseManager : MonoBehaviour
{
    private static PauseManager _instance;

    public static PauseManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<PauseManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("PauseManager_RuntimeFallback");
                    _instance = go.AddComponent<PauseManager>();
                    Debug.Log("[PauseManager] Created automatic runtime fallback.");
                }
            }
            return _instance;
        }
    }

    Canvas _pauseCanvas;
    bool _isPaused;

    void Awake()
    {
        if (_instance != null && _instance != this) 
        { 
            Destroy(gameObject); 
            return; 
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        BuildOverlay();
    }

    void Update()
    {
        bool transitioning = SceneTransition.Instance != null && SceneTransition.Instance.IsLoading;
        if (transitioning) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 🛑 NEW INTERCEPTOR CHECK: Check if the Level Up UI window is actively open
            S12_LevelUpOverlay levelUpOverlay = FindFirstObjectByType<S12_LevelUpOverlay>();
            if (levelUpOverlay != null && levelUpOverlay.IsActive)
            {
                Debug.Log("[PauseManager] Pause layout window request blocked because S12_LevelUpOverlay is active.");
                return; // Blocks the Pause Menu from opening!
            }

            SetPaused(!_isPaused);
        }
    }

    public void SetPaused(bool paused)
    {
        _isPaused      = paused;
        Time.timeScale = paused ? 0f : 1f; 
        
        if (_pauseCanvas != null)
        {
            _pauseCanvas.gameObject.SetActive(paused);
            
            if (paused)
            {
                EnsureEventSystemExists();
            }
        }
    }

    public void OnContinue() => SetPaused(false);

    public void OnExitToMainMenu()
    {
        SetPaused(false); 
        
        if (SceneTransition.Instance != null)
        {
            SceneTransition.Instance.LoadScene("SC_MainMenu");
        }
        else
        {
            SceneManager.LoadScene("SC_MainMenu");
        }
    }

    private void EnsureEventSystemExists()
    {
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemGO = new GameObject("EventSystem_RuntimeFallback");
            eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("[PauseManager] Spawned missing EventSystem to allow UI clicks.");
        }
    }

    // ── 🎨 Code-Built UI Generation ────────────────────────────────────────

    void BuildOverlay()
    {
        var canvasGO = new GameObject("PauseOverlay");
        canvasGO.transform.SetParent(transform);
        _pauseCanvas = canvasGO.AddComponent<Canvas>();
        _pauseCanvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        _pauseCanvas.sortingOrder = 998; 
        
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Background Panel
        var bg    = new GameObject("Background");
        bg.transform.SetParent(_pauseCanvas.transform, false);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0f, 0f, 0f, 0.75f);
        var bgRT  = bg.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;

        // Center Container Panel
        var panel    = new GameObject("Panel");
        panel.transform.SetParent(_pauseCanvas.transform, false);
        var panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.08f, 0.08f, 0.12f, 1f); 
        var panelRT   = panel.GetComponent<RectTransform>();
        panelRT.anchorMin = panelRT.anchorMax = panelRT.pivot = new Vector2(0.5f, 0.5f);
        panelRT.sizeDelta        = new Vector2(420f, 320f);
        panelRT.anchoredPosition = Vector2.zero;

        // Title text
        var titleGO = new GameObject("Title");
        titleGO.transform.SetParent(panel.transform, false);
        var title = titleGO.AddComponent<TextMeshProUGUI>();
        title.text      = "PAUSED";
        title.fontSize  = 42f;
        title.alignment = TextAlignmentOptions.Center;
        title.color     = Color.white;
        title.raycastTarget = false; 
        var titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0f, 0.6f);
        titleRT.anchorMax = new Vector2(1f, 1f);
        titleRT.offsetMin = titleRT.offsetMax = Vector2.zero;

        // Create buttons
        MakeButton(panel.transform, "Continue",          new Vector2(0f,  40f), OnContinue);
        MakeButton(panel.transform, "Exit to Main Menu", new Vector2(0f, -40f), OnExitToMainMenu);

        _pauseCanvas.gameObject.SetActive(false);
    }

    void MakeButton(Transform parent, string label, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        var go  = new GameObject(label + "Button");
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.18f, 0.38f, 0.78f, 1f);  
        
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);
        
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta        = new Vector2(280f, 60f);
        rt.anchoredPosition = pos;
        
        var textGO = new GameObject("Label");
        textGO.transform.SetParent(go.transform, false);
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 22f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;
        tmp.raycastTarget = false; 
        
        var textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = textRT.offsetMax = Vector2.zero;
    }
}