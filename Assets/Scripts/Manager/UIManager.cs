using System.Collections;
using System.Collections.Generic;
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
    public Button startGameButton = null;

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
    }

    public void OpenDeckSelectionPanel()
    {
        deckSelectionUI.gameObject.SetActive(true);
    }

    public void GotoMainScene()
    {
        SceneManager.LoadScene("MainScene");
    }
}
