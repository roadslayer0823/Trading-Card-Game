using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    public PanelType panelType;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        if (dropped == null) return;

        CardDisplay droppedCard = dropped.GetComponent<CardDisplay>();
        if (droppedCard == null) return;

        PanelType fromPanel = droppedCard.currentPanel;
        string cardID = droppedCard.cardID;

        if (fromPanel == PanelType.Library && panelType == PanelType.Deck && CardManager.Instance.currentDeckCount >= CardManager.Instance.maxDeckCount)
        {
            Debug.Log("Deck is full!");
            return;
        }

        Debug.Log($"Dropped card ID: {cardID} from {fromPanel} to {panelType}");

        CardManager.Instance.TransferCard(cardID, fromPanel, panelType);
        Destroy(dropped);
    }
}
