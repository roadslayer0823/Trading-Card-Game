using System;
using System.Collections.Generic;
using UnityEngine;

public static class EffectFactory
{
    private static readonly Dictionary<string, Func<EffectBase>> _creators = new();

    static EffectFactory()
    {
        RegisterAllEffects();
    }

    private static void RegisterAllEffects()
    {
        _creators["Damage"] = () => new DamageEffect();
        _creators["Heal"] = () => new HealingEffect();
        _creators["Status"] = () => new StatusEffect();
        _creators["Buff"] = () => new BuffEffect();
        _creators["DamageReduction"] = () => new DamageReductionEffect();
        _creators["LifeSteal"] = () => new LifeStealEffect();
        _creators["DrawCard"] = () => new DrawCardEffect();
    }

    // Factory method to create effects based on type
    public static EffectBase CreateEffect(string effectType)
    {
        if (string.IsNullOrEmpty(effectType))
        {
            return null;
        }

        if(_creators.TryGetValue(effectType, out var creator))
        {
            return creator();
        }

        return null;
    }

    public static void Register(string effectType, Func<EffectBase> creator)
    {
        if (_creators.ContainsKey(effectType))
        {
            Debug.LogWarning($"效果 {effectType} 已存在，將被覆蓋");
        }

        _creators[effectType] = creator;
    }
}
