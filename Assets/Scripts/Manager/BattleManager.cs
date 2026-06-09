using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
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
    public Transform playerFieldZone;
    public GameObject cardPrefab;
    public HealthPointHandler playerHealth;

    [Header("Enemy UI Reference")]
    public Transform enemyHandZone;
    public Transform enemyFieldZone;
    public HealthPointHandler enemyHealth;

    [Header("Setting")]
    public TurnState currentTurn = TurnState.Player;
    public Canvas canvas;

    [Header("Game Over")]
    public bool gameEnded = false;
    public GameObject gameOverPanel;

    [Header("Script Reference")]
    public FirstTurnIntro firstTurnIntro;
    public BattleLogUI battleLogUI;

    private string monsterType = "Monster";
    private string spellType = "Spell";
    private int startingHandSize = 5;
    private int startingHP = 20;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        if (gameEnded)
        {
            if (Input.GetMouseButtonDown(0)) return;
            return;
        }
    }

    private void Start()
    {
        battleLogUI.Initialize();
        StartGame();
    }

    //Battle
    public void StartGame()
    {
        gameEnded = false;
        ClearFieldZone(playerFieldZone);
        ClearFieldZone(enemyFieldZone);
        if (enemyCurrentBehaviour != null) enemyCurrentBehaviour.text = "";

        DeckManager.Instance.GeneratePlayerDeck();
        DeckManager.Instance.GenerateEnemyDeck();
        ManaManager.Instance.ResetMana();
        playerHealth.Initialize(startingHP);
        enemyHealth.Initialize(startingHP);
        gameOverPanel.SetActive(false);

        StartBattle();
    }

    private void ClearFieldZone(Transform fieldZone)
    {
        foreach (Transform slot in fieldZone)
        {
            var fieldSlot = slot.GetComponent<FieldSlot>();
            if (fieldSlot != null)
            {
                fieldSlot.isOccupied = false;
            }

            foreach (Transform child in slot)
            {
                if (child.GetComponent<CardDisplay>() != null)
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }

    private void StartBattle()
    {
        //player
        DeckManager.Instance.DrawStartHand(startingHandSize, true);
        HandManager.Instance.RefreshHandUI(true);
        ManaManager.Instance.StartTurn(Owner.Player);

        //enemy
        DeckManager.Instance.DrawStartHand(startingHandSize, false);
        HandManager.Instance.RefreshHandUI(false);
        ManaManager.Instance.StartTurn(Owner.Enemy);

        // Random coin flip for first turn
        bool playerGoesFirst = Random.value > 0.5f;
        
        StartCoroutine(StartBattleWithIntro(playerGoesFirst));
    }

    private IEnumerator StartBattleWithIntro(bool playerGoesFirst)
    {
        if (firstTurnIntro != null)
        {
            yield return StartCoroutine(firstTurnIntro.FlashAndShowResult(playerGoesFirst));
        }

        if (playerGoesFirst)
        {
            StartPlayerTurn();
        }
        else
        {
            StartEnemyTurn();
        }
    }

    public void StartPlayerTurn()
    {
        UpdateFieldStatus(Owner.Enemy);
        ManaManager.Instance.StartTurn(Owner.Player);
        if (BattleLogManager.Instance != null)
        {
            BattleLogManager.Instance.LogGeneral("<color=yellow>Player's turn started.</color>");
        }
        DrawOneCard(true);
        currentTurn = TurnState.Player;

        ResetFieldCards(Owner.Player);
    }

    public void StartEnemyTurn()
    {
        ManaManager.Instance.StartTurn(Owner.Enemy);
        if (BattleLogManager.Instance != null)
        {
            BattleLogManager.Instance.LogGeneral("<color=yellow>Enemy's turn started.</color>");
        }
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
            CheckGameOver();
        }
        else
        {
            StartPlayerTurn();
            CheckGameOver();
        }
    }

    public void CheckGameOver()
    {
        if (gameEnded) return;

        if(playerHealth.currentHealth <= 0)
        {
            gameEnded = true;
            ShowGameOver(false);
            return;
        }

        if(enemyHealth.currentHealth <= 0)
        {
            gameEnded = true;
            ShowGameOver(true);
            return;
        }
    }

    private void ShowGameOver(bool isWin)
    {
        if(gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            TextMeshProUGUI titleText = gameOverPanel.GetComponentInChildren<TextMeshProUGUI>();
            titleText.text = isWin ? "You Win!" : "You Lose!";
        }
    }

    //Generic Behaviour
    public void DrawOneCard(bool isPlayer)
    {
        if ((isPlayer ? DeckManager.Instance.playerHand.Count : DeckManager.Instance.enemyHand.Count) >= 10)
            return;

        var card = DeckManager.Instance.DrawOneCard(isPlayer);
        HandManager.Instance.DrawCard(card, isPlayer);
    }

    private int CalculateElementReaction(CardDisplay attacker, CardDisplay defender, int baseDamage)
    {
        HashSet<string> attTags = new HashSet<string>(attacker.elementTags.ConvertAll(t => t.ToLower()));
        HashSet<string> defTags = new HashSet<string>(defender.elementTags.ConvertAll(t => t.ToLower()));

        // 蒸发
        if (attTags.Contains("fire") && defTags.Contains("water"))
        {
            if (BattleLogManager.Instance != null)
                BattleLogManager.Instance.LogElementReaction($"Vaporize Reaction! <color=white>{attacker.cardName}</color> dealt 1.5x damage to <color=white>{defender.cardName}</color>.");
            return Mathf.CeilToInt(baseDamage * 1.5f);
        }
        if (attTags.Contains("water") && defTags.Contains("fire"))
        {
            if (BattleLogManager.Instance != null)
                BattleLogManager.Instance.LogElementReaction($"Vaporize Reaction! <color=white>{attacker.cardName}</color> dealt 2.0x damage to <color=white>{defender.cardName}</color>.");
            return baseDamage * 2;
        }

        // 融化
        if (attTags.Contains("fire") && defTags.Contains("ice"))
        {
            if (BattleLogManager.Instance != null)
                BattleLogManager.Instance.LogElementReaction($"Melt Reaction! <color=white>{attacker.cardName}</color> dealt 2.0x damage to <color=white>{defender.cardName}</color>.");
            return baseDamage * 2;
        }
        if (attTags.Contains("ice") && defTags.Contains("fire"))
        {
            if (BattleLogManager.Instance != null)
                BattleLogManager.Instance.LogElementReaction($"Melt Reaction! <color=white>{attacker.cardName}</color> dealt 1.5x damage to <color=white>{defender.cardName}</color>.");
            return Mathf.CeilToInt(baseDamage * 1.5f);
        }

        // 雷 + 水 = 感电
        if (attTags.Contains("lightning") && defTags.Contains("water"))
        {
            if (BattleLogManager.Instance != null)
                BattleLogManager.Instance.LogElementReaction($"Electro-Charged Reaction! <color=white>{attacker.cardName}</color> dealt 1.5x damage to <color=white>{defender.cardName}</color>.");
            return Mathf.CeilToInt(baseDamage * 1.5f);
        }
        if (attTags.Contains("water") && defTags.Contains("lightning"))
        {
            if (BattleLogManager.Instance != null)
                BattleLogManager.Instance.LogElementReaction($"Electro-Charged Reaction! <color=white>{attacker.cardName}</color> dealt 1.5x damage to <color=white>{defender.cardName}</color>.");
            return Mathf.CeilToInt(baseDamage * 1.5f);
        }

        return baseDamage;
    }

    private void UpdateFieldStatus(Owner owner)
    {
        Transform fieldZone = owner == Owner.Player ? playerFieldZone : enemyFieldZone;
        foreach (Transform slot in fieldZone)
        {
            CardDisplay card = slot.GetComponentInChildren<CardDisplay>();
            if (card != null && card.cardType == monsterType)
            {
                card.UpdateStatusAtTurnEnd();
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
                if (card.stunTurnRemaining <= 0)
                {
                    card.ResetAttackState();
                }

                var data = card.GetCardDataSO();
                foreach (var trigger in data.triggers)
                {
                    if (trigger.skillTiming == "PerTurn" || trigger.skillTiming == "OnTurnEnd")
                    {
                        EffectContext turnContext = new EffectContext(card.owner, null, null, 0, "");
                        EffectExecutor.TriggerMonsterEffect(card, data, turnContext);
                    }
                }
            }
        }
    }

    //Player Behaviour
    public void PlayCard(BattleCardDragHandler card)
    {
        CardDisplay cardDisplay = card.GetComponent<CardDisplay>();
        if (cardDisplay == null) return;

        bool isPlayer = cardDisplay.owner == Owner.Player;
        var handList = isPlayer ? DeckManager.Instance.playerHand : DeckManager.Instance.enemyHand;

        if (handList.Contains(cardDisplay.GetCardDataSO()))
        {
            handList.Remove(cardDisplay.GetCardDataSO());
        }

        if (BattleLogManager.Instance != null)
        {
            BattleLogManager.Instance.LogGeneral($"<color=white>{(isPlayer ? "Player" : "Enemy")}</color> played card: <color=yellow>{cardDisplay.cardName}</color> ({(cardDisplay.cardType == spellType ? "Spell" : "Monster")}).");
        }

        if (cardDisplay.cardType == spellType)
        {
            EffectExecutor.ExecuteSpell(cardDisplay, cardDisplay.GetCardDataSO());
            var parentSlot = card.transform.parent;
            if (parentSlot != null)
            {
                FieldSlot fs = parentSlot.GetComponent<FieldSlot>();
                if (fs != null) fs.isOccupied = false;
            }

            Destroy(card.gameObject);
            return;
        }

        if(cardDisplay.cardType == monsterType)
        {
            var data = cardDisplay.GetCardDataSO();
            foreach(var trigger in data.triggers)
            {
                if (trigger.skillTiming == "OnSummon")
                {
                    EffectContext summonContext = new EffectContext(cardDisplay.owner, EffectTarget.FromCard(cardDisplay), null, 0, "");
                    EffectExecutor.TriggerMonsterEffect(cardDisplay, cardDisplay.GetCardDataSO(), summonContext);
                }
            }
        }
    }

    public void Attack(CardDisplay attacker, CardDisplay target = null)
    {
        int attackerDmg = attacker.currentAtkPoint + attacker.tempAtkBuff;
        int targetDmg = target != null ? target.currentAtkPoint + target.tempAtkBuff: 0;

        if (attackerDmg <= 0) return;

        if (!attacker.isAttack)
        {
            if (BattleLogManager.Instance != null)
                BattleLogManager.Instance.LogStatus($"<color=white>{attacker.cardName}</color> is stunned or has already acted!");
            return;
        }
        if (attacker.isFrozen && (attacker.currentAtkPoint + attacker.tempAtkBuff) <= 0)
        {
            if (BattleLogManager.Instance != null)
                BattleLogManager.Instance.LogStatus($"<color=white>{attacker.cardName}</color> is frozen and has 0 ATK, unable to deal damage!");
            attacker.SetIdleAfterAttack(); // 还是要标记已攻击
            return;
        }

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
                if (BattleLogManager.Instance != null)
                    BattleLogManager.Instance.LogGeneral("Cannot attack the leader directly while enemy monsters are on the field!");
                return;
            }

            if (attacker.owner == Owner.Player)
            {
                enemyHealth.TakeDamage(attackerDmg);
                if (BattleLogManager.Instance != null)
                    BattleLogManager.Instance.LogDamage(attacker.cardName, "Enemy Leader", attackerDmg);
            }
            else
            {
                playerHealth.TakeDamage(attackerDmg);
                if (BattleLogManager.Instance != null)
                    BattleLogManager.Instance.LogDamage(attacker.cardName, "Player Leader", attackerDmg);
            }
        }
        else
        {
            int finalAttackerDmg = CalculateElementReaction(attacker, target, attackerDmg);
            int finalTargetDmg = CalculateElementReaction(target, attacker, targetDmg);

            target.TakeDamage(finalAttackerDmg);
            attacker.TakeDamage(finalTargetDmg);

            var targetData = target.GetCardDataSO();
            foreach(var trigger in targetData.triggers)
            {
                if (trigger.skillTiming == "OnHit")
                {
                    EffectContext hitContext = new EffectContext(attacker.owner, EffectTarget.FromCard(target), attacker, 0, "");  // attacker 作為 context
                    EffectExecutor.TriggerMonsterEffect(target, target.GetCardDataSO(), hitContext);
                }
            }

            if (BattleLogManager.Instance != null)
            {
                BattleLogManager.Instance.LogDamage(attacker.cardName, target.cardName, finalAttackerDmg);
            }
        }
        attacker.SetIdleAfterAttack();
        CheckGameOver();
    }

    //Enemy AI Behavior
    private IEnumerator EnemyTurnRoutine()
    {
        if (gameEnded) yield break;
        yield return new WaitForSeconds(1f);
        EnemyPlayCard();
        yield return new WaitForSeconds(1f);
        EnemyAttack();
        yield return new WaitForSeconds(1f);

        ProcessStatusEndOfTurn();

        StartPlayerTurn();
    }

    private void ProcessStatusEndOfTurn()
    {
        foreach (Transform slot in playerFieldZone)
            slot.GetComponentInChildren<CardDisplay>()?.UpdateStatusAtTurnEnd();
        foreach (Transform slot in enemyFieldZone)
            slot.GetComponentInChildren<CardDisplay>()?.UpdateStatusAtTurnEnd();
    }

    private void EnemyPlayCard()
    {
        var enemyhand = DeckManager.Instance.enemyHand;
        if (enemyhand.Count == 0) return;

        var monsterCandidates = enemyhand.FindAll(c => c.type == monsterType);
        if(monsterCandidates.Count > 0)
        {
            var validMonsters = monsterCandidates.FindAll(c => ManaManager.Instance.CanAfford(c.cost, Owner.Enemy));
            if(validMonsters.Count > 0)
            {
                var card = validMonsters[Random.Range(0, validMonsters.Count)];

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
                        CardDataSO data = cardDisplay.GetCardDataSO();
                        cardDisplay.cardCountText.gameObject.SetActive(false);
                        cardDisplay.SetCard(card);
                        cardDisplay.currentZone = CardZone.Field;
                        cardDisplay.owner = Owner.Enemy;
                        fieldSlot.isOccupied = true;
                        cardDisplay.SetupCardUI(card);

                        if (cardDisplay != null && data != null)
                        {
                            foreach (var trigger in data.triggers)
                            {
                                if (trigger.skillTiming == "OnSummon" || trigger.skillTiming == "OnPlay")
                                {
                                    EffectContext summonContext = new EffectContext(Owner.Enemy, EffectTarget.FromCard(cardDisplay), null, 0, "");
                                    EffectExecutor.TriggerMonsterEffect(cardDisplay, data, summonContext);
                                }
                            }
                        }

                        if (BattleLogManager.Instance != null)
                        {
                            BattleLogManager.Instance.LogGeneral($"<color=white>Enemy</color> played card: <color=yellow>{card.cardName}</color> (Monster).");
                        }
                        return;
                    }
                }
            }
        }

        var spellCandidates = enemyhand.FindAll(c => c.type == spellType);
        if(spellCandidates.Count > 0)
        {
            var validSpells = spellCandidates.FindAll(c => ManaManager.Instance.CanAfford(c.cost, Owner.Enemy));
            if(validSpells.Count > 0)
            {
                var card = validSpells[Random.Range(0, validSpells.Count)];

                bool canPlay = true;
                CardDisplay tempDisplay = null;
                foreach (Transform t in enemyHandZone)
                {
                    var cd = t.GetComponent<CardDisplay>();
                    if (cd != null && cd.cardID == card.id)
                    {
                        tempDisplay = cd;
                        break;
                    }
                }

                foreach (var trigger in card.triggers)
                {
                    if (NeedsTarget(trigger.skillTarget))
                    {
                        EffectContext tempContext = new EffectContext(Owner.Enemy, null, null, 0, "");
                        List<EffectTarget> targets = TargetSelector.GetTargets(trigger.skillTarget, Owner.Enemy, tempContext, null);
                        if(targets.Count == 0)
                        {
                            canPlay = false;
                            break;
                        }
                    }
                }

                if (canPlay)
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

                    ShowSpellPopup(card.cardName, card.skillText, card.cost.ToString(), card.cardSprite);
                    if (BattleLogManager.Instance != null)
                    {
                        BattleLogManager.Instance.LogGeneral($"<color=white>Enemy</color> played card: <color=yellow>{card.cardName}</color> (Spell).");
                    }
                    EffectExecutor.ExecuteSpell(tempDisplay, card);
                }
            }
        }
    }

    private void EnemyAttack()
    {
        foreach (Transform slot in enemyFieldZone)
        {
            CardDisplay attacker = slot.GetComponentInChildren<CardDisplay>();
            if (attacker == null || attacker.cardType != monsterType || !attacker.isAttack) continue;

            List<EffectTarget> validTargets = TargetSelector.GetTargets("Enemies", Owner.Enemy);

            if (validTargets.Count > 0)
            {
                EffectTarget randomTarget = validTargets[Random.Range(0, validTargets.Count)];
                CardDisplay targetCard = randomTarget.card;
                if (BattleLogManager.Instance != null)
                {
                    BattleLogManager.Instance.LogGeneral($"<color=white>Enemy's {attacker.cardName}</color> attacked <color=white>{targetCard.cardName}</color>.");
                }
                Attack(attacker, targetCard);
            }
            else
            {
                if (BattleLogManager.Instance != null)
                {
                    BattleLogManager.Instance.LogGeneral($"<color=white>Enemy's {attacker.cardName}</color> attacked directly!");
                }
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

    private bool NeedsTarget(string targetType)
    {
        return targetType != "Self" || targetType != "None" || targetType != "";  // 根據你的 targetType 調整
    }

    //Verification
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

    //UI
    private void ShowSpellPopup(string spellName, string skillText, string cost, Sprite artSprite = null)
    {
        if (cardPrefab == null) return;

        GameObject popup = Instantiate(cardPrefab, Vector3.zero, Quaternion.identity);
        popup.transform.SetParent(canvas.transform, false);
        Transform container = popup.transform.Find("Container");

        if(container != null)
        {
            TMP_Text nameText = container.Find("NameText")?.GetComponent<TMP_Text>();
            TMP_Text costText = container.Find("Cost")?.GetComponent<TMP_Text>();
            TMP_Text descriptionText = container.Find("SkillText")?.GetComponent<TMP_Text>();
            if (nameText != null) nameText.text = spellName;
            if (costText != null) costText.text = cost;
            if (descriptionText != null) descriptionText.text = skillText;

            Image bgImage = container.Find("CardImage")?.GetComponent<Image>();
            if (bgImage != null && artSprite != null) bgImage.sprite = artSprite;

            Transform stateArea = container.Find("StateArea");
            if (stateArea != null) stateArea.gameObject.SetActive(false);
        }

        CanvasGroup cg = popup.GetComponent<CanvasGroup>();
        if (cg == null) cg = popup.AddComponent<CanvasGroup>();

        cg.alpha = 1f;
        LeanTween.alphaCanvas(cg, 0f, 2f).setEase(LeanTweenType.easeInOutQuad).setOnComplete(() => Destroy(popup));
    }

    public void ReturnToMainMenu()
    {
        // UIManager only lives in MainScene, so load directly here.
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }
}

