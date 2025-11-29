using System.Text.RegularExpressions;
using UnityEngine;

public class StatusEffect : EffectBase
{
    public override void ApplyEffect(CardDisplay target, EffectContext context)
    {
        string raw = context.rawValue;
        string pattern = @"(\w+)\((\d+)\)";

        var match = Regex.Match(raw, pattern);

        if (!match.Success)
        {
            Debug.Log("did not work");
            return;
        }

        string statusType = match.Groups[1].Value;
        int duration = int.Parse(match.Groups[2].Value);

        Debug.Log($"[StatusEffect] 对 {target.cardName} 施加状态 {statusType} ({duration}回合)");

        switch (statusType)
        {
            case "Freeze":
                target.ApplyFreeze(duration);
                break;
            default:
                Debug.Log($"[StatusEffect] 未知状态类型: {statusType}");
                break;
        }

    }
}
