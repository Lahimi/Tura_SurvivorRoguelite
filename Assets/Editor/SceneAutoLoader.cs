using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

// 🔧 Functionality Contract: Editor-only tooling. Lives exclusively in Assets/Editor/
// Injects SC_BootStrap before Play Mode so singletons are always initialized,
// regardless of which workspace scene the developer currently has open.
[InitializeOnLoad]
public static class SceneAutoLoader
{
    // 🔧 Path updated to match your exact capitalization layout: SC_BootStrap.unity
    const string BOOTSTRAP_PATH  = "Assets/_04_Scenes/SC_BootStrap.unity";
    const string PREFS_KEY       = "SceneAutoLoader.AddedBootstrap";

    // Static constructor — Runs automatically when Unity instances start or scripts recompile.
    static SceneAutoLoader()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        switch (state)
        {
            case PlayModeStateChange.ExitingEditMode:
                // Developer pressed Play. If Bootstrap is already loaded manually, skip injection.
                if (IsBootstrapLoaded()) return;

                // Bootstrap is missing — inject it additively before runtime loops lock in.
                // The open test scene stays open and remains the primary active context.
                // Record that this tool performed the injection so it cleans up later.
                EditorPrefs.SetBool(PREFS_KEY, true);
                EditorSceneManager.OpenScene(BOOTSTRAP_PATH, OpenSceneMode.Additive);
                break;

            case PlayModeStateChange.EnteredEditMode:
                // Play mode terminated. Only clean up Bootstrap if we were the ones who injected it.
                if (!EditorPrefs.GetBool(PREFS_KEY, false)) return;

                EditorPrefs.DeleteKey(PREFS_KEY);
                var bootstrap = SceneManager.GetSceneByPath(BOOTSTRAP_PATH);
                
                if (bootstrap.IsValid() && bootstrap.isLoaded)
                {
                    EditorSceneManager.CloseScene(bootstrap, true);
                }
                break;
        }
    }

    private static bool IsBootstrapLoaded()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).path == BOOTSTRAP_PATH)
                return true;
        }
        return false;
    }
}