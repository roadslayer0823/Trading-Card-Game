using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class DeckFilterPanel : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject filterCategoryPrefab;
    [SerializeField] private GameObject clearAllButtonPrefab;

    private DeckBuilderManager deckBuilder;
    private List<FilterButtonPrefab> allFilterButtons = new List<FilterButtonPrefab>(); 

    public void Initialize()
    {
        deckBuilder = DeckBuilderManager.Instance;
        if(deckBuilder == null) return;

        if(backgroundImage != null)
        {
            EventTrigger trigger = backgroundImage.gameObject.GetComponent<EventTrigger>();
            if (trigger == null) trigger = backgroundImage.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            entry.callback.AddListener((data) => { CloseFilterPanel(); });
            trigger.triggers.Add(entry);
        }

        ClearContent();
        allFilterButtons.Clear();

        CreateElementCategory();
        CreateCostCategory();
        CreateCardTypeCategory();
        CreateClearAllButton();

        RefreshTotalContentHeight();
    }
    private void ClearContent()
    {
        foreach(Transform child in content)
        {
            Destroy(child.gameObject);
        }
    }

    private void CreateElementCategory()
    {
        var category = SetupCategory("Element");

        string[] elements = { "Fire", "Water", "Earth", "Wind", "Light", "Dark" };

        foreach(string elem in elements)
        {
            string elementName = elem;
            var btn = category.AddButton(elem, () =>
            {
                deckBuilder.ToggleElementFilter(elementName);
            });
            if(btn != null) allFilterButtons.Add(btn);
        }
    }

    private void CreateCostCategory()
    {
        var category = SetupCategory("Cost");
        for(int i = 1; i <= 8; i++)
        {
            int cost = i;
            var btn = category.AddButton(i.ToString(), ()=>
            {
                deckBuilder.ToggleCostFilter(cost);
            });
            if(btn != null) allFilterButtons.Add(btn);
        }
    }

    private void CreateCardTypeCategory()
    {
         var category = SetupCategory("Card Type");

         string[] types = { "Monster", "Spell"};

         foreach(string type in types)
         {
            string cardType = type;
            var btn = category.AddButton(type, () =>
            {
                deckBuilder.ToggleCardTypeFilter(cardType);
            });
            if(btn != null) allFilterButtons.Add(btn);
         }
    }

    private void CreateClearAllButton()
    {
        if(clearAllButtonPrefab == null) return;
        
        GameObject obj = Instantiate(clearAllButtonPrefab, content);
        Button btn = obj.GetComponentInChildren<Button>();
        if(btn != null)
        {
            btn.onClick.AddListener(() =>
            {
                deckBuilder.ClearAllFilters();
                foreach(var btn in allFilterButtons)
                {
                    btn.ResetState();
                }
            });
        }
    }

    private FilterCategoryPrefab SetupCategory(string categoryName)
    {
        GameObject obj = Instantiate(filterCategoryPrefab, content);
        FilterCategoryPrefab category = obj.GetComponent<FilterCategoryPrefab>();
        category.Setup(categoryName);
        return category;
    }

    private void RefreshTotalContentHeight()
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());

        float totalHeight = 0f;

        foreach(Transform child in content)
        {
            RectTransform categoryRect = child.GetComponent<RectTransform>();
            if(categoryRect != null)
            {
                totalHeight += categoryRect.rect.height + 60;
                Debug.Log("totalHeight: " + totalHeight);
            }
        }

        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalHeight);
    }

    private void CloseFilterPanel()
    {
        this.gameObject.SetActive(false);
    }
}
