using Unity.VisualScripting;
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

        bool isPlaced = false;

        CardDisplay cardDisplay = GetComponent<CardDisplay>();
        ModelDatas.CardData data = cardDisplay.GetCardData();

        if (eventData.pointerEnter != null)
        {
            FieldSlot slot = eventData.pointerEnter.GetComponentInParent<FieldSlot>();  // 支持拖到卡上
            if (slot != null)
            {
                // 检查是否是法术且需要手动选源 (SingleAlly)
                foreach(var trigger in data.triggers)
                {
                    if (cardDisplay.cardType == "Spell" && trigger.skillTarget == "SingleAlly")
                    {
                        CardDisplay targetOnSlot = slot.GetComponentInChildren<CardDisplay>();
                        if (targetOnSlot != null && targetOnSlot.owner == cardDisplay.owner && targetOnSlot.cardType == "Monster")
                        {
                            // 手动选择成功！传播以 targetOnSlot 为源
                            ManaManager.Instance.SpendMana(cardDisplay.owner, data.cost);
                            EffectExecutor.ExecuteSpellWithManualSource(cardDisplay, data, targetOnSlot);

                            Destroy(gameObject);  // 法术消耗
                            slot.isOccupied = false;  // 不占位
                            isPlaced = true;
                        }
                        else
                        {
                            // 拖到空位或敌方怪兽 → 无效
                            ReturnToHand();
                        }
                    }
                    else
                    {
                        // 正常出牌逻辑（怪兽或不需要手动选的法术）
                        if (!ManaManager.Instance.CanAfford(data.cost, cardDisplay.owner) || slot.isOccupied)
                        {
                            ReturnToHand();
                            Destroy(placeholder);
                            return;
                        }

                        ManaManager.Instance.SpendMana(cardDisplay.owner, data.cost);
                        slot.isOccupied = true;
                        transform.SetParent(slot.transform, false);
                        rectTransform.localScale = Vector3.one;
                        rectTransform.localPosition = Vector3.zero;

                        isLocked = true;
                        cardDisplay.currentZone = CardZone.Field;
                        cardDisplay.UpdateDisplay();
                        BattleManager.Instance.PlayCard(this);

                        if (cardDisplay.cardType == "Monster")
                        {
                            this.AddComponent<AttackDragHandler>();
                        }

                        isPlaced = true;
                    }
                }
            }
        }

        if (!isPlaced)
        {
            ReturnToHand();
        }

        Destroy(placeholder);
    }

    private void ReturnToHand()
    {
        int index = placeholder != null ? placeholder.transform.GetSiblingIndex() : originalParent.childCount;
        transform.SetParent(originalParent, false);
        transform.SetSiblingIndex(index);
        rectTransform.localScale = Vector3.one;
        rectTransform.localPosition = Vector3.zero;
    }
}
