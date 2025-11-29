using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public DeckManager DeckManager;
    public CardManager CardManager;
    public UIManager UIManager;
 
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void Start()
    {
        if (DeckManager != null) DeckManager.Initialize();
        if (UIManager != null) UIManager.Initialize();
        if (CardManager != null) CardManager.Initialize();
    }
}
