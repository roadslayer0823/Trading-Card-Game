using UnityEngine;
public class EffectContext
{
    public Owner sourceOwner;
    public EffectTarget target;
    public int value;
    public string rawValue;
    public string statusName;
    public int duration;
    public EffectContext(Owner sourceOwner, EffectTarget target, int value = 0, string statusName = "", int duration = 0, string rawValue = "")
    {
        this.sourceOwner = sourceOwner;
        this.target = target;
        this.value = value;
        this.rawValue = rawValue;
        this.statusName = statusName;
        this.duration = duration;

        if (target != null && target.type == EffectTargetType.Card)
        {
            if (target.card == null)
            {
                Debug.LogError($"[EffectContext] 嚴重錯誤！target.type=Card 但 target.card == null！sourceOwner={sourceOwner}");
            }
            else
            {
                Debug.Log($"[EffectContext] 建構成功：target.card = {target.card.cardName}");
            }
        }
    }
}
