using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class CharacterCard : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] private Image background;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameLabel;

    public CharacterData Data { get; private set; }
    public Action OnClick;

    static readonly Color SELECTED   = new Color(1f, 1f, 1f, 1f);
    static readonly Color UNSELECTED = new Color(0.5f, 0.5f, 0.5f, 0.6f);

    public void Init(CharacterData data)
    {
        Data = data;
        nameLabel.text = data.displayName;

        if (ColorUtility.TryParseHtmlString(data.colorHex, out Color charColor))
        {
            icon.color = charColor;
        }

        SetSelected(false);
    }

    public void SetSelected(bool isSelected)
    {
        if (background != null)
            background.color = isSelected ? SELECTED : UNSELECTED;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick?.Invoke();
    }
}