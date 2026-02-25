using UnityEngine;
using TMPro;

public enum CardPlayError
{
    None,
    InsufficientMana,
    NoValidTarget,
    InvalidZone,
    Untargetable
}

public class FeedbackManager : MonoBehaviour
{
    public static FeedbackManager Instance;

    [Header("UI Reference")]
    public TextMeshProUGUI feedbackText;
    public CanvasGroup feedbackCanvasGroup;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (feedbackCanvasGroup != null) feedbackCanvasGroup.alpha = 0f;
    }

    /// <summary>
    /// 顯示失敗提示文字，自動淡出
    /// </summary>
    /// <param name="error">錯誤類型</param>
    /// <param name="duration">停留時間（秒）</param>

    public void ShowFeedback(CardPlayError error, float duration = 1f)
    {
        if (error == CardPlayError.None) return;

        string message = error switch
        {
            CardPlayError.InsufficientMana => "Insufficient Mana",
            CardPlayError.NoValidTarget => "No Valid Target",
            CardPlayError.InvalidZone => "Invalid Zone",
            CardPlayError.Untargetable => "Untargetable",
            _ => "Unable to play the card"
        };

        feedbackText.text = message;
        LeanTween.cancel(feedbackCanvasGroup.gameObject);
        feedbackCanvasGroup.alpha = 1f;

        LeanTween.alphaCanvas(feedbackCanvasGroup, 0f, 0.5f).setDelay(duration).setEase(LeanTweenType.easeInOutQuad);
        
    }
}
