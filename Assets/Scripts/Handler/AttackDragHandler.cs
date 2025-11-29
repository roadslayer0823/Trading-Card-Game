using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;

[RequireComponent(typeof(CardDisplay))]
public class AttackDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private CardDisplay attackerCard;
    private UILineRenderer uiLineRenderer;
    private Vector2 startPos;

    void Awake()
    {
        attackerCard = GetComponent<CardDisplay>();

        uiLineRenderer = GameObject.Find("AttackLine").GetComponent<UILineRenderer>();
        uiLineRenderer.Points = new Vector2[0];
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(attackerCard.currentZone != CardZone.Field || !attackerCard.isAttack) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(uiLineRenderer.rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localStart)) 
        {
            startPos = localStart;
            uiLineRenderer.Points = new Vector2[] { startPos, startPos };
            uiLineRenderer.SetAllDirty();
            Debug.Log($"[BeginDrag] StartPos: {this.startPos}");
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (uiLineRenderer.Points == null || uiLineRenderer.Points.Length < 2) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(uiLineRenderer.rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
        {
            uiLineRenderer.Points = new Vector2[] { startPos, localPoint};
            uiLineRenderer.SetAllDirty();

            Debug.Log($"[OnDrag] Mouse: {eventData.position}, Local: {localPoint}");
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        uiLineRenderer.Points = new Vector2[0];

        Debug.Log("[OnEndDrag] End drag triggered");

        if (eventData.pointerEnter != null)
        {
            Debug.Log($"[OnEndDrag] PointerEnter: {eventData.pointerEnter.name}");

            CardDisplay targetCard = eventData.pointerEnter.GetComponentInParent<CardDisplay>();
            if (targetCard != null)
            {
                Debug.Log($"[OnEndDrag] TargetCard found: {targetCard.name}, Owner: {targetCard.owner}, Zone: {targetCard.currentZone}");
                if (targetCard.owner != attackerCard.owner && targetCard.currentZone == CardZone.Field)
                {
                    Debug.Log("[OnEndDrag] Attacking enemy card");
                    BattleManager.Instance.Attack(attackerCard, targetCard);
                    return;
                }
            }

            HealthPointHandler targetHp = eventData.pointerEnter.GetComponentInParent<HealthPointHandler>();
            if (targetHp != null)
            {
                Debug.Log($"[OnEndDrag] TargetHP found. Owner: {targetHp.owner}, Attacker Owner: {attackerCard.owner}");
                if (targetHp.owner != attackerCard.owner)
                {
                    Debug.Log("[OnEndDrag] Attacking enemy HP directly!");
                    BattleManager.Instance.Attack(attackerCard, null);
                    return;
                }
            }
        }
        else
        {
            Debug.Log("[OnEndDrag] pointerEnter is NULL");
        }
    }
}
