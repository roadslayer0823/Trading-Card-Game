using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ConfirmationDialog : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialogRoot;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private Action onConfirmCallback;
    private Action onCancelCallback;

    public void Initialize()
    {
        if (confirmButton != null) confirmButton.onClick.AddListener(OnConfirmClicked);
        if (cancelButton != null) cancelButton.onClick.AddListener(OnCancelClicked);
    }

    /// <summary>
    /// 显示确认弹窗
    /// </summary>
    /// <param name="message">提示文字</param>
    /// <param name="onConfirm">点击「确认」时执行的回调</param>
    /// <param name="onCancel">点击「取消」时执行的回调（可选）</param>
    public void Show(string message, Action onConfirm, Action onCancel = null)
    {
        messageText.text = message;
        onConfirmCallback = onConfirm;
        onCancelCallback = onCancel;

        dialogRoot.SetActive(true);
    }

    private void OnConfirmClicked()
    {
        onConfirmCallback?.Invoke();
        CloseDialog();
    }

    private void OnCancelClicked()
    {
        onCancelCallback?.Invoke();
        CloseDialog();
    }

    public void CloseDialog()
    {
        dialogRoot.SetActive(false);

        // 清空回调，防止内存泄漏
        onConfirmCallback = null;
        onCancelCallback = null;
    }
}
