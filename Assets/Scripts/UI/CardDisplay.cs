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
{   public enum Element
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

    [HideInInspector] public string cardName;
    [HideInInspector] public string cardType;
    [HideInInspector] public string cardID;
    [HideInInspector] public int atkPoint;
    [HideInInspector] public int hpPoint;
    [HideInInspector] public int currentCount;
    [HideInInspector] public int deckCount;
    [HideInInspector] public int maxHpPoint;
    [HideInInspector] public PanelType currentPanel;
    [HideInInspector] public bool isAttack = true;
    [HideInInspector] public bool isFrozen = false;

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

    public void UpdateStatusAtTurnStart()
    {
        if (isFrozen)
        {
            frozenTurnRemaining--;
            if (frozenTurnRemaining <= 0)
            {
                isFrozen = false;
                isAttack = true;
                GetComponent<CanvasGroup>().alpha = 1f;
                Debug.Log($"{cardName} 的冰冻状态解除。");
            }
        }
    }

    public void ApplyFreeze(int duration)
    {
        isFrozen = true;
        frozenTurnRemaining = duration;
        isAttack = false;
        GetComponent<CanvasGroup>().alpha = 0.5f;
        Debug.Log($"{cardName} 被冰冻，持续 {duration} 回合。");
    }

    public void Heal(int amount)
    {
        hpPoint = Mathf.Min(hpPoint + amount, maxHpPoint);
        hpText.text = "HP: " + hpPoint;
        Debug.Log($"{cardName} 恢复 {amount} 点HP，当前HP为 {hpPoint}/{maxHpPoint}");
    }

    public void TakeDamage(int dmg)
    {
        hpPoint -= dmg;
        if(hpPoint <= 0)
        {
            FieldSlot parentSlot = GetComponentInParent<FieldSlot>();
            if(parentSlot  != null)
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

    private void SetElementColor(string element)
    {
        if(elementColors.TryGetValue(element.ToLower(), out var color))
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
}
