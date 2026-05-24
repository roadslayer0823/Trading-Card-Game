using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum PanelType {None, Library, Deck}
public class DeckBuilderManager : MonoBehaviour
{
    public static DeckBuilderManager Instance;
    
    [Header("UI References")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private GameObject filterPanel;
    [SerializeField] private Transform libraryGridParent;
    [SerializeField] private Transform deckGridParent;
    [SerializeField] private TMP_Text deckCountText;
    [SerializeField] private Button saveDeckButton;
    [SerializeField] private Button filterPanelButton;
    [SerializeField] private TMP_InputField deckNameInput;

    [HideInInspector] public int currentDeckCount = 0;
    [HideInInspector] public int maxDeckCount = 30;

    private Dictionary<string, CardDisplay> libraryCards = new();
    private Dictionary<string, CardDisplay> deckCards = new();
    private List<CardDataSO> cardDataList;

    //filter panel function
    private HashSet<string> activeElementFilters = new HashSet<string>();
    private HashSet<int> activeCostFilters = new HashSet<int>();
    private HashSet<string> activeTypeFilters = new HashSet<string>();
    
    public void Initialize() 
    {
        Instance = this;

        saveDeckButton.onClick.AddListener(() =>
        {
            OnSaveClicked();
        });

        filterPanelButton.onClick.AddListener(() =>
        {
            OpenFilterPanel(); 
        });

        if (DeckManager.Instance.cardPool != null && DeckManager.Instance.cardPool.Count > 0)
        {
            LoadDataAndSpawn();
        }
        else
        {
            DeckManager.Instance.OnCardPoolLoaded += LoadDataAndSpawn;
        }
    }

    private void LoadDataAndSpawn()
    {
        cardDataList = DeckManager.Instance.cardPool;
        RefreshCardList();
        UpdateDeckCountUI();
    }

    private void OnSaveClicked()
    {
        string deckName = deckNameInput.text.Trim();

        if (string.IsNullOrEmpty(deckName))
        {
            return;
        }

        int count = currentDeckCount;
        if (count == 0)
        {
            Debug.LogWarning("卡組為空，無法儲存");
            return;
        }

        if (count < maxDeckCount)
        {
            Debug.LogWarning($"卡組只有 {count}/{maxDeckCount} 張，是否仍要儲存？");
        }
        DeckManager.Instance.SaveCurrentDeckAs(deckName);
        DeckSelectionManager.Instance.RefreshDeckList();
        this.gameObject.SetActive(false);
        Debug.Log($"卡組已儲存：{deckName}");
    }

    private void OpenFilterPanel()
    {
        filterPanel.SetActive(true);
    }

    public void RefreshCardList()
    {
        foreach(Transform child in libraryGridParent)
        {
            Destroy(child.gameObject);
        }
        libraryCards.Clear();

        var currentDeckDict = DeckManager.Instance.GetCurrentDeck();

        foreach(CardDataSO data in cardDataList)
        {
            bool match = true;

            if(activeElementFilters.Count > 0)
            {
                match &= activeElementFilters.Contains(data.element);
            }

            if(activeCostFilters.Count > 0)
            {
                match &= activeCostFilters.Contains(data.cost);    
            }

            if(activeTypeFilters.Count > 0)
            {
                match &= activeTypeFilters.Contains(data.type);
            }

            if(!match) continue;

            int usedCount = 0;
            if (currentDeckDict != null && currentDeckDict.ContainsKey(data.id))
            {
                usedCount = currentDeckDict[data.id];
            }

            int remainingCount = data.cardCount - usedCount;
            if (remainingCount > 0)
            {
                SpawnCard(data, remainingCount);
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(libraryGridParent.GetComponent<RectTransform>());
    }

    private void SpawnCard(CardDataSO data, int overrideCount = -1)
    {
        int countToUse = overrideCount >= 0 ? overrideCount : data.cardCount;

        GameObject prefab = Instantiate(cardPrefab, libraryGridParent);
        prefab.AddComponent<CardDragHandler>();
        prefab.transform.localScale = Vector3.one;

        CardDisplay display = prefab.GetComponent<CardDisplay>();

        if(display != null)
        {
            display.SetCard(data, countToUse, PanelType.Library);
            display.SetupCardUI(data);
            libraryCards[data.id] = display;
        }
        else
        {
            Debug.Log("lose card display script");
        }
        if (display.cardType == "Spell")
        {
            prefab.transform.Find("Container").gameObject.transform.Find("StateArea").gameObject.SetActive(false);
        }
    }

    private void UpdateDeckCountUI()
    {
        if (deckCountText != null) 
        {
            deckCountText.text = $"Deck: {currentDeckCount}/{maxDeckCount}";
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(deckCountText.GetComponentInParent<RectTransform>());
    }

    public void TransferCard(string cardID, PanelType from, PanelType to)
    {
        if (from == to) return;

        Dictionary<string, CardDisplay> fromDict = from == PanelType.Library ? libraryCards : deckCards;
        Dictionary<string, CardDisplay> toDict = to == PanelType.Library ? libraryCards : deckCards;
        Transform toParent = to == PanelType.Library ? libraryGridParent : deckGridParent;

        if (!fromDict.ContainsKey(cardID)) return;

        CardDisplay fromCard = fromDict[cardID];

        bool success = false;

        if(to == PanelType.Deck)
        {
            success = DeckManager.Instance.IsAddCardToDeck(cardID);
            if (success) currentDeckCount++;
            else
            {
                Debug.LogWarning($"無法加入卡片 {cardID}：已達上限或牌庫滿");
                return;
            }
        }
        else if(from == PanelType.Deck)
        {
            success = DeckManager.Instance.RemoveCardFromDeck(cardID);
            if (success) currentDeckCount--;
        }


        fromCard.UpdateCount(fromCard.currentCount - 1);
        if(fromCard.currentCount <= 0)
        {
            Destroy(fromCard.gameObject);
            fromDict.Remove(cardID);
        }

        if (toDict.ContainsKey(cardID))
        {
            toDict[cardID].UpdateCount(toDict[cardID].currentCount + 1);
        }
        else
        {
            CardDataSO data = cardDataList.Find(c => c.id == cardID);
            if(data != null)
            {
                GameObject newCard = Instantiate(cardPrefab, toParent);
                newCard.AddComponent<CardDragHandler>();
                CardDisplay display = newCard.GetComponent<CardDisplay>();
                display.SetCard(data, 1, to);
                display.SetupCardUI(data);
                toDict.Add(cardID, display);
            }
        }

        UpdateDeckCountUI();
        LayoutRebuilder.ForceRebuildLayoutImmediate(toParent.GetComponent<RectTransform>());
    }

    public void LoadDeckForEdit(string deckName)
    {
        if (DeckManager.Instance.LoadDeckByName(deckName))
        {
            deckNameInput.text = deckName;

            foreach (Transform child in deckGridParent) Destroy(child.gameObject);
            deckCards.Clear();
            currentDeckCount = 0;

            var currentDeckDict = DeckManager.Instance.GetCurrentDeck();

            foreach(var kvp in currentDeckDict)
            {
                string id = kvp.Key;
                int count = kvp.Value;

                CardDataSO data = cardDataList.Find(c => c.id == id);
                if (data == null) continue;

                GameObject cardObj = Instantiate(cardPrefab, deckGridParent);
                cardObj.AddComponent<CardDragHandler>();
                CardDisplay display = cardObj.GetComponent<CardDisplay>();
                display.SetCard(data, count, PanelType.Deck);
                display.SetupCardUI(data);
                deckCards.Add(id, display);

                currentDeckCount += count;
            }
            RefreshCardList();
            UpdateDeckCountUI();
        }
    }

    public void CreateNewDeck()
    {
        DeckManager.Instance.ClearCurrentDeck();
        deckNameInput.text = "";

        foreach (Transform child in deckGridParent) Destroy(child.gameObject);
        deckCards.Clear();
        currentDeckCount = 0;

        RefreshCardList();
        UpdateDeckCountUI();
    }

    //filter card function
    public void ToggleElementFilter(string element)
    {
        if(activeElementFilters.Contains(element))
        activeElementFilters.Remove(element);
        else
        activeElementFilters.Add(element);

        RefreshCardList();
    }

    public void ToggleCostFilter(int cost)
    {
        if(activeCostFilters.Contains(cost))
        activeCostFilters.Remove(cost);
        else
        activeCostFilters.Add(cost);

        RefreshCardList();
    }

    public void ToggleCardTypeFilter(string type)
    {
        if(activeTypeFilters.Contains(type))
        activeTypeFilters.Remove(type);
        else
        activeTypeFilters.Add(type);

        RefreshCardList();
    }

    public void ClearAllFilters()
    {
        activeElementFilters.Clear();
        activeCostFilters.Clear();
        activeTypeFilters.Clear();
        RefreshCardList();
    }
}
