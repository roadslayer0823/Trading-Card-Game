using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.EventSystems;

public class DeckOptionsPopup : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private GameObject popupRoot;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TMP_Text deckNameText;
    [SerializeField] private Button editButton;
    [SerializeField] private Button deleteButton;

    private string currentDeckName;

    public void Initialize()
    {
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

        popupRoot.SetActive(true);
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
        popupRoot.SetActive(false);
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
