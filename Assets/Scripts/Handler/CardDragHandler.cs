using UnityEngine;
using UnityEngine.EventSystems;

public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public CardDisplay cardDisplay;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

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
        GameObject targetObj = eventData.pointerEnter;

        if (targetObj == null || !targetObj.TryGetComponent<DropZone>(out _))
        {
            Destroy(gameObject);
        }
    }
}
