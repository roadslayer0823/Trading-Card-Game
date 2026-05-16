using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DeckFilterPanel : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject filterCategoryPrefab;

    public void Initialize()
    {
        if(backgroundImage != null)
        {
            EventTrigger trigger = backgroundImage.gameObject.GetComponent<EventTrigger>();
            if (trigger == null) trigger = backgroundImage.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            entry.callback.AddListener((data) => { CloseFilterPanel(); });
            trigger.triggers.Add(entry);
        }

        ClearContent();
        CreateElementCategory();
        CreateCostCategory();
        CreateCardTypeCategory();

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
            category.AddButton(elem, () =>
            {
                Debug.Log($"Filter: Element = {elem}");
            });
        }
    }

    private void CreateCostCategory()
    {
        var category = SetupCategory("Cost");
        for(int i = 1; i <= 8; i++)
        {
            category.AddButton(i.ToString(), ()=>
            {
                Debug.Log($"Filter: Cost = {i}");
            });
        }
    }

    private void CreateCardTypeCategory()
    {
         var category = SetupCategory("Card Type");

         string[] types = { "Monster", "Spell"};

         foreach(string type in types)
         {
            category.AddButton(type, () =>
            {
                Debug.Log($"Filter: Card Type = {type}");
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
