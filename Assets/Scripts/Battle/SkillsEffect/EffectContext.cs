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
    }
}
