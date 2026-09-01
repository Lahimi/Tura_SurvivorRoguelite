using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GymSceneButton : MonoBehaviour
{
    public TextMeshProUGUI label; // Drag the child Text (TMP) here
    public Button button;         // Drag the Button component here

    // This is called by the GymManager when the button is spawned
    public void Init(string displayName, System.Action onClickAction)
    {
        label.text = displayName;
        
        // Safety: Clear old listeners so clicking doesn't trigger 10 scenes at once
        button.onClick.RemoveAllListeners();
        
        // Add the new scene-loading action
        button.onClick.AddListener(() => onClickAction());
    }
}