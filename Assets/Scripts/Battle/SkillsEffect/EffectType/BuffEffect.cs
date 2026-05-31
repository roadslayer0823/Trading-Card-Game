using UnityEngine;
using UnityEngine.UI;

public class BuffEffect : EffectBase
{
    public override void ApplyEffect(CardDisplay source, EffectContext context)
    {
        if (context.target?.type != EffectTargetType.Card || context.target.card == null) return;

        CardDisplay targetCard = context.target.card;
        string statType = context.statusName;
        int value = context.value;

        if (statType == "ATK")
        {
            targetCard.tempAtkBuff += value;
            targetCard.RefreshAtk();
            if (BattleLogManager.Instance != null)
            {
                string sourceName = source != null ? source.cardName : "Spell";
                BattleLogManager.Instance.LogStatus($"<color=white>{targetCard.cardName}</color> ATK +{value} (Source: <color=yellow>{sourceName}</color>).");
            }
        }
        else if (statType == "HP")
        {
            targetCard.tempHpBuff += value;
            targetCard.maxHpPoint += value;
            targetCard.Heal(value);

            // 額外更新顯示（顯示加成）
            if (targetCard.hpText != null)
            {
                targetCard.hpText.text = $"{targetCard.hpPoint} (+{targetCard.tempHpBuff})";
                LayoutRebuilder.ForceRebuildLayoutImmediate(targetCard.hpText.GetComponentInParent<RectTransform>());
            }

            if (BattleLogManager.Instance != null)
            {
                string sourceName = source != null ? source.cardName : "Spell";
                BattleLogManager.Instance.LogStatus($"<color=white>{targetCard.cardName}</color> max HP +{value} (Source: <color=yellow>{sourceName}</color>).");
            }
        }
        else
        {
            if (BattleLogManager.Instance != null)
            {
                BattleLogManager.Instance.LogGeneral($"Warning: Unknown or invalid buff type: {statType} (rawValue: {context.rawValue}).");
            }
        }
    }
}
