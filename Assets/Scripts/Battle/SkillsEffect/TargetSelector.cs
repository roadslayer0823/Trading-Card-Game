using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;

public static class TargetSelector
{
    public static List<EffectTarget> GetTargets(string targetType, Owner owner)
    {
        List<EffectTarget> results = new();
        string baseType = Regex.Replace(targetType, @"\([^)]*\)", "");
        int number = ExtractNumberInParentheses(targetType);

        List<CardDisplay> enemyCards = BattleManager.Instance.GetEnemyUnits(owner);
        List<CardDisplay> playerCards = BattleManager.Instance.GetAllyUnits(owner);
        HealthPointHandler enemyLeader = owner == Owner.Player ? BattleManager.Instance.enemyHealth : BattleManager.Instance.playerHealth;
        HealthPointHandler playerLeader = owner == Owner.Player ? BattleManager.Instance.playerHealth : BattleManager.Instance.enemyHealth;

        switch (baseType)
        {
            case "SingleEnemy":
                if(enemyCards.Count > 0)
                {
                    var pick = enemyCards[Random.Range(0, enemyCards.Count)];
                    results.Add(EffectTarget.FromCard(pick));
                }
                break;

            case "AllEnemies":
                foreach (var item in enemyCards)
                {
                    results.Add(EffectTarget.FromCard(item));
                }
                break;

            case "RandomEnemies":
            case "Enemies":
                {
                    int count = Mathf.Min(number, enemyCards.Count);
                    var pool = new List<CardDisplay>(enemyCards);
                    for(int i = 0; i < count && pool.Count > 0; i++)
                    {
                        int index = Random.Range(0, pool.Count);
                        results.Add(EffectTarget.FromCard(pool[index]));
                        pool.RemoveAt(index);
                    }
                }
                break;

            case "AllAllies":
                foreach (var item in playerCards)
                {
                    results.Add(EffectTarget.FromCard(item));
                }
                break;

            case "Self":
                results.Add(EffectTarget.FromLeader(playerLeader));
                break;

            case "Leader":
                results.Add(EffectTarget.FromLeader(enemyLeader));
                break;

            case "All":
                // 全场单位（both sides）
                foreach (var c in playerCards) results.Add(EffectTarget.FromCard(c));
                foreach (var c in enemyCards) results.Add(EffectTarget.FromCard(c));
                break;

            default:
                Debug.LogWarning($"[TargetSelector] 未识别的 targetType: {targetType}");
                break;

        }
        Debug.Log($"[TargetSelector] {targetType} -> 选中 {results.Count} 个目标");
        return results;
    }

    private static int ExtractNumberInParentheses(string input)
    {
        var m = Regex.Match(input, @"\((\d+)\)");
        return m.Success ? int.Parse(m.Groups[1].Value) : 1;
    }
}
