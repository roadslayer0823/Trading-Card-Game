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
    public Transform playerFieldZone;
    public GameObject cardPrefab;
    public HealthPointHandler playerHealth;

    [Header("Enemy UI Reference")]
    public Transform enemyHandZone;
    public Transform enemyFieldZone;
    public HealthPointHandler enemyHealth;

    [Header("Setting")]
    public TurnState currentTurn = TurnState.Player;

    private string monsterType = "Monster";
    private string spellType = "Spell";
    private int startingHandSize = 5;
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
        HandManager.Instance.RefreshHandUI(true);
        ManaManager.Instance.StartTurn(Owner.Player);

        //enemy
        DeckManager.Instance.DrawStartHand(startingHandSize, false);
        HandManager.Instance.RefreshHandUI(false);
        ManaManager.Instance.StartTurn(Owner.Enemy);
    }

    public void StartPlayerTurn()
    {
        UpdateFieldStatus(Owner.Enemy);
        ManaManager.Instance.StartTurn(Owner.Player);
        Debug.Log($"[BattleManager] Player turn start. Hand count before draw: {DeckManager.Instance.playerHand.Count}");
        DrawOneCard(true);
        Debug.Log($"[BattleManager] Player hand count after draw: {DeckManager.Instance.playerHand.Count}");
        currentTurn = TurnState.Player;

        ResetFieldCards(Owner.Player);
    }

    public void StartEnemyTurn()
    {
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
        if ((isPlayer ? DeckManager.Instance.playerHand.Count : DeckManager.Instance.enemyHand.Count) >= 10)
            return;

        var card = DeckManager.Instance.DrawOneCard(isPlayer);
        HandManager.Instance.DrawCard(card, isPlayer);
    }

    public void PlayCard(BattleCardDragHandler card)
    {
        CardDisplay cardDisplay = card.GetComponent<CardDisplay>();
        if (cardDisplay == null) return;

        Debug.Log($"[BattleManager] 卡牌 {card.gameObject.name} 被打出！");

        bool isPlayer = cardDisplay.owner == Owner.Player;
        var handList = isPlayer ? DeckManager.Instance.playerHand : DeckManager.Instance.enemyHand;

        if (handList.Contains(cardDisplay.GetCardData()))
        {
            handList.Remove(cardDisplay.GetCardData());
            Debug.Log($"[BattleManager] Removed {cardDisplay.cardNameText.text} from hand. New hand count: {handList.Count}");
        }

        if (cardDisplay.cardType == spellType)
        {
            Debug.Log($"[BattleManager] Spell {cardDisplay.cardNameText.text} 正在发动效果...");
            EffectExecutor.ExecuteSpell(cardDisplay, cardDisplay.GetCardData());

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
            var data = cardDisplay.GetCardData();
            foreach(var trigger in data.triggers)
            {
                if (trigger.skillTiming == "OnSummon")
                {
                    EffectContext summonContext = new EffectContext(cardDisplay.owner, EffectTarget.FromCard(cardDisplay), 0, "");
                    EffectExecutor.TriggerMonsterEffect(cardDisplay, cardDisplay.GetCardData(), summonContext);
                }
            }
        }

        else
        {
            Debug.Log($"[BattleManager] {cardDisplay.cardNameText.text} 是怪兽卡，进入场上。");
        }
    }

    public void Attack(CardDisplay attacker, CardDisplay target = null)
    {
        int attackerDmg = attacker.atkPoint + attacker.tempAtkBuff;
        int targetDmg = target != null ? target.atkPoint + target.tempAtkBuff: 0;

        if (attackerDmg <= 0) return;

        if (!attacker.isAttack)
        {
            Debug.Log($"{attacker.cardName} 被眩晕或已攻击过，无法行动！");
            return;
        }
        if (attacker.isFrozen && attacker.atkPoint <= 0)
        {
            Debug.Log($"{attacker.cardName} 被冰冻，攻击力为0，无法造成伤害！");
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
            int finalAttackerDmg = CalculateElementReaction(attacker, target, attackerDmg);
            int finalTargetDmg = CalculateElementReaction(target, attacker, targetDmg);

            target.TakeDamage(finalAttackerDmg);
            attacker.TakeDamage(finalTargetDmg);

            var targetData = target.GetCardData();
            foreach(var trigger in targetData.triggers)
            {
                if (trigger.skillTiming == "OnHit")
                {
                    EffectContext hitContext = new EffectContext(attacker.owner, EffectTarget.FromCard(attacker), 0, "");  // attacker 作為 context
                    EffectExecutor.TriggerMonsterEffect(target, target.GetCardData(), hitContext);
                }
            }

            Debug.Log($"战斗结果: {attacker.cardName} [{string.Join(",", attacker.elementTags)}] → {target.cardName} [{string.Join(",", target.elementTags)}] 造成 {finalAttackerDmg} 伤害");
        }
        attacker.SetIdleAfterAttack();
    }

    private int CalculateElementReaction(CardDisplay attacker, CardDisplay defender, int baseDamage)
    {
        HashSet<string> attTags = new HashSet<string>(attacker.elementTags.ConvertAll(t => t.ToLower()));
        HashSet<string> defTags = new HashSet<string>(defender.elementTags.ConvertAll(t => t.ToLower()));

        Debug.Log($"[元素反应检查] {attacker.cardName} tags: [{string.Join(", ", attTags)}] → {defender.cardName} tags: [{string.Join(", ", defTags)}] 基础伤害 {baseDamage}");

        // 蒸发
        if (attTags.Contains("fire") && defTags.Contains("water"))
        {
            Debug.Log("⚡ 蒸发反应！伤害 ×1.5");
            return Mathf.CeilToInt(baseDamage * 1.5f);
        }
        if (attTags.Contains("water") && defTags.Contains("fire"))
        {
            Debug.Log("💨 蒸发反应！伤害 ×2");
            return baseDamage * 2;
        }

        // 融化
        if (attTags.Contains("fire") && defTags.Contains("ice"))
        {
            Debug.Log("🔥 融化反应！伤害 ×2");
            return baseDamage * 2;
        }
        if (attTags.Contains("ice") && defTags.Contains("fire"))
        {
            Debug.Log("❄️ 融化反应！伤害 ×1.5");
            return Mathf.CeilToInt(baseDamage * 1.5f);
        }

        // 雷 + 水 = 感电
        if (attTags.Contains("lightning") && defTags.Contains("water"))
        {
            Debug.Log("⚡ 感电反应！伤害 ×1.5");
            return Mathf.CeilToInt(baseDamage * 1.5f);
        }
        if (attTags.Contains("water") && defTags.Contains("lightning"))
        {
            Debug.Log("💧 感电反应！伤害 ×1.5");
            return Mathf.CeilToInt(baseDamage * 1.5f);
        }

        // 可以继续加：风 + 任意 = 扩散、超载 等

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
                if(card.stunTurnRemaining <= 0)
                {
                     card.ResetAttackState();
                }

                var data = card.GetCardData();
                foreach(var trigger in data.triggers)
                {
                    if (trigger.skillTiming == "PerTurn" || trigger.skillTiming == "OnTurnEnd")
                    {
                        EffectContext turnContext = new EffectContext(card.owner, null, 0, "");
                        EffectExecutor.TriggerMonsterEffect(card, data, turnContext);
                    }
                }
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

                Debug.Log($"[Enemy] Trying to play: {card.cardName}, hand count before removal: {enemyhand.Count}, handZone child count: {enemyHandZone.childCount}");
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
                    Debug.Log($"[Enemy] Removed card object from handZone. New child count: {enemyHandZone.childCount}");
                }
                enemyhand.Remove(card);
                Debug.Log($"[Enemy] Removed card from hand list. New hand count: {enemyhand.Count}");

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

            List<EffectTarget> validTargets = TargetSelector.GetTargets("Enemies", Owner.Enemy);

            if (validTargets.Count > 0)
            {
                EffectTarget randomTarget = validTargets[Random.Range(0, validTargets.Count)];
                CardDisplay targetCard = randomTarget.card;
                EnemyLog($"[Enemy] {attacker.cardNameText.text} attacked {targetCard.cardNameText.text}");
                Attack(attacker, targetCard);
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

