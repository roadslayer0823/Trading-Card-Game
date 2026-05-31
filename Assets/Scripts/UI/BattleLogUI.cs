using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class BattleLogUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Transform content;
    [SerializeField] private GameObject logEntryPrefab;
    [SerializeField] private Image backgroundImage;

    [Header("Settings")]
    [SerializeField] private int maxLogEntries = 50;

    private List<GameObject> logEntries = new List<GameObject>();

    public void Initialize()
    {
         if(backgroundImage != null)
        {
            EventTrigger trigger = backgroundImage.gameObject.GetComponent<EventTrigger>();
            if (trigger == null) trigger = backgroundImage.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            entry.callback.AddListener((data) => { CloseBattleLog(); });
            trigger.triggers.Add(entry);
        }
    }
    // Start is called before the first frame update
    public void AddLog(string message, Color textColor = default)
    {
        if(logEntryPrefab == null || content == null) return;

        GameObject entryObj = Instantiate(logEntryPrefab, content);
        TMP_Text textComponent = entryObj.GetComponentInChildren<TMP_Text>();

        if(textComponent != null)
        {
            textComponent.text = message;
            if(textColor != default) textComponent.color = textColor;
        }

        logEntries.Add(entryObj);

        if(logEntries.Count > maxLogEntries)
        {
            Destroy(logEntries[0]);
            logEntries.RemoveAt(0);
        }

        Canvas.ForceUpdateCanvases();
        if(scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    public void ClearLog()
    {
        foreach(var entry in logEntries)
        {
            Destroy(entry);
        }
        logEntries.Clear();
    }

    public void ShowBattleLog()
    {
        this.gameObject.SetActive(true);
    }

    public void CloseBattleLog()
    {
        this.gameObject.SetActive(false);
    }
}
