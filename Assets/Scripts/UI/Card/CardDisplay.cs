using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
public enum CardZone
{
    None,
    Hand,
    Field,
    Deck
}

public class CardDisplay : MonoBehaviour
{
    public enum Element
    {
        Fire,
        Water,
        Earth,
        Wind,
        Light,
        Dark
    }

    [Header("UI Reference")]
    public GameObject damagePopupPrefab;
    public GameObject damagePopupContainer;
    public RectTransform stateArea = null;
    public CanvasGroup cardPrefabCanvasGroup;
    public TMP_Text cardNameText = null;
    public TMP_Text costText = null;
    public TMP_Text skillText = null;
    public TMP_Text atkText = null;
    public TMP_Text hpText = null;
    public TMP_Text cardCountText = null;
    public Image cardBackground = null;
    public Image elementIcon = null;
    public Image cardArtImage = null;
    public CardZone currentZone = CardZone.None;
    public Owner owner = Owner.None;
    public Color monsterCardColor;
    public Color spellCardColor;

    [HideInInspector] public PanelType currentPanel;
    [HideInInspector] public string cardName;
    [HideInInspector] public string cardType;
    [HideInInspector] public string cardID;
    [HideInInspector] public int atkPoint;
    [HideInInspector] public int currentAtkPoint;
    [HideInInspector] public int hpPoint;
    [HideInInspector] public int currentCount;
    [HideInInspector] public int deckCount;
    [HideInInspector] public int maxHpPoint;
    [HideInInspector] public int tempAtkBuff = 0;
    [HideInInspector] public int tempHpBuff = 0;
    [HideInInspector] public int damageReduction = 0;
    [HideInInspector] public int stunTurnRemaining = 0;
    [HideInInspector] public int untargetableTurnRemaining = 0;
    [HideInInspector] public bool isAttack = true;
    [HideInInspector] public bool isFrozen = false;
    [HideInInspector] public List<string> elementTags = new();

    private CardDataSO cardDataSO;
    private int frozenTurnRemaining = 0;
    private CardZone lastZone = CardZone.None;
    private CanvasGroup canvasGroup;

