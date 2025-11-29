using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Transform originalParent;
    private Vector2 originalPosition;

    public CardDisplay cardDisplay;

    void Awake() 
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        cardDisplay = GetComponent<CardDisplay>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (cardDisplay.currentCount <= 0) return;
        GameObject dragObject = Instantiate(gameObject, transform.root);
        CardDisplay dragObjectDisplay = dragObject.GetComponent<CardDisplay>();
        dragObjectDisplay.cardCountText.gameObject.SetActive(false);
        dragObject.transform.position = transform.position;

        dragObject.GetComponent<CanvasGroup>().blocksRaycasts = false;
        eventData.pointerDrag = dragObject;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {

        canvasGroup.blocksRaycasts = true;

        GameObject targetObj = eventData.pointerEnter;
        
        if(targetObj != null && targetObj.TryGetComponent(out DropZone dropZone))
        {
            PanelType targetPanel = dropZone.panelType;

            if(targetPanel != cardDisplay.currentPanel)
            {
                CardManager.Instance.TransferCard(cardDisplay.cardID, cardDisplay.currentPanel, targetPanel);
            }
        }

        Destroy(gameObject);
    }
}
