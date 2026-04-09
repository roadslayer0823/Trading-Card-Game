using UnityEngine;
using UnityEngine.UI;

public class BuffEffect : EffectBase
{
    public override void ApplyEffect(CardDisplay source, EffectContext context)
    {
        if (context.target?.type != EffectTargetType.Card || context.target.card == null) return;

        CardDisplay targetCard = context.target.card;
        string raw = context.rawValue.Trim();

        Debug.Log($"[BuffEffect] 收到 rawValue: '{raw}'");

        if (raw.Contains("ATK") || raw.Contains("Damage"))
        {
            int value = ExtractNumber(raw);
            if (value != 0)
            {
                targetCard.tempAtkBuff += value;
                targetCard.RefreshAtk();
                Debug.Log($"[Buff] {targetCard.cardName} 攻擊力 +{value} (目前: {targetCard.currentAtkPoint + targetCard.tempAtkBuff})");
            }
        }
        else if (raw.Contains("HP"))
        {
            int value = ExtractNumber(raw);
            if(value != 0)
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

                LayoutRebuilder.ForceRebuildLayoutImmediate(targetCard.hpText.GetComponentInParent<RectTransform>());
                Debug.Log($"[Buff HP] {targetCard.cardName} HP 上限 +{value} (新上限: {targetCard.maxHpPoint})");
            }
        }
        else
        {
            Debug.LogWarning($"[BuffEffect] 無法解析的 Buff 格式: {raw}");
        }
    }

    private int ExtractNumber(string raw)
    {
        string numStr = "";
        foreach(char c in raw)
        {
            if (char.IsDigit(c) || c == '-')
                numStr += c;
        }

        return int.TryParse(numStr, out int value) ? value : 0;
    }
}
