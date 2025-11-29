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

        if(eventData.pointerEnter != null) 
        {
            FieldSlot slot = eventData.pointerEnter.GetComponent<FieldSlot>();
            if (slot != null) 
            {
                CardDisplay cardDisplay = this.GetComponent<CardDisplay>();

                if (!ManaManager.Instance.CanAfford(int.Parse(cardDisplay.costText.text), cardDisplay.owner) || slot.isOccupied) 
                {
                    ReturnToHand();
                    Destroy(placeholder);
                    return;
                }

                ManaManager.Instance.SpendMana(cardDisplay.owner, int.Parse(cardDisplay.costText.text));
                slot.isOccupied = true;
                transform.SetParent(slot.transform, false);
                rectTransform.localScale = Vector3.one;
                rectTransform.localPosition = Vector3.zero;

                isLocked = true;
                cardDisplay.currentZone = CardZone.Field;
                cardDisplay.UpdateDisplay();
                BattleManager.Instance.PlayCard(this);
                if(cardDisplay.cardType == "Monster")
                {
                    this.AddComponent<AttackDragHandler>();
                }

                isPlaced = true;
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
