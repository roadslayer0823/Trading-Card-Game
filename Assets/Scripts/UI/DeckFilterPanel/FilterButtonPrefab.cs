using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class FilterButtonPrefab : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text buttonText;

    public void Setup(string text, Action onClickAction)
    {
        buttonText.text = text;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClickAction?.Invoke());
    }

    public void SetSelected(bool isSelected)
    {
        button.interactable = !isSelected;
    }
}
