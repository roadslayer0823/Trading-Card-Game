using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventTrigger))]
public class CardHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover Effect Setting")]
    public float scaleFactor = 1.2f;
    public float liftAmount = 50f;
    public float animationTime = 0.15f;
    public CardDisplay cardDisplay;

    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Canvas canvas;

    private LTDescr currentTween;

    private void Awake()
    {
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (cardDisplay.currentZone != CardZone.Hand) return;
        if (currentTween != null) LeanTween.cancel(currentTween.uniqueId);

        float canvasScale = canvas != null ? canvas.scaleFactor : 1f;
        Vector3 targetPos = originalPosition + new Vector3(0, liftAmount * canvasScale, 0);

        currentTween = LeanTween.value(gameObject, 0f, 1f, animationTime).setOnUpdate((float t) =>
        {
            transform.localScale = Vector3.Lerp(originalScale, originalScale * scaleFactor, t);
            transform.localPosition = Vector3.Lerp(originalPosition, targetPos, t);
        })
        .setEase(LeanTweenType.easeOutQuad);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (cardDisplay.currentZone != CardZone.Hand) return;
        if (currentTween != null)
        {
            LeanTween.cancel(currentTween.uniqueId);
            currentTween = null;
        } 

        currentTween = LeanTween.value(gameObject, 0f, 1f, animationTime).setOnUpdate((float t) =>
        {
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, t);
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, t);
        })
        .setEase(LeanTweenType.easeInOutQuad).setOnComplete(() => currentTween = null);
    }

    public void DisableHover()
    {
        LeanTween.cancel(gameObject);
        transform.localScale = originalScale;
        transform.localPosition = originalPosition;
    }
}
