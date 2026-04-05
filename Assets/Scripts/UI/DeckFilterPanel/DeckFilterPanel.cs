using UnityEngine;
using UnityEngine.UI;

public class DeckFilterPanel : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private GameObject filterCategoryPrefab;

    private DeckBuilderManager deckBuilder;

    public void Initialize()
    {
        deckBuilder = DeckBuilderManager.Instance;
        if (deckBuilder == null) return;
        
        ClearContent();
        CreateElementCategory();
        CreateCostCategory();
        CreateCardTypeCategory();

         LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
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
}
