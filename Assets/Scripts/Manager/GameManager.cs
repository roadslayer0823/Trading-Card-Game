using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public DeckManager DeckManager;
    public DeckBuilderManager DeckBuilderManager;
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
        if (DeckBuilderManager != null) DeckBuilderManager.Initialize();
        // UIManager self-initializes via its own Start() — no call needed here.
    }
}
