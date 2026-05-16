using System.Collections;
using System.Collections.Generic;
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
        DontDestroyOnLoad(gameObject);
    }

    public void Initialize()
    {
        startGameButton.onClick.AddListener(() => OpenDeckSelectionPanel());
        confirmationPanel.Initialize();
        deckOptionPanel.Initialize();
        deckFilterPanel.Initialize();

        deckFilterPanel.gameObject.SetActive(false);
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
        SceneManager.LoadScene("MainScene");
    }
}
