using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckSelectionManager : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject deckItemPrefab;
    public TMP_Text selectedDeckText;
    public Button startButton;
    public Button backButton;
    public Transform deckContent;


    private string selectedDeckName = null;

    private void Start()
    {
        RefreshDeckList();
    }

    private void OnEnable()
    {
        startButton.interactable = false;
        selectedDeckText.text = "Select a Deck";
        backButton.onClick.AddListener(OnBackClicked);
    }

    public void RefreshDeckList()
    {
        foreach (Transform child in deckContent) Destroy(child.gameObject);

        var decks = DeckManager.Instance.GetAllSavedDecks();
        if(decks.Count == 0)
        {
            selectedDeckText.text = "You don't have a deck, please create it";
            return;
        }

        foreach(var deck in decks)
        {
            GameObject item = Instantiate(deckItemPrefab, deckContent);
            DeckItemUI itemUI = item.GetComponent<DeckItemUI>();
            itemUI.Setup(deck.deckName);
            itemUI.selectButton.onClick.AddListener(() => OnDeckItemClicked(deck.deckName, item));
        }
    }

    private void OnDeckItemClicked(string deckName, GameObject clickedItem)
    {
        foreach(Transform child in deckContent)
        {
            child.GetComponent<Image>().color = Color.white;
        }
        clickedItem.GetComponent<Image>().color = new Color(1f, 0.9f, 0.6f);

        selectedDeckName = deckName;
        selectedDeckText.text = $"selected deck: {deckName}";
        startButton.interactable = true;
        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(() => StartGameWithDeck(deckName));
    }

    private void StartGameWithDeck(string deckName)
    {
        DeckManager.Instance.LoadDeckByName(deckName);
        UnityEngine.SceneManagement.SceneManager.LoadScene("BattleScene");
    }

    private void OnBackClicked()
    {
        gameObject.SetActive(false);
    }

}
