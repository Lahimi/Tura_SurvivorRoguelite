using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SceneTransition : MonoBehaviour
{
    private static SceneTransition _instance;

    public static SceneTransition Instance
    {
        get
        {
            // If no instance is currently tracked, look for one in the active scene hierarchy
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<SceneTransition>();

                // LAZY INITIALIZATION: If it still doesn't exist (e.g., scene launched directly),
                // spawn it dynamically so the game pipeline never snaps.
                if (_instance == null)
                {
                    GameObject go = new GameObject("SceneTransition_RuntimeFallback");
                    _instance = go.AddComponent<SceneTransition>();
                    Debug.Log("[SceneTransition] No instance found. Created automatic runtime fallback.");
                }
            }
            return _instance;
        }
    }

    Canvas _overlayCanvas;
    Image  _spinner;
    bool   _isLoading;

    // 🔧 Read-only constraint: PauseManager checks this before allowing pause during a load.
    public bool IsLoading => _isLoading;

    void Awake()
    {
        // 🔧 Functionality Contract: Persistent Root Singleton Execution
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return; // Exits immediately so duplicates don't run initialization logic
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // Build the overlay dynamically if it hasn't been built yet
        if (_overlayCanvas == null)
        {
            BuildOverlay();
        }
    }

    /// <summary>
    /// Central public API entry point for scene loading across the game framework.
    /// Usage: SceneTransition.Instance.LoadScene("YourSceneName");
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (_isLoading) return;
        StartCoroutine(LoadRoutine(sceneName));
    }

    IEnumerator LoadRoutine(string sceneName)
    {
        _isLoading = true;
        _overlayCanvas.gameObject.SetActive(true);
        StartCoroutine(SpinRoutine());

        // 🔧 Functionality: Asynchronous background loading operation
        var op = SceneManager.LoadSceneAsync(sceneName);

        // yield return null suspends execution until the next frame.
        // This loop keeps running while letting Unity render frames behind the scenes.
        while (!op.isDone)
        {
            yield return null;
        }

        // Clean up when loading finishes
        _overlayCanvas.gameObject.SetActive(false);
        _isLoading = false;
    }

    IEnumerator SpinRoutine()
    {
        while (_overlayCanvas.gameObject.activeSelf)
        {
            // Rotates the UI spinner based on time delta instead of frame rate
            _spinner.transform.Rotate(0f, 0f, -270f * Time.deltaTime);
            yield return null;
        }
    }

    // ── 🎨 UI Overlay Construction ───────────────────────────────────────────
    void BuildOverlay()
    {
        var canvasGO = new GameObject("TransitionOverlay");
        canvasGO.transform.SetParent(transform);
        _overlayCanvas = canvasGO.AddComponent<Canvas>();
        _overlayCanvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        _overlayCanvas.sortingOrder = 999; // Ensures loading visuals always sit on top of everything
        
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // 🎨 Background Panel Setup
        var bg    = new GameObject("Background");
        bg.transform.SetParent(canvasGO.transform, false);
        var bgImg = bg.AddComponent<Image>();
        
        // Dark midnight theme - tweak this Color value if you want to match specific game color identities
        bgImg.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);
        var bgRT  = bg.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;

        // 🎨 Rotating Spinner Setup
        var spinnerGO = new GameObject("Spinner");
        spinnerGO.transform.SetParent(canvasGO.transform, false);
        _spinner = spinnerGO.AddComponent<Image>();
        
        // Bright cyan/blue focal indicator accent color
        _spinner.color = new Color(0.3f, 0.6f, 1f, 1f);
        var spinnerRT = spinnerGO.GetComponent<RectTransform>();
        spinnerRT.anchorMin = spinnerRT.anchorMax = spinnerRT.pivot = new Vector2(0.5f, 0.5f);
        spinnerRT.sizeDelta        = new Vector2(50f, 50f);
        spinnerRT.anchoredPosition = new Vector2(0f, 30f);

        // 🎨 Text Element Mesh Setup
        var textGO = new GameObject("LoadingText");
        textGO.transform.SetParent(canvasGO.transform, false);
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = "LOADING...";
        tmp.fontSize  = 20;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = new Color(0.7f, 0.85f, 1f, 1f);
        
        var textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = textRT.anchorMax = textRT.pivot = new Vector2(0.5f, 0.5f);
        textRT.sizeDelta        = new Vector2(300f, 40f);
        textRT.anchoredPosition = new Vector2(0f, -30f);

        // Hide structural layers immediately until called via LoadScene pipeline routine
        _overlayCanvas.gameObject.SetActive(false);
    }
}