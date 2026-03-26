using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;
    public List<ModelDatas.CardData> cardPool = new();
    public List<ModelDatas.CardData> playerHand = new();
    public List<ModelDatas.CardData> enemyHand = new();

    private int maxDeckSize = 30;
    private int maxCopiesPerCard = 3;
    private int maxHandSize = 10;
    private string decksSavePath => Path.Combine(Application.persistentDataPath, "saved_decks.json");

    private Dictionary<string, int> currentDeck = new();
    private List<ModelDatas.SavedDeck> savedDecks = new List<ModelDatas.SavedDeck>();
    private List<ModelDatas.CardData> playerDeckList = new();
    private List<ModelDatas.CardData> enemyDeckList = new();
    private Dictionary<string, ModelDatas.CardData> cardLookup = new();

    public void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        LoadAllDecks();
        DontDestroyOnLoad(gameObject);
    }

    public void Initialize()
    {
        LoadCardPool();
    }

    private void LoadCardPool()
    {
        string path = Path.Combine(Application.streamingAssetsPath, Terminology.CARDS_JSON_NAME);
        if (!File.Exists(path))
        {
            Debug.LogError("Card JSON not found: " + path);
            return;
        }

        string json = File.ReadAllText(path);
        cardPool = JsonConvert.DeserializeObject<List<ModelDatas.CardData>>(json);

        cardLookup.Clear();
        foreach(var card in cardPool)
        {
            cardLookup[card.id] = card;
        }

        Debug.Log($"Loaded {cardPool.Count} cards into pool.");
        Debug.Log(Application.persistentDataPath + Terminology.CARDS_JSON_NAME);
    }

    public void GeneratePlayerDeck()
    {
        playerDeckList.Clear();
        foreach (var card in currentDeck)
        {
            if (!cardLookup.ContainsKey(card.Key)) continue;
            for (int i = 0; i < card.Value; i++)
            {
                playerDeckList.Add(cardLookup[card.Key]);
            }
        }
        Debug.Log($"[DEBUG] deckList Count: {playerDeckList.Count}, currentDeck Count: {currentDeck.Count}");
    }

    public void GenerateEnemyDeck() 
    {
        enemyDeckList.Clear();
        Dictionary<string, int> enemyCardCount = new Dictionary<string, int>();

        while(enemyDeckList.Count < maxDeckSize)
        {
            int index = Random.Range(0, cardPool.Count);
            var randomCard = cardPool[index];
            if (!enemyCardCount.ContainsKey(randomCard.id)) 
            {
                enemyCardCount[randomCard.id] = 0;
            }
               
            if (enemyCardCount[randomCard.id] < maxCopiesPerCard)
            {
                enemyDeckList.Add(randomCard);
                enemyCardCount[randomCard.id]++;
            }
        }
        Debug.Log($"Generated enemy deck with {enemyDeckList.Count} cards (max {maxCopiesPerCard} copies per card).");
    }

    public void DrawStartHand(int count, bool isPlayer)
    {
        var hand = isPlayer ? playerHand : enemyHand;
        var deck = isPlayer ? playerDeckList : enemyDeckList;

        hand.Clear();

        for(int i = 0; i < count && deck.Count > 0; i++)
        {
            int index = Random.Range(0, deck.Count);
            hand.Add(deck[index]);
            deck.RemoveAt(index);
        }

        Debug.Log($"Drew {hand.Count} cards as starting hand:");

        foreach(var card in hand)
        {
            Debug.Log($"- {card.cardName} [{card.element}]");
        }
    }
    public ModelDatas.CardData DrawOneCard(bool isPlayer)
    {
        var hand = isPlayer ? playerHand : enemyHand;
        var deck = isPlayer ? playerDeckList : enemyDeckList;
        string owner = isPlayer ? "Player" : "Enemy";

        Debug.Log($"[{owner}] Attempting to draw a card...");
        Debug.Log($"[{owner}] Current hand count: {hand.Count}, deck count: {deck.Count}");

        if (deck.Count == 0) 
        {
            Debug.LogWarning($"[{owner}] Deck is empty, cannot draw!");
            return null;
        }
        if (hand.Count >= maxHandSize) 
        {
            Debug.LogWarning($"[{owner}] Hand is full, cannot draw!");
            return null;
        }

        int index = Random.Range(0, deck.Count);
        var card = deck[index];

        deck.RemoveAt(index);
        hand.Add(card);

        Debug.Log($"[{owner}] Drew card: {card.cardName}");
        Debug.Log($"[{owner}] Deck now has {deck.Count} cards left.");
        Debug.Log($"[{owner}] Hand now has {hand.Count} cards.");

        return card;
    }

    //return value
    public bool IsAddCardToDeck(string cardID)
    {
        if (GetDeckCardCount() >= maxDeckSize) return false;

        if(currentDeck.TryGetValue(cardID, out int count) && count >= maxCopiesPerCard)
        {
            return false;
        }

        if (!currentDeck.ContainsKey(cardID)) 
        {
            currentDeck[cardID] = 1;
        }
        else
        {
            currentDeck[cardID]++;
        }
        Debug.Log($"Added {cardID} to deck.");
        return true;
    }
    public bool RemoveCardFromDeck(string cardID)
    {
        if (!currentDeck.ContainsKey(cardID)) return false;

        currentDeck[cardID]--;
        if (currentDeck[cardID] <= 0) currentDeck.Remove(cardID);

        return true;
    }

    public int GetDeckCardCount()
    {
        int total = 0;
        foreach (var kvp in currentDeck)
        {
            total += kvp.Value;
        }
        return total;
    }

    //save and load deck
    public void SaveAllDecks()
    {
        string json = JsonConvert.SerializeObject(savedDecks, Formatting.Indented);
        File.WriteAllText(decksSavePath, json);
        Debug.Log($"Saved {savedDecks.Count} decks to {decksSavePath}");
    }

    public void SaveCurrentDeckAs(string deckName, string description = "", string coverCardID = "")
    {
        ModelDatas.SavedDeck existDeck = savedDecks.Find(d => d.deckName == deckName);
        if(existDeck != null)
        {
            existDeck.cards = new Dictionary<string, int>(currentDeck);
            existDeck.description = description;
            existDeck.coverCardID = coverCardID;
            Debug.Log($"Overwrote existing deck: {deckName}");
        }
        else
        {
            ModelDatas.SavedDeck newDeck = new ModelDatas.SavedDeck
            {
                deckName = deckName,
                cards = new Dictionary<string, int>(currentDeck),
                description = description,
                coverCardID = coverCardID
            };
            savedDecks.Add(newDeck);
            Debug.Log($"Created new deck: {deckName}");
        }
        SaveAllDecks();
    }

    public void LoadAllDecks()
    {
        if (!File.Exists(decksSavePath))
        {
            Debug.Log("No saved decks found, starting with empty list.");
            savedDecks = new List<ModelDatas.SavedDeck>();
            return;
        }

        string json = File.ReadAllText(decksSavePath);
        savedDecks = JsonConvert.DeserializeObject<List<ModelDatas.SavedDeck>>(json) ?? new List<ModelDatas.SavedDeck>();
        Debug.Log($"Loaded {savedDecks.Count} decks from {decksSavePath}");
    }

    public bool LoadDeckByName(string deckName)
    {
        ModelDatas.SavedDeck selected = savedDecks.Find(d => d.deckName == deckName);
        if(selected == null)
        {
            Debug.LogWarning($"Deck not found: {deckName}");
            return false;
        }

        currentDeck = new Dictionary<string, int>(selected.cards);
        GeneratePlayerDeck();
        return true;
    }

    public bool DeleteDeckByName(string deckName)
    {
        ModelDatas.SavedDeck deckToDelete = savedDecks.Find(d => d.deckName == deckName);
        if (deckToDelete == null) return false;

        savedDecks.Remove(deckToDelete);
        SaveAllDecks();
        return true;
    }

    public Dictionary<string, int> GetCurrentDeck() => currentDeck;
    public List<ModelDatas.SavedDeck> GetAllSavedDecks()
    {
        return savedDecks;
    }
}
