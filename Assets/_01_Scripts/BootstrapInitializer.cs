using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Attach this to your initializer GameObject inside SC00_Bootstrap
public class BootstrapInitializer : MonoBehaviour
{
    [Header("🔧 Functional Routing")]
    [Tooltip("The exact name of your main menu scene.")]
    public string mainMenuSceneName = "SC_MainMenu";

    IEnumerator Start()
    {
        // 🔧 Editor Workflow Check:
        // If sceneCount > 1, it means the autoloader injected this bootstrap scene 
        // alongside a specific GYM scene you are trying to test. 
        // We halt here so the main menu doesn't override your test scene!
        if (SceneManager.sceneCount > 1)
        {
            Debug.Log("[Bootstrap] Multiple scenes detected (Editor Test Mode). Stopping menu load.");
            yield break;
        }

        // 🔧 Normal Game Start:
        // Bootstrap is the only scene open. Load the Main Menu ADDITIVELY 
        // so SC00_Bootstrap and its singletons stay alive underneath it.
        Debug.Log("[Bootstrap] Global managers initialized. Loading Main Menu additively...");
        
        var op = SceneManager.LoadSceneAsync(mainMenuSceneName, LoadSceneMode.Additive);
        yield return op;

        // Swap the active scene focus to the Main Menu so lights and skyboxes look correct
        var mainMenu = SceneManager.GetSceneByName(mainMenuSceneName);
        if (mainMenu.IsValid())
        {
            SceneManager.SetActiveScene(mainMenu);
        }
    }
}