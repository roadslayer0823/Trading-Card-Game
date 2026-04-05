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
        FilterButtonPrefab btnPrefab = btnObj.GetComponent<FilterButtonPrefab>();

        if(btnObj != null)
        {
            btnPrefab.Setup(buttonText, onClick);
        }
        return btnPrefab;
    }
}
