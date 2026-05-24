using UnityEngine;
using System;
using TMPro;

public class FilterCategoryPrefab : MonoBehaviour
{
    [SerializeField] private TMP_Text categoryTitle;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private GameObject filterButtonPrefab;

    public void Setup(string title)
    {
        categoryTitle.text = title;
    }

    public FilterButtonPrefab AddButton(string buttonText, Action onClick)
    {
        GameObject btnObj = Instantiate(filterButtonPrefab, buttonContainer);
        FilterButtonPrefab btn = btnObj.GetComponent<FilterButtonPrefab>();

        if(btn != null)
        {
            btn.Setup(buttonText, onClick);
        }
        return btn;
    }
}
