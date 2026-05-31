using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleLogManager : MonoBehaviour
{
    public static BattleLogManager Instance;

    [SerializeField] private BattleLogUI battleLogUI;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LogDamage(string attacker, string target, int damage)
    {
        battleLogUI.AddLog($"<color=white>{attacker}</color> dealt <color=red>{damage}</color> damage to <color=white>{target}</color>.", Color.white);
    }

    public void LogHeal(string target, int heal)
    {
        battleLogUI.AddLog($"<color=white>{target}</color> restored <color=green>{heal}</color> HP.", Color.white);
    }

    public void LogElementReaction(string message)
    {
        battleLogUI.AddLog(message, new Color(0.3f, 0.7f, 1f));
    }

    public void LogStatus(string message)
    {
        battleLogUI.AddLog(message, new Color(1f, 0.8f, 0.2f));
    }

    public void LogGeneral(string message)
    {
        battleLogUI.AddLog(message);
    }
}
