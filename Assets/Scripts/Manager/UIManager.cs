using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [Header("Manager")]
    public GameManager GameManager = null;

    [Header("UI Reference")]
    public GameObject deckSelectionUI = null;
    public GameObject deckBuilderUI = null;
    public Button startGameButton = null;
    public ConfirmationDialog confirmationPanel = null;
    public DeckOptionsPopup deckOptionPanel = null;
    public DeckFilterPanel deckFilterPanel = null;

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // No DontDestroyOnLoad — UIManager is main-scene UI only.
        // It gets recreated each time MainScene loads.
    }

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (startGameButton != null)
            startGameButton.onClick.AddListener(() => OpenDeckSelectionPanel());
        if (confirmationPanel != null)
            confirmationPanel.Initialize();
        if (deckOptionPanel != null)
            deckOptionPanel.Initialize();
        if (deckFilterPanel != null)
            deckFilterPanel.Initialize();

        if (deckFilterPanel != null)
            deckFilterPanel.gameObject.SetActive(false);
        if (deckBuilderUI != null)
            deckBuilderUI.SetActive(false);
    }

    public void ShowConfirmationDialog(string message, Action action)
    {
        confirmationPanel.Show(message, action);
    }

    public void OpenDeckSelectionPanel()
    {
        deckSelectionUI.gameObject.SetActive(true);
    }

    public void OpenDeckBuilderPanel()
    {
        deckBuilderUI.gameObject.SetActive(true);
    }

    public void GotoMainScene()
    {
        // UIManager will be destroyed when BattleScene unloads.
        // The fresh MainScene will create a new UIManager and call Start() -> Initialize().
        SceneManager.LoadScene("MainScene");
    }
}
