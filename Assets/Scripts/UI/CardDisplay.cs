using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
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
    public RectTransform stateArea = null;
    public TMP_Text cardNameText = null;
    public TMP_Text costText = null;
    public TMP_Text skillText = null;
    public TMP_Text atkText = null;
    public TMP_Text hpText = null;
    public TMP_Text cardCountText = null;
    public Image elementIcon = null;
    public Image cardArtImage = null;
    public CardZone currentZone = CardZone.None;
    public Owner owner = Owner.None;

    [HideInInspector] public PanelType currentPanel;
    [HideInInspector] public string cardName;
    [HideInInspector] public string cardType;
    [HideInInspector] public string cardID;
    [HideInInspector] public int atkPoint;
    [HideInInspector] public int hpPoint;
    [HideInInspector] public int currentCount;
    [HideInInspector] public int deckCount;
    [HideInInspector] public int maxHpPoint;
    [HideInInspector] public int tempAtkBuff = 0;
    [HideInInspector] public int tempHpBuff = 0;
    [HideInInspector] public int stunTurnRemaining = 0;
    [HideInInspector] public int untargetableTurnRemaining = 0;
    [HideInInspector] public int originalAtkPoint;
    [HideInInspector] public bool isAttack = true;
    [HideInInspector] public bool isFrozen = false;
    [HideInInspector] public List<string> elementTags = new();

    private ModelDatas.CardData cardData;
    private int frozenTurnRemaining = 0;
    private static readonly Dictionary<string, Color32> elementColors = new()
    {
        { "fire",  new Color32(255, 80, 80, 255) },
        { "water", new Color32(80, 150, 255, 255) },
        { "earth", new Color32(130, 100, 70, 255) },
        { "wind",  new Color32(100, 255, 180, 255) },
        { "light", new Color32(255, 240, 100, 255) },
        { "dark",  new Color32(150, 100, 200, 255) },
    };

    public void SetCard(ModelDatas.CardData data, int count = 0, PanelType panel = PanelType.None, CardZone zone = CardZone.None)
    {
        cardName = data.cardName;
        cardData = data;
        cardID = data.id;
        currentCount = count;
        currentPanel = panel;
        currentZone = zone;
        cardType = data.type;
        atkPoint = data.atk;
        hpPoint = data.hp;
        maxHpPoint = data.hp;
        elementTags.Clear();
        elementTags.Add(data.element);
    }

    public void SetupCardUI(ModelDatas.CardData data)
    {
        cardNameText.text = cardName;
        costText.text = data.cost.ToString();
        atkText.text = "ATK: " + atkPoint.ToString();
        hpText.text = "HP: " + hpPoint.ToString();
        cardCountText.text = "x" + currentCount.ToString();

        cardArtImage.sprite = data.cardSprite;

        skillText.text = data.skillText;
        SetElementColor(data.element);

        LayoutRebuilder.ForceRebuildLayoutImmediate(stateArea);
    }

    public void UpdateDisplay()
    {
        if (owner == Owner.Player)
        {
            if (currentZone == CardZone.Hand)
            {
                bool canPlay = ManaManager.Instance.CanAfford(cardData.cost, owner);
                SetGreyedOut(!canPlay);
            }
            else
            {
                SetGreyedOut(false);
            }
        }
    }

   public void UpdateStatusAtTurnEnd()
   {
        if (isFrozen)
        {
            frozenTurnRemaining--;
            if (frozenTurnRemaining <= 0)
            {
                isFrozen = false;
                atkPoint = originalAtkPoint;
                atkText.text = "ATK: " + atkPoint;
                Debug.Log($"{cardName} 的冰冻状态解除，攻击力恢复为 {atkPoint}。");
            }
        }

        if (stunTurnRemaining > 0)
        {
            stunTurnRemaining--;
            if (stunTurnRemaining <= 0)
            {
                isAttack = true;
                Debug.Log($"{cardName} 的眩晕状态解除，可以攻击了。");
            }
        }

        if(untargetableTurnRemaining > 0)
        {
            untargetableTurnRemaining--;
            if(untargetableTurnRemaining <= 0)
            {
                Debug.Log($"{cardName} 的 Untargetable 狀態解除");
                GetComponent<CanvasGroup>().alpha = 1f;
            }
        }

        // Auto-restore alpha when BOTH effects are gone
        if (!isFrozen && stunTurnRemaining <= 0)
        {
            GetComponent<CanvasGroup>().alpha = 1f;
        }
    }

    public void ApplyFreeze(int duration)
    {
        if (!isFrozen)
        {
            originalAtkPoint = atkPoint;  // Save original ATK
            atkPoint = 0;
            atkText.text = "ATK: 0";
        }

        isFrozen = true;
        frozenTurnRemaining = Mathf.Max(frozenTurnRemaining, duration);
        GetComponent<CanvasGroup>().alpha = 0.5f;
    }

    public void ApplyStun(int duration)
    {
        isAttack = false;
        stunTurnRemaining = Mathf.Max(stunTurnRemaining, duration);
        GetComponent<CanvasGroup>().alpha = 0.5f;
    }

    public void ApplyUntargetable(int duration)
    {
        untargetableTurnRemaining = Mathf.Max(untargetableTurnRemaining, duration);
        GetComponent<CanvasGroup>().alpha = 0.7f;
        Debug.Log($"{cardName} 獲得 Untargetable（無法被選為目標），持續 {duration} 回合");
    }

    public void Heal(int amount)
    {
        int newMaxHp = maxHpPoint + tempHpBuff;
        hpPoint = Mathf.Min(hpPoint + amount, newMaxHp);
        hpText.text = $"HP: {hpPoint} (+{tempHpBuff})";
        Debug.Log($"{cardName} 恢复 {amount} 点HP，当前HP为 {hpPoint}/{newMaxHp}");
    }

    public void TakeDamage(int dmg)
    {
        hpPoint -= dmg;

        if (hpPoint <= 0)
        {
            FieldSlot parentSlot = GetComponentInParent<FieldSlot>();
            if (parentSlot != null)
            {
                parentSlot.isOccupied = false;
            }
            Destroy(gameObject);
        }

        hpText.text = "HP: " + Mathf.Max(hpPoint, 0);
        Debug.Log($"{cardName} take {dmg} damage, final hp is {hpPoint}");
    }

    public void UpdateCount(int newCount)
    {
        currentCount = newCount;
        cardCountText.text = $"x{currentCount}";
    }

    public void AddElementTag(string element)
    {
        string lowerElement = element.ToLower();
        if (!elementTags.Contains(lowerElement))
        {
            elementTags.Add(lowerElement);
            Debug.Log($"[{cardName}] 获得新元素标签: {lowerElement}");
        }
        else
        {
            Debug.Log($"[{cardName}] 已拥有元素标签: {lowerElement} (跳过)");
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
        GetComponent<CanvasGroup>().alpha = 0.7f;
    }

    public void ResetAttackState()
    {
        isAttack = true;
        GetComponent<CanvasGroup>().alpha = 1f;
    }
    public ModelDatas.CardData GetCardData()
    {
        return cardData;
    }

    private void SetGreyedOut(bool isGreyed)
    {
        GetComponent<CanvasGroup>().alpha = isGreyed ? 0.5f : 1f;
    }

    private void OnEnable()
    {
        ManaManager.OnManaChanged += UpdateDisplay;
    }
    private void OnDisable()
    {
        ManaManager.OnManaChanged -= UpdateDisplay;
    }

    public bool IsUntargetable()
    {
        return untargetableTurnRemaining > 0;
    }
}