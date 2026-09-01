using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; 
using System.Collections.Generic;

public class GymManager : MonoBehaviour
{
    // Inside your GymManager.cs file:
    [Header("Registry Reference")]
    public S_GymRegistry registry; // 🔧 Updated to new type reference!

    [Header("UI References")]
    public GameObject buttonPrefab; // Your button template
    public Transform contentPanel;  // Viewport > Content inside the Scroll View

    void Start()
    {
        // Safety check to make sure you assigned the registry in the Inspector
        if (registry != null)
        {
            PopulateList();
        }
        else
        {
            Debug.LogError("GymManager is missing the Registry Asset! Drag the GymRegistry asset into the Inspector.");
        }
    }

    void PopulateList()
    {
        foreach (GymEntry entry in registry.scenes)
        {
            // Spawn the button prefab
            GameObject newButtonObj = Instantiate(buttonPrefab, contentPanel);
            
            // Get our custom script from the prefab
            GymSceneButton sceneButton = newButtonObj.GetComponent<GymSceneButton>();

            if (sceneButton != null)
            {
                // Use the Init method! 
                // We pass the name and a "lambda" function that tells it what to do.
                sceneButton.Init(entry.displayName, () => LoadGymScene(entry.scenePath));
            }
            else
            {
                Debug.LogError("The Button Prefab is missing the GymSceneButton script!");
            }
        }
    }

    void LoadGymScene(string sceneName)
    {
        Debug.Log("Loading: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }
}