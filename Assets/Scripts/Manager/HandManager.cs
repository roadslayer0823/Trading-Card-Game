using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance;

    [Header("Hand UI")]
    public Transform playerHandZone;
    public Transform enemyHandZone;
    public GameObject playerCardPrefab;
    public GameObject enemyCardPrefab;

    private List<GameObject> playerCardObjects = new();
    private List<GameObject> enemyCardObjects = new();

    private void Awake()
    {
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RefreshHandUI(bool isPlayer)
    {
        List<ModelDatas.CardData> hand = isPlayer ? DeckManager.Instance.playerHand : DeckManager.Instance.enemyHand;
        Transform handZone = isPlayer ? playerHandZone : enemyHandZone;
        List<GameObject> cardObjects = isPlayer ? playerCardObjects : enemyCardObjects;
        GameObject prefab = isPlayer ? playerCardPrefab : enemyCardPrefab;

        // Clear existing UI
        foreach (var obj in cardObjects) Destroy(obj);
        cardObjects.Clear();

        // Rebuild UI
        foreach (var card in hand)
        {
            GameObject cardObj = Instantiate(prefab, handZone);
            CardDisplay cardDisplay = cardObj.GetComponent<CardDisplay>();
            cardDisplay.SetCard(card);
            cardDisplay.currentZone = CardZone.Hand;
            cardDisplay.owner = isPlayer ? Owner.Player : Owner.Enemy;

            if (isPlayer)
            {
                cardObj.AddComponent<BattleCardDragHandler>();
                cardDisplay.cardCountText.gameObject.SetActive(false);
                bool isMonster = card.type == "Monster";
                cardDisplay.stateArea.gameObject.SetActive(isMonster);
                cardDisplay.SetupCardUI(card);
            }

            cardObjects.Add(cardObj);
        }
    }

    public void DrawCard(ModelDatas.CardData card, bool isPlayer)
    {
        if (card == null) return;

        Transform handZone = isPlayer ? playerHandZone : enemyHandZone;
        List<GameObject> cardObjects = isPlayer ? playerCardObjects : enemyCardObjects;
        GameObject prefab = isPlayer ? playerCardPrefab : enemyCardPrefab;

        GameObject cardObj = Instantiate(prefab, handZone);
        CardDisplay cardDisplay = cardObj.GetComponent<CardDisplay>();
        cardDisplay.SetCard(card);
        cardDisplay.currentZone = CardZone.Hand;
        cardDisplay.owner = isPlayer ? Owner.Player : Owner.Enemy;

        if (isPlayer)
        {
            cardObj.AddComponent<BattleCardDragHandler>();
            cardDisplay.cardCountText.gameObject.SetActive(false);
            bool isMonster = card.type == "Monster";
            cardDisplay.stateArea.gameObject.SetActive(isMonster);
            cardDisplay.SetupCardUI(card);
        }

        cardObjects.Add(cardObj);
    }
}
