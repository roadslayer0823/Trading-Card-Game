using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthPointHandler : MonoBehaviour
{
    [Header("UI Reference")]
    public Image healthBar;
    public TMP_Text healthText;
    public Owner owner = Owner.None;

    [HideInInspector] public int maxHealth;
    [HideInInspector] public int currentHealth;
 

    public void Initialize(int hp)
    {
        maxHealth = hp;
        currentHealth = hp;
        UpdateUI(maxHealth, currentHealth);
    }
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;
        Debug.Log($"{gameObject.name} took {damage} damage. Current HP: {currentHealth}/{maxHealth}");
        UpdateUI(maxHealth, currentHealth);
        if (currentHealth == 0)
        {
            Debug.Log($"{owner} lose");
        }
    }

    public void Heal(int heal)
    {
        currentHealth = Mathf.Min(currentHealth + heal, maxHealth);
        UpdateUI(maxHealth, currentHealth);
        Debug.Log($"{gameObject.name} healed {heal} HP. Current HP: {currentHealth}/{maxHealth}");
    }

    private void UpdateUI(int maxHealth, int currentHealth)
    {
        healthBar.fillAmount = (float)currentHealth / maxHealth;
        healthText.text = $"{currentHealth}/{maxHealth}";
    }
}
