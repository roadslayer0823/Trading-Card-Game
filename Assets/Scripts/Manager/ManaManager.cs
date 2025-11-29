using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

public enum Owner
{
    None, Player, Enemy
}

public class ManaManager : MonoBehaviour
{
    public static ManaManager Instance;
    public static event Action OnManaChanged;

    [Header("UI Reference")]
    public TextMeshProUGUI playerManaText;
    public TextMeshProUGUI enemyManaText;
    public Image playerManaBar;
    public Image enemyManaBar;

    private int manaCapacity = 10;
    private Dictionary<Owner, int> currentMana = new();
    private Dictionary<Owner, int> maxMana = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        maxMana[Owner.Player] = 0;
        currentMana[Owner.Player] = 0;

        maxMana[Owner.Enemy] = 0;
        currentMana[Owner.Enemy] = 0;
    }

    public void IncreasePlayerMana()
    {
        StartTurn(Owner.Player);
    }

    public void StartTurn(Owner owner)
    {
        if (maxMana[owner] < manaCapacity) maxMana[owner]++;
        currentMana[owner] = maxMana[owner];
        UpdateManaUI(owner);
        NotifyManaChange();
    }
    public void RestoreMana(Owner owner, int amount) 
    {
        currentMana[owner] = Mathf.Min(currentMana[owner] + amount, maxMana[owner]);
        UpdateManaUI(owner);
        NotifyManaChange();
    }

    private void UpdateManaUI(Owner owner)
    {
       TextMeshProUGUI currentTextObject = owner == Owner.Player ? playerManaText : enemyManaText;
       Image currentManaBar = owner == Owner.Player ? playerManaBar : enemyManaBar;
       currentTextObject.text = $"{currentMana[owner]}/{maxMana[owner]}";
       currentManaBar.fillAmount = (float)currentMana[owner] / maxMana[owner];
    }

    private void NotifyManaChange() 
    {
        OnManaChanged?.Invoke();
    }

    public bool SpendMana(Owner owner, int amount)
    {
        if (currentMana[owner] < amount) return false;
        currentMana[owner] -= amount;
        UpdateManaUI(owner);
        NotifyManaChange();
        return true;
    }
    public bool CanAfford(int cost, Owner owner)
    {
        return currentMana[owner] >= cost;
    }
}
