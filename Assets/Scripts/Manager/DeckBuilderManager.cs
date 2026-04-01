using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using static ModelDatas;
using TMPro;

public enum PanelType {None, Library, Deck}
public class DeckBuilderManager : MonoBehaviour
{
    public static DeckBuilderManager Instance;
    
    [Header("UI References")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform libraryGridParent;
    [SerializeField] private Transform deckGridParent;
    [SerializeField] private TMP_Text deckCountText;
    [SerializeField] private Button saveDeckButton;
    [SerializeField] private TMP_InputField deckNameInput;

    [HideInInspector] public int currentDeckCount = 0;
    [HideInInspector] public int maxDeckCount = 30;

    private Dictionary<string, CardDisplay> libraryCards = new();
    private Dictionary<string, CardDisplay> deckCards = new();
    private List<CardData> cardDataList;
    
    public void Initialize() 
    {
        LoadCardDataFromJson();
        SpawnAllCards();
        UpdateDeckCountUI();

        saveDeckButton.onClick.AddListener(() =>
        {
            OnSaveClicked();
        });

        Instance = this;
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
            // 可以加 UI 確認彈窗，暫時直接存
        }
        DeckManager.Instance.SaveCurrentDeckAs(deckName);
        DeckSelectionManager.Instance.RefreshDeckList();
        this.gameObject.SetActive(false);
        Debug.Log($"卡組已儲存：{deckName}");
    }

    private void LoadCardDataFromJson()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "cards.json");
        if (!File.Exists(path))
        {
            return;
        }

        string json = File.ReadAllText(path);
        CardData[] cardArray = JsonHelper.FromJson<CardData>(json);
        cardDataList = new List<CardData>(cardArray);
    }

    public void SpawnAllCards()
    {
        foreach(Transform child in libraryGridParent)
        {
            Destroy(child.gameObject);
        }

        foreach(CardData data in cardDataList)
        {
            SpawnCard(data);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(libraryGridParent.GetComponent<RectTransform>());
    }

    private void SpawnCard(CardData data)
    {
        GameObject prefab = Instantiate(cardPrefab, libraryGridParent);
        prefab.AddComponent<CardDragHandler>();
        prefab.transform.localScale = Vector3.one;

        CardDisplay display = prefab.GetComponent<CardDisplay>();

        if(display != null)
        {
            display.SetCard(data, data.cardCount, PanelType.Library);
            display.SetupCardUI(data);
            libraryCards.Add(data.id, display);
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
            CardData data = cardDataList.Find(c => c.id == cardID);
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

                CardData data = cardDataList.Find(c => c.id == id);
                if (data == null) continue;

                GameObject cardObj = Instantiate(cardPrefab, deckGridParent);
                cardObj.AddComponent<CardDragHandler>();
                CardDisplay display = cardObj.GetComponent<CardDisplay>();
                display.SetCard(data, count, PanelType.Deck);
                display.SetupCardUI(data);
                deckCards.Add(id, display);

                currentDeckCount += count;
            }
            RefreshLibraryForEdit();
            UpdateDeckCountUI();
        }
    }

    private void RefreshLibraryForEdit()
    {
        foreach(Transform child in libraryGridParent)
        {
            Destroy(child.gameObject);
        }
        libraryCards.Clear();

        var currentDeckDict = DeckManager.Instance.GetCurrentDeck();

        foreach (CardData data in cardDataList)
        {
            // Only spawn cards that have NOT been added to the current deck
            if (!currentDeckDict.ContainsKey(data.id))
            {
                SpawnCard(data);   // uses full data.cardCount as before
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(libraryGridParent.GetComponent<RectTransform>());
    }
}
