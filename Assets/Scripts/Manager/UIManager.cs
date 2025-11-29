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

    [Header("Button")]
    public Button StartButton = null;
    public Button SaveDeckButton = null;

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
        SaveDeckButton.onClick.AddListener(() => GameManager.DeckManager.SaveDeck());
        StartButton.onClick.AddListener(() =>
        {
            GotoBattleScene();
        });
    }

    public void GotoBattleScene()
    {
        SceneManager.LoadScene("BattleScene");
    }
}
