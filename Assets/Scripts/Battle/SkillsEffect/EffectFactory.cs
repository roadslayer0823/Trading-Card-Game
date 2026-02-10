using System;

public static class EffectFactory
{
    // Factory method to create effects based on type
    public static EffectBase CreateEffect(string effectType)
    {
        return effectType switch 
        {
            "Damage" => new DamageEffect(),
            "Heal" => new HealingEffect(),
            "Status" => new StatusEffect(),
            "Buff" => new BuffEffect(),
            "DamageReduction" => new DamageReductionEffect(),
            "LifeSteal" => new LifeStealEffect(),
            "DrawCard" => new DrawCardEffect(),
            _ => throw new ArgumentException($"Unknown effect type: {effectType}")
        };
    }
}
