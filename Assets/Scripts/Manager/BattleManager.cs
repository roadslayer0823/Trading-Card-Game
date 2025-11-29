using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;

public enum TurnState
{
    Player,
    Enemy
}

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    [Header("Player UI Reference")]
    public TMP_Text enemyCurrentBehaviour;
    public Transform playerHandZone;
    public Transform playerFieldZone;
    public GameObject cardPrefab;
    public HealthPointHandler playerHealth;

    [Header("Enemy UI Reference")]
    public Transform enemyHandZone;
    public Transform enemyFieldZone;
    public GameObject enemyHandPrefab;
    public HealthPointHandler enemyHealth;

    [Header("Setting")]
    public TurnState currentTurn = TurnState.Player;

    private string monsterType = "Monster";
    private string spellType = "Spell";
    private int startingHandSize = 5;
    private int maxDeckSize = 30;
    private int startingHP = 20;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        DeckManager.Instance.LoadDeck();
        DeckManager.Instance.GeneratePlayerDeck();
        DeckManager.Instance.GenerateEnemyDeck();
        playerHealth.Initialize(startingHP);
        enemyHealth.Initialize(startingHP);

        StartBattle();
    }

    private void StartBattle()
    {
        //player
        DeckManager.Instance.DrawStartHand(startingHandSize, true);
        foreach (var card in DeckManager.Instance.playerHand)
        {
            InstantiateCardPrefab(card, true);
        }
        ManaManager.Instance.StartTurn(Owner.Player);

        //enemy
        DeckManager.Instance.DrawStartHand(startingHandSize, false);
        foreach (var card in DeckManager.Instance.enemyHand)
        {
            InstantiateCardPrefab(card, false);
        }

        ManaManager.Instance.StartTurn(Owner.Enemy);
    }

    public void StartPlayerTurn()
    {
        UpdateFieldStatus(Owner.Enemy);
        ManaManager.Instance.StartTurn(Owner.Player);
        DrawOneCard(true);
        currentTurn = TurnState.Player;

        ResetFieldCards(Owner.Player);
    }

    public void StartEnemyTurn()
    {
        UpdateFieldStatus(Owner.Player);
        ManaManager.Instance.StartTurn(Owner.Enemy);
        DrawOneCard(false);
        currentTurn = TurnState.Enemy;

        ResetFieldCards(Owner.Enemy);

        StartCoroutine(EnemyTurnRoutine());
    }

    public void TurnChange()
    {
        if (currentTurn == TurnState.Enemy) return;

        if (currentTurn == TurnState.Player)
        {
            StartEnemyTurn();
        }
        else
        {
            StartPlayerTurn();
        }
    }

    public void DrawOneCard(bool isPlayer)
    {
        Transform handZone = isPlayer ? playerHandZone : enemyHandZone;

        if (handZone.childCount >= 9)
        {
            return;
        }

        var card = DeckManager.Instance.DrawOneCard(isPlayer);
        if (card != null)
        {
            InstantiateCardPrefab(card, isPlayer);
        }
    }

    public void InstantiateCardPrefab(ModelDatas.CardData card, bool isPlayer)
    {
        GameObject cardObj = Instantiate(isPlayer ? cardPrefab : enemyHandPrefab, isPlayer ? playerHandZone : enemyHandZone);
        CardDisplay cardDisplay = cardObj.GetComponent<CardDisplay>();
        bool isMonster = card.type == monsterType;

        cardDisplay.owner = isPlayer ? Owner.Player : Owner.Enemy;

        if (isPlayer)
        {
            cardObj.AddComponent<BattleCardDragHandler>();
            cardDisplay.cardCountText.gameObject.SetActive(false);
            cardDisplay.stateArea.gameObject.SetActive(isMonster);
            cardDisplay.SetCard(card);
            cardDisplay.currentZone = CardZone.Hand;
            cardDisplay.UpdateDisplay();
            cardDisplay.SetupCardUI(card);
        }
        else
        {
            cardDisplay.SetCard(card);
            cardDisplay.currentZone = CardZone.Hand;
        }
    }

    public void PlayCard(BattleCardDragHandler card)
    {
        CardDisplay cardDisplay = card.GetComponent<CardDisplay>();
        if (cardDisplay == null) return;

        Debug.Log($"[BattleManager] 卡牌 {card.gameObject.name} 被打出！");

        if (cardDisplay.cardType == "Spell")
        {
            Debug.Log($"[BattleManager] Spell {cardDisplay.cardNameText.text} 正在发动效果...");
            SpellExecutor.ExecuteSpell(cardDisplay, cardDisplay.GetCardData());

            var parentSlot = card.transform.parent;
            if (parentSlot != null)
            {
                FieldSlot fs = parentSlot.GetComponent<FieldSlot>();
                if (fs != null) fs.isOccupied = false;
            }
            Destroy(card.gameObject);
        }
        else
        {
            Debug.Log($"[BattleManager] {cardDisplay.cardNameText.text} 是怪兽卡，进入场上。");
        }
    }

    public void Attack(CardDisplay attacker, CardDisplay target = null)
    {
        int attackerDmg = attacker.atkPoint;
        int targetDmg = target != null ? target.atkPoint : 0;

        if (attackerDmg <= 0) return;

        if (target == null)
        {
            Transform targetField = attacker.owner == Owner.Player ? enemyFieldZone : playerFieldZone;
            bool hasMonster = false;

            foreach (Transform slot in targetField)
            {
                CardDisplay fieldCard = slot.GetComponentInChildren<CardDisplay>();
                if (fieldCard != null && fieldCard.cardType == monsterType)
                {
                    hasMonster = true;
                    break;
                }
            }

            if (hasMonster)
            {
                Debug.Log("[Attack] 无法直接攻击玩家，必须先消灭敌方怪兽！");
                return;
            }

            if (attacker.owner == Owner.Player)
            {
                enemyHealth.TakeDamage(attackerDmg);
            }
            else
            {
                playerHealth.TakeDamage(attackerDmg);
            }
        }
        else
        {
            target.TakeDamage(attackerDmg);
            attacker.TakeDamage(targetDmg);
        }
        attacker.SetIdleAfterAttack();
    }

    private void UpdateFieldStatus(Owner owner)
    {
        Transform fieldZone = owner == Owner.Player ? playerFieldZone : enemyFieldZone;
        foreach (Transform slot in fieldZone)
        {
            CardDisplay card = slot.GetComponentInChildren<CardDisplay>();
            if (card != null && card.cardType == monsterType)
            {
                card.UpdateStatusAtTurnStart();
            }
        }
    }

    private void ResetFieldCards(Owner owner)
    {
        Transform fieldZone = owner == Owner.Player ? playerFieldZone : enemyFieldZone;
        foreach (Transform slot in fieldZone)
        {
            CardDisplay card = slot.GetComponentInChildren<CardDisplay>();
            if (card != null && card.cardType == monsterType)
            {
                card.ResetAttackState();
            }
        }
    }

    public List<CardDisplay> GetEnemyUnits(Owner owner)
    {
        Transform enemyZone = owner == Owner.Player ? enemyFieldZone : playerFieldZone;
        List<CardDisplay> enemies = new();

        foreach (Transform slot in enemyZone)
        {
            CardDisplay card = slot.GetComponentInChildren<CardDisplay>();
            if (card != null && card.cardType == monsterType)
            {
                enemies.Add(card);
            }
        }
        return enemies;
    }

    public List<CardDisplay> GetAllyUnits(Owner owner)
    {
        Transform allyZone = owner == Owner.Player ? playerFieldZone : enemyFieldZone;
        List<CardDisplay> allies = new();

        foreach (Transform slot in allyZone)
        {
            CardDisplay card = slot.GetComponentInChildren<CardDisplay>();
            if (card != null && card.cardType == "Monster")
            {
                allies.Add(card);
            }
        }
        return allies;
    }
    public HealthPointHandler GetCardByOwner(Owner owner)
    {
        return owner == Owner.Player ? playerHealth : enemyHealth;
    }

    //Enemy AI Behavior
    private IEnumerator EnemyTurnRoutine()
    {
        yield return new WaitForSeconds(1f);
        EnemyPlayCard();
        yield return new WaitForSeconds(1f);
        EnemyAttack();
        yield return new WaitForSeconds(1f);
        StartPlayerTurn();
    }

    private void EnemyPlayCard()
    {
        var enemyhand = DeckManager.Instance.enemyHand;
        if (enemyhand.Count == 0) return;

        var candidates = enemyhand.FindAll(c => c.type == monsterType);
        if (candidates.Count == 0) return;

        var card = candidates[Random.Range(0, candidates.Count)];

        if (!ManaManager.Instance.CanAfford(card.cost, Owner.Enemy)) return;

        foreach (Transform slot in enemyFieldZone)
        {
            FieldSlot fieldSlot = slot.GetComponent<FieldSlot>();
            if (!fieldSlot.isOccupied)
            {
                ManaManager.Instance.SpendMana(Owner.Enemy, card.cost);

                Transform toRemove = null;
                foreach (Transform t in enemyHandZone)
                {
                    var cd = t.GetComponent<CardDisplay>();
                    if (cd != null && cd.cardID == card.id)
                    {
                        toRemove = t;
                        break;
                    }
                }
                if (toRemove != null)
                {
                    Destroy(toRemove.gameObject);
                }
                enemyhand.Remove(card);

                GameObject cardObj = Instantiate(cardPrefab, slot);
                CardDisplay cardDisplay = cardObj.GetComponent<CardDisplay>();
                cardDisplay.cardCountText.gameObject.SetActive(false);
                cardDisplay.SetCard(card);
                cardDisplay.currentZone = CardZone.Field;
                cardDisplay.owner = Owner.Enemy;
                fieldSlot.isOccupied = true;
                cardDisplay.SetupCardUI(card);

                EnemyLog($"Enemy played {card.cardName} to the field.");
                break;
            }
        }
    }

    private void EnemyAttack()
    {
        foreach (Transform slot in enemyFieldZone)
        {
            CardDisplay attacker = slot.GetComponentInChildren<CardDisplay>();
            if (attacker == null || attacker.cardType != monsterType || !attacker.isAttack) continue;

            List<CardDisplay> playerMonsters = new List<CardDisplay>();
            foreach (Transform playerSlot in playerFieldZone)
            {
                CardDisplay playerCard = playerSlot.GetComponentInChildren<CardDisplay>();
                if (playerCard != null && playerCard.cardType == monsterType)
                {
                    playerMonsters.Add(playerCard);
                }
            }

            if (playerMonsters.Count > 0)
            {
                CardDisplay target = playerMonsters[Random.Range(0, playerMonsters.Count)];
                EnemyLog($"[Enemy] {attacker.cardNameText.text} attacked {target.cardNameText.text}");
                Attack(attacker, target);
            }
            else
            {
                EnemyLog($"[Enemy] {attacker.cardNameText.text} direct attack！");
                Attack(attacker, null);
            }
        }
    }

    private void EnemyLog(string currentAction)
    {
        if (enemyCurrentBehaviour != null)
        {
            enemyCurrentBehaviour.text = currentAction;
        }
    }
}

