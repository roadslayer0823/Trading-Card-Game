using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class FilterButtonPrefab : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private Image backgroundImage;

    private Color normalColor = Color.white;
    private Color selectedColor = Color.red;

    private bool isSelected = false;

    public void Setup(string text, Action onClickAction)
    {
        buttonText.text = text;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            ToggleSelect();
            onClickAction?.Invoke();
        });
    }

    public void ToggleSelect()
    {
        isSelected = !isSelected;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if(backgroundImage != null)
        {
            backgroundImage.color = isSelected  ? selectedColor : normalColor;
        }
    }

    public void ResetState()
    {
        isSelected = false;
        UpdateVisual();
    }
}
