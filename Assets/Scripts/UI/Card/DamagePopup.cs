using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    [SerializeField] private TMP_Text damageText;

    public void Show(int damage, Vector3 startPosition)
    {
        transform.position = startPosition;
        damageText.text = "-" + damage;

        LeanTween.moveY(gameObject, startPosition.y + 90f, 0.85f).setEase(LeanTweenType.easeOutQuad);
        LeanTween.alphaCanvas(GetComponent<CanvasGroup>(), 0f, 0.8f).setEase(LeanTweenType.easeOutQuad).setOnComplete(() => Destroy(gameObject));
    }
}
