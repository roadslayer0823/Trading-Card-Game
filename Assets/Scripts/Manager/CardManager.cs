using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using static ModelDatas;
using TMPro;

public enum PanelType {None, Library, Deck}
public class CardManager : MonoBehaviour
{
    public static CardManager Instance;
    
    [Header("References")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform libraryGridParent;
    [SerializeField] private Transform deckGridParent;
    [SerializeField] private TMP_Text deckCountText;
    [SerializeField] private Button saveDeckButton;

    [HideInInspector] public int currentDeckCount = 0;
    [HideInInspector] public int maxDeckCount = 30;

    private Dictionary<string, CardDisplay> libraryCards = new();
    private Dictionary<string, CardDisplay> deckCards = new();
    private List<CardData> cardDataList;
    private void Awake()
    {
        Instance = this;
    }
    public void Initialize() 
    {
        LoadCardDataFromJson();
        SpawnAllCards();
        UpdateDeckCountUI();

        saveDeckButton.onClick.AddListener(() =>
        {
            DeckManager.Instance.SaveDeck();
        }); 
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

        LayoutRebuilder.ForceRebuildLayoutImmediate(deckCountText.GetComponent<RectTransform>());
    }

    public void TransferCard(string cardID, PanelType from, PanelType to)
    {
        if (from == to) return;

        Dictionary<string, CardDisplay> fromDict = from == PanelType.Library ? libraryCards : deckCards;
        Dictionary<string, CardDisplay> toDict = to == PanelType.Library ? libraryCards : deckCards;
        Transform toParent = to == PanelType.Library ? libraryGridParent : deckGridParent;

        if (!fromDict.ContainsKey(cardID)) return;

        CardDisplay fromCard = fromDict[cardID];

        if (from == PanelType.Deck) currentDeckCount--;
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
            GameObject newCard = Instantiate(cardPrefab, toParent);
            CardDisplay display = newCard.GetComponent<CardDisplay>();
            display.SetCard(data, 1, to);
            display.SetupCardUI(data);
            toDict.Add(cardID, display);
        }

        if (to == PanelType.Deck) 
        {
            currentDeckCount++;
            DeckManager.Instance.IsAddCardToDeck(cardID);
        }
        else if(from == PanelType.Deck)
        {
            DeckManager.Instance.RemoveCardFromDeck(cardID);
        }

        UpdateDeckCountUI();
        LayoutRebuilder.ForceRebuildLayoutImmediate(toParent.GetComponent<RectTransform>());
    }
}
