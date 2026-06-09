using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;
    public System.Action OnCardPoolLoaded;

    public List<CardDataSO> cardPool = new();
    public List<CardDataSO> playerHand = new();
    public List<CardDataSO> enemyHand = new();

    private int maxDeckSize = 30;
    private int maxCopiesPerCard = 3;
    private int maxHandSize = 10;
    private const string DECKS_PREFS_KEY = "saved_decks";

    private Dictionary<string, int> currentDeck = new();
    private List<ModelDatas.SavedDeck> savedDecks = new List<ModelDatas.SavedDeck>();
    private List<CardDataSO> playerDeckList = new();
    private List<CardDataSO> enemyDeckList = new();
    private Dictionary<string, CardDataSO> cardLookup = new();

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
        Debug.Log("Loading cards via Addressables...");
        Addressables.LoadAssetsAsync<CardDataSO>("CardData", null).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                cardPool = new List<CardDataSO>(handle.Result);
                cardLookup.Clear();
                foreach (var card in cardPool)
                {
                    cardLookup[card.id] = card;
                }
                Debug.Log($"Successfully loaded {cardPool.Count} cards via Addressables.");
                OnCardPoolLoaded?.Invoke();
            }
            else
            {
                Debug.LogError("Failed to load cards via Addressables. Did you set the 'Cards' label on your ScriptableObjects?");
            }
        };
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
    public CardDataSO DrawOneCard(bool isPlayer)
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
        PlayerPrefs.SetString(DECKS_PREFS_KEY, json);
        PlayerPrefs.Save(); // Required on WebGL to flush to IndexedDB
        Debug.Log($"Saved {savedDecks.Count} decks to PlayerPrefs.");
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
        if (!PlayerPrefs.HasKey(DECKS_PREFS_KEY))
        {
            Debug.Log("No saved decks found, starting with empty list.");
            savedDecks = new List<ModelDatas.SavedDeck>();
            return;
        }

        string json = PlayerPrefs.GetString(DECKS_PREFS_KEY, "");
        savedDecks = JsonConvert.DeserializeObject<List<ModelDatas.SavedDeck>>(json) ?? new List<ModelDatas.SavedDeck>();
        Debug.Log($"Loaded {savedDecks.Count} decks from PlayerPrefs.");
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
    public void ClearCurrentDeck()
    {
        currentDeck.Clear();
    }
    public List<ModelDatas.SavedDeck> GetAllSavedDecks()
    {
        return savedDecks;
    }
}
