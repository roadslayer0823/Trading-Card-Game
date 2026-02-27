using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BattleCardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Transform originalParent;

    private bool isLocked = false;
    private GameObject placeholder;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isLocked) return;

        originalParent = transform.parent;
        canvasGroup.blocksRaycasts = false;

        placeholder = new GameObject("Placeholder");
        placeholder.transform.SetParent(originalParent, false);

        var layoutElem = placeholder.AddComponent<LayoutElement>();
        var currentLayout = GetComponent<LayoutElement>();
        if (currentLayout != null)
        {
            layoutElem.preferredWidth = currentLayout.preferredWidth;
            layoutElem.preferredHeight = currentLayout.preferredHeight;
            layoutElem.flexibleWidth = currentLayout.flexibleWidth;
            layoutElem.flexibleHeight = currentLayout.flexibleHeight;
        }

        placeholder.transform.SetSiblingIndex(transform.GetSiblingIndex());

        transform.SetParent(transform.root, true);
        GetComponent<CardHoverEffect>()?.DisableHover();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isLocked) return;
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isLocked) return;

        canvasGroup.blocksRaycasts = true;
        bool successfullyPlaced;

        CardDisplay cardDisplay = GetComponent<CardDisplay>();
        var data = cardDisplay.GetCardData();

        // 沒有拖到任何有效位置 → 直接回手牌
        if (eventData.pointerEnter == null)
        {
            ReturnToHand();
            Destroy(placeholder);
            return;
        }

        FieldSlot slot = eventData.pointerEnter.GetComponentInParent<FieldSlot>();
        if (slot == null)
        {
            ReturnToHand();
            Destroy(placeholder);
            return;
        }

        if (IsManualTargetSpell(cardDisplay, data))
        {
            successfullyPlaced = TryPlayManualTargetSpell(cardDisplay, data, slot);
        }
        else
        {
            successfullyPlaced = TryPlayNormalCard(cardDisplay, data, slot);
        }

        if (!successfullyPlaced)
        {
            ReturnToHand();
        }

        Destroy(placeholder);
        GetComponent<CardHoverEffect>()?.DisableHover();
    }

    private bool IsManualTargetSpell(CardDisplay cardDisplay, ModelDatas.CardData data)
    {
        if (cardDisplay.cardType != "Spell") return false;

        foreach (var trigger in data.triggers)
        {
            if (trigger.skillTarget == "SingleAlly")
                return true;
        }
        return false;
    }

    private bool TryPlayManualTargetSpell(CardDisplay cardDisplay, ModelDatas.CardData data, FieldSlot slot)
    {
        CardDisplay target = slot.GetComponentInChildren<CardDisplay>();

        // 有目標 → 嘗試扣魔力
        if (!ManaManager.Instance.SpendMana(cardDisplay.owner, data.cost))
        {
            // SpendMana 內部已經顯示 "Insufficient Mana" 了，這裡不用再顯示
            return false;
        }

        if (target == null || target.owner != cardDisplay.owner || target.cardType != "Monster")
        {
            FeedbackManager.Instance.ShowFeedback(CardPlayError.NoValidTarget);
            ManaManager.Instance.ReturnMana(cardDisplay.owner, data.cost);
            return false;
        }

        // 成功扣魔 → 執行效果
        EffectExecutor.ExecuteSpellWithManualSource(cardDisplay, data, target);
        Destroy(gameObject);
        // 注意：法術不占位，所以不設 slot.isOccupied = true

        return true;
    }

    private bool TryPlayNormalCard(CardDisplay cardDisplay, ModelDatas.CardData data, FieldSlot slot)
    {
        // 先檢查位置是否合法
        if (slot.isOccupied)
        {
            FeedbackManager.Instance.ShowFeedback(CardPlayError.InvalidZone);
            return false;
        }

        // 所有前置檢查通過 → 才扣魔力
        if (!ManaManager.Instance.SpendMana(cardDisplay.owner, data.cost))
        {
            return false;
        }

        // 如果是法術，檢查自動目標是否有效（在扣魔力之前！）
        if (cardDisplay.cardType == "Spell" && !HasValidAutoTargets(cardDisplay, data))
        {
            FeedbackManager.Instance.ShowFeedback(CardPlayError.NoValidTarget);
            ManaManager.Instance.ReturnMana(cardDisplay.owner, data.cost);
            return false;
        }

        // 成功扣魔 → 放置到場上
        slot.isOccupied = true;
        transform.SetParent(slot.transform, false);
        rectTransform.localScale = Vector3.one;
        rectTransform.localPosition = Vector3.zero;
        isLocked = true;
        cardDisplay.currentZone = CardZone.Field;

        // 呼叫 PlayCard 執行 OnSummon 或法術效果
        BattleManager.Instance.PlayCard(this);

        if (cardDisplay.cardType == "Monster")
        {
            gameObject.AddComponent<AttackDragHandler>();
        }

        return true;
    }

    private bool HasValidAutoTargets(CardDisplay cardDisplay, ModelDatas.CardData data)
    {
        if (cardDisplay.cardType != "Spell") return true;  // 怪獸不需要檢查目標

        foreach (var trigger in data.triggers)
        {
            string targetType = trigger.skillTarget;

            // 跳過手動選目標的類型（已經在 TryPlayManualTargetSpell 處理）
            if (targetType == "SingleAlly") continue;

            List<EffectTarget> targets = TargetSelector.GetTargets(targetType, cardDisplay.owner, context: null,sourceCard: cardDisplay);

            if (targets.Count == 0)
            {
                return false;
            }
        }

        return true;
    }

    public void ReturnToHand()
    {
        int index = placeholder != null ? placeholder.transform.GetSiblingIndex() : originalParent.childCount;
        transform.SetParent(originalParent, false);
        transform.SetSiblingIndex(index);
        rectTransform.localScale = Vector3.one;
        rectTransform.localPosition = Vector3.zero;
    }
}
