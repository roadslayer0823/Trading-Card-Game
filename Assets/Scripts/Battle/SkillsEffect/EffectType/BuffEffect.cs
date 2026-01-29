using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class BuffEffect : EffectBase
{
    public override void ApplyEffect(CardDisplay source, EffectContext context)
    {
        if (context.target?.type != EffectTargetType.Card || context.target.card == null) return;

        string raw = context.rawValue;
        var atkMatch = Regex.Match(raw, @"(Damage|ATK)\+(\d+)", RegexOptions.IgnoreCase);
        if (atkMatch.Success)
        {
            int value = int.Parse(atkMatch.Groups[2].Value);
            CardDisplay targetCard = context.target.card;
            targetCard.tempAtkBuff += value;
            targetCard.atkText.text = $"ATK: {targetCard.atkPoint + targetCard.tempAtkBuff}";
            Debug.Log($"[Buff] {targetCard.cardName} 攻击力 +{value} (当前: {targetCard.atkPoint + targetCard.tempAtkBuff})");
            return;
        }

        var hpMatch = Regex.Match(raw, @"HP\+(\d+)", RegexOptions.IgnoreCase);
        if(hpMatch.Success)
        {
            int value = int.Parse(hpMatch.Groups[1].Value);
            CardDisplay targetCard = context.target.card;
            targetCard.tempHpBuff += value;
            targetCard.maxHpPoint += value;
            targetCard.hpPoint = targetCard.maxHpPoint;
            targetCard.hpText.text = $"HP: {targetCard.hpPoint} (+{targetCard.tempHpBuff})";  // UI 显示加成
            LayoutRebuilder.ForceRebuildLayoutImmediate(targetCard.hpText.GetComponentInParent<RectTransform>());
            Debug.Log($"[Buff HP] {targetCard.cardName} HP +{value} (新上限 {targetCard.maxHpPoint})");
            return;
        }
    }
}