    private CanvasGroup GetCanvasGroup()
    {
        if (canvasGroup == null)
        {
            canvasGroup = cardPrefabCanvasGroup != null ? cardPrefabCanvasGroup : GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        return canvasGroup;
    }

    private void Update()
    {
        if (owner == Owner.Player)
        {
            if (currentZone == CardZone.Hand)
            {
                int cost = cardDataSO != null ? cardDataSO.cost : 0;
                bool canPlay = ManaManager.Instance.CanAfford(cost, owner);
                SetGreyedOut(!canPlay);
            }
            else if (lastZone == CardZone.Hand)
            {
                SetGreyedOut(false);
            }
        }
        lastZone = currentZone;
    }

    private static readonly Dictionary<string, Color32> elementColors = new()
    {
        { "fire",  new Color32(255, 80, 80, 255) },
        { "water", new Color32(80, 150, 255, 255) },
        { "earth", new Color32(130, 100, 70, 255) },
        { "wind",  new Color32(100, 255, 180, 255) },
        { "light", new Color32(255, 240, 100, 255) },
        { "dark",  new Color32(150, 100, 200, 255) },
    };

    public void SetCard(CardDataSO data, int count = 0, PanelType panel = PanelType.None, CardZone zone = CardZone.None)
    {
        cardName = data.cardName;
        cardDataSO = data;
        cardID = data.id;
        currentCount = count;
        currentPanel = panel;
        currentZone = zone;
        cardType = data.type;
        atkPoint = data.atk;
        currentAtkPoint = atkPoint;
        hpPoint = data.hp;
        maxHpPoint = data.hp;
        elementTags.Clear();
        elementTags.Add(data.element);
    }

    public void SetupCardUI(CardDataSO data)
    {
        cardNameText.text = cardName;
        costText.text = data.cost.ToString();
        atkText.text = currentAtkPoint.ToString();
        hpText.text = hpPoint.ToString();
        cardCountText.text = "x" + currentCount.ToString();

        cardArtImage.sprite = data.cardSprite;
        cardBackground.color = data.type == "Monster" ? monsterCardColor : spellCardColor;

        skillText.text = data.skillText;
        SetElementColor(data.element);
    }

    public void UpdateStatusAtTurnEnd()
    {
         if (isFrozen)
         {
             frozenTurnRemaining--;
             if (frozenTurnRemaining <= 0)
             {
                 isFrozen = false;
                 currentAtkPoint = atkPoint;
                 RefreshAtk();
                 if (BattleLogManager.Instance != null)
                 {
                     BattleLogManager.Instance.LogStatus($"<color=white>{cardName}</color>'s freeze status has ended. Attack restored to {currentAtkPoint}.");
                 }
             }
         }

         if (stunTurnRemaining > 0)
         {
             stunTurnRemaining--;
             if (stunTurnRemaining <= 0)
             {
                 isAttack = true;
                 if (BattleLogManager.Instance != null)
                 {
                     BattleLogManager.Instance.LogStatus($"<color=white>{cardName}</color>'s stun status has ended. Ready to attack!");
                 }
             }
         }

         if(untargetableTurnRemaining > 0)
         {
             untargetableTurnRemaining--;
             if(untargetableTurnRemaining <= 0)
             {
                 if (BattleLogManager.Instance != null)
                 {
                     BattleLogManager.Instance.LogStatus($"<color=white>{cardName}</color> is no longer untargetable.");
                 }
                 GetCanvasGroup().alpha = 1f;
             }
         }

         // Auto-restore alpha when BOTH effects are gone
         if (!isFrozen && stunTurnRemaining <= 0)
         {
             GetCanvasGroup().alpha = 1f;
         }
    }

    public void ApplyFreeze(int duration, CardDisplay targetCard)
    {
        if (!isFrozen)
        {
            currentAtkPoint = 0;
            atkText.text = "0";
        }

        isFrozen = true;
        frozenTurnRemaining = Mathf.Max(frozenTurnRemaining, duration + 1);
        GetCanvasGroup().alpha = 0.5f;
    }

    public void ApplyStun(int duration)
    {
        isAttack = false;
        stunTurnRemaining = Mathf.Max(stunTurnRemaining, duration + 1);
        GetCanvasGroup().alpha = 0.5f;
    }

    public void ApplyUntargetable(int duration)
    {
        untargetableTurnRemaining = Mathf.Max(untargetableTurnRemaining, duration + 1);
        GetCanvasGroup().alpha = 0.7f;
    }

    public void Heal(int amount)
    {
        int newMaxHp = maxHpPoint + tempHpBuff;
        hpPoint = Mathf.Min(hpPoint + amount, newMaxHp);
        hpText.text = hpPoint.ToString();
    }

    public void TakeDamage(int dmg)
    {
        int reducedDamage = Mathf.Max(0, dmg - damageReduction);
        hpPoint -= reducedDamage;

        if (hpPoint <= 0)
        {
            FieldSlot parentSlot = GetComponentInParent<FieldSlot>();
            if (parentSlot != null)
            {
                parentSlot.isOccupied = false;
            }
            Destroy(gameObject);
            return;
        }

        hpText.text = Mathf.Max(hpPoint, 0).ToString();

        ShakeCard();

        if(damagePopupPrefab != null)
        {
            Vector3 startPos = hpText.transform.position;
            Vector3 spawnPos = startPos + new Vector3(0, 30f, 0);
            GameObject popupObj = Instantiate(damagePopupPrefab, spawnPos, Quaternion.identity, damagePopupContainer.transform);
            DamagePopup popup = popupObj.GetComponent<DamagePopup>();
            if(popup != null)
            {
                popup.Show(reducedDamage, spawnPos);
            }
        }
    }

    private void ShakeCard()
    {
        LeanTween.cancel(gameObject);

        Vector3 originalPos = transform.localPosition;

        LeanTween.sequence()
        .append(LeanTween.moveLocalX(gameObject, originalPos.x - 15f, 0.08f).setEase(LeanTweenType.easeShake))
        .append(LeanTween.moveLocalX(gameObject, originalPos.x + 15f, 0.08f).setEase(LeanTweenType.easeShake))
        .append(LeanTween.moveLocalX(gameObject, originalPos.x - 8f, 0.06f).setEase(LeanTweenType.easeShake))
        .append(LeanTween.moveLocalX(gameObject, originalPos.x, 0.06f).setEase(LeanTweenType.easeShake));
    }

    public void UpdateCount(int newCount)
    {
        currentCount = newCount;
        cardCountText.text = $"x{currentCount}";
    }

    public void RefreshAtk()
    {
        int finalAtk = currentAtkPoint + tempAtkBuff;
        atkText.text = finalAtk.ToString();
    }

    public void AddElementTag(string element)
    {
        string lowerElement = element.ToLower();
        if (!elementTags.Contains(lowerElement))
        {
            elementTags.Add(lowerElement);
        }
    }

    private void SetElementColor(string element)
    {
        if (elementColors.TryGetValue(element.ToLower(), out var color))
        {
            elementIcon.color = color;
        }
        else
        {
            elementIcon.color = Color.gray;
        }
    }

    public void SetIdleAfterAttack()
    {
        isAttack = false;
        GetCanvasGroup().alpha = 0.5f;
    }

    public void ResetAttackState()
    {
        isAttack = true;
        GetCanvasGroup().alpha = 1f;
    }
    public CardDataSO GetCardDataSO()
    {
        return cardDataSO;
    }

    private void SetGreyedOut(bool isGreyed)
    {
        GetCanvasGroup().alpha = isGreyed ? 0.5f : 1f;
    }

    public bool IsUntargetable()
    {
        return untargetableTurnRemaining > 0;
    }
}