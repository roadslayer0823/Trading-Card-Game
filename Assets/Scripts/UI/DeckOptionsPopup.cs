using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class DeckOptionsPopup : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private RectTransform popupRoot;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TMP_Text deckNameText;
    [SerializeField] private Button editButton;
    [SerializeField] private Button deleteButton;

    [Header("Animation Settings")]
    [SerializeField] private float slideDuration = 0.35f;
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private Vector2 slideOffset = new Vector2(400f, 0f);

    private string currentDeckName;
    private Vector2 originalAnchoredPostion;

    public void Initialize()
    {
        originalAnchoredPostion = popupRoot.anchoredPosition;
        if(backgroundImage != null)
        {
            EventTrigger trigger = backgroundImage.gameObject.GetComponent<EventTrigger>();
            if (trigger == null) trigger = backgroundImage.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            entry.callback.AddListener((data) => { ClosePopup(); });
            trigger.triggers.Add(entry);
        }
    }

    public void Show(string deckName)
    {
        currentDeckName = deckName;
        deckNameText.text = deckName;

        editButton.onClick.RemoveAllListeners();
        deleteButton.onClick.RemoveAllListeners();

        editButton.onClick.AddListener(OnEditClicked);
        deleteButton.onClick.AddListener(OnDeleteClicked);

        popupRoot.gameObject.SetActive(true);
        backgroundImage.gameObject.SetActive(true);
        PlayShowAnimation();
    }

    private void PlayShowAnimation()
    {
        popupRoot.anchoredPosition = originalAnchoredPostion + slideOffset;
        popupRoot.localScale = Vector3.one;
        CanvasGroup cg = popupRoot.GetComponent<CanvasGroup>();
        if (cg == null) cg = popupRoot.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        LeanTween.move(popupRoot, originalAnchoredPostion, slideDuration).setEase(LeanTweenType.easeOutQuad);
        LeanTween.alphaCanvas(cg, 1f, fadeDuration).setEase(LeanTweenType.easeOutQuad);
    }

    private void OnEditClicked()
    {
        ClosePopup();
        UIManager.Instance.OpenDeckBuilderPanel();
        DeckBuilderManager.Instance.LoadDeckForEdit(currentDeckName);
    }

    private void OnDeleteClicked()
    {
        ShowDeleteConfirmation();
    }

    private void ClosePopup()
    {
        popupRoot.gameObject.SetActive(false);
        backgroundImage.gameObject.SetActive(false);
    }

    private void ShowDeleteConfirmation()
    {
        UIManager.Instance.ShowConfirmationDialog("Are you sure to delete this deck", OnDeleteConfirmed);
    }

    private void OnDeleteConfirmed()
    {
        bool success = DeckManager.Instance.DeleteDeckByName(currentDeckName);
        if (success)
        {
            Debug.Log($"卡组已删除：{currentDeckName}");
            DeckSelectionManager.Instance.RefreshDeckList();
            ClosePopup();
        }
    }
}
