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

        Debug.Log($"[BuffEffect] 收到 類型: {statType}, 數值: {value}");

        if (statType == "ATK")
        {
            targetCard.tempAtkBuff += value;
            targetCard.RefreshAtk();
            Debug.Log($"[Buff] {targetCard.cardName} 攻擊力 +{value} (目前: {targetCard.currentAtkPoint + targetCard.tempAtkBuff})");
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

            Debug.Log($"[Buff HP] {targetCard.cardName} HP 上限 +{value} (新上限: {targetCard.maxHpPoint})");
        }
        else
        {
            Debug.LogWarning($"[BuffEffect] 未知或無效的 Buff 類型: {statType}, rawValue: {context.rawValue}");
        }
    }
}
