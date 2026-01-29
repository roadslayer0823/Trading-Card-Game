using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffect : EffectBase
{
    public override void ApplyEffect(CardDisplay source, EffectContext context)
    {
        if (context.target.type != EffectTargetType.Card || context.target.card == null)
        {
            Debug.LogWarning("[StatusEffect] 目标不是卡牌，跳过");
            return;
        }

        CardDisplay actualTarget = context.target.card;  // ← 这才是被施加状态的卡

        string raw = context.rawValue;
        var match = Regex.Match(raw, @"(\w+)\((\d+)\)");
        if (!match.Success)
        {
            Debug.LogWarning($"[StatusEffect] 格式错误: {raw}");
            return;
        }

        string statusType = match.Groups[1].Value;
        int duration = int.Parse(match.Groups[2].Value);

        // 正确日志：谁 → 对谁 → 施加了什么
        Debug.Log($"[状态效果] {source.cardName} → 对 {actualTarget.cardName} 施加 {statusType}({duration}回合)");

        switch (statusType)
        {
            case "Freeze":
                actualTarget.ApplyFreeze(duration);
                break;
            case "Stun":
                actualTarget.ApplyStun(duration);
                break;
            case "Spread":
                CardDisplay sourceTarget = context.target.card;
                if(sourceTarget == null) break;
                List<CardDisplay> spreadTargets = TargetSelector.GetSpreadTargets(sourceTarget.owner, 2, sourceTarget);
                if (spreadTargets.Count == 0)
                {
                    Debug.Log("[Spread] 没有可传播的目标");
                    break;
                }
                
                foreach(var spreadTarget in spreadTargets)
                {
                    if(spreadTarget == sourceTarget) continue;
                    foreach (var tag in sourceTarget.elementTags)
                    {
                        spreadTarget.AddElementTag(tag);
                    }
                    Debug.Log($"[元素传播] {sourceTarget.cardName} 的元素标签传播给了 {spreadTarget.cardName}");
                }
                break;
            case "Untargetable":
                actualTarget.ApplyUntargetable(duration);
                break;
            default:
                Debug.Log($"[StatusEffect] 未知状态类型: {statusType}");
                break;
        }
    }
}