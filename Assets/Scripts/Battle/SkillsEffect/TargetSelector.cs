using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class TargetSelector
{
    private static readonly Dictionary<string, Func<string, Owner, EffectContext, CardDisplay, List<EffectTarget>>> _targetHandlers = new();

    static TargetSelector()
    {
        RegisterAllTargetHandlers();
    }

    private static void RegisterAllTargetHandlers()
    {
        _targetHandlers["SingleEnemy"] = HandleSingleEnemy;
        _targetHandlers["AllEnemies"] = HandleAllEnemies;
        _targetHandlers["RandomEnemy"] = HandleRandomEnemy;
        _targetHandlers["RandomEnemies"] = HandleRandomEnemies;
        _targetHandlers["Enemies"] = HandleRandomEnemies;   // 同上
        _targetHandlers["HitTarget"] = HandleHitTarget;
        _targetHandlers["AllAllies"] = HandleAllAllies;
        _targetHandlers["NearbyAllies"] = HandleNearbyAllies;
        _targetHandlers["Self"] = HandleSelf;
        _targetHandlers["AreaAroundSelf"] = HandleAreaAroundSelf;
        _targetHandlers["Leader"] = HandleLeader;
        _targetHandlers["All"] = HandleAll;
        _targetHandlers["SingleAlly"] = HandleSingleAlly;
    }

    public static List<EffectTarget> GetTargets(string targetType, Owner owner, EffectContext context = null, CardDisplay sourceCard = null)
    {
        if (string.IsNullOrEmpty(targetType))
            return new List<EffectTarget>();

        // 處理帶數字的格式，例如 "RandomEnemies(2)"
        string baseType = System.Text.RegularExpressions.Regex.Replace(targetType, @"\([^)]*\)", "");
        int number = ExtractNumberInParentheses(targetType);

        if (_targetHandlers.TryGetValue(baseType, out var handler))
        {
            var results = handler(targetType, owner, context, sourceCard);

            // 如果有指定數量，則限制數量（RandomEnemies 等會自己處理，這裡是保險）
            if (number > 1 && results.Count > number)
                results = results.GetRange(0, number);

            Debug.Log($"[TargetSelector] {targetType} -> 选中 {results.Count} 个目標");
            return results;
        }

        Debug.LogWarning($"[TargetSelector] 未識別的目標類型: {targetType}");
        return new List<EffectTarget>();
    }

    // ==================== Target Selector Function ====================

    private static List<EffectTarget> HandleSingleEnemy(string type, Owner owner, EffectContext ctx, CardDisplay source)
    {
        var enemies = BattleManager.Instance.GetEnemyUnits(owner).Where(c => !c.IsUntargetable()).ToList();
        if (enemies.Count == 0) return new List<EffectTarget>();

        var pick = enemies[UnityEngine.Random.Range(0, enemies.Count)];
        return new List<EffectTarget> { EffectTarget.FromCard(pick) };
    }

    private static List<EffectTarget> HandleAllEnemies(string type, Owner owner, EffectContext ctx, CardDisplay source)
    {
        return BattleManager.Instance.GetEnemyUnits(owner)
            .Where(c => !c.IsUntargetable())
            .Select(EffectTarget.FromCard)
            .ToList();
    }

    private static List<EffectTarget> HandleRandomEnemy(string type, Owner owner, EffectContext ctx, CardDisplay source)
    {
        var valid = BattleManager.Instance.GetEnemyUnits(owner).Where(c => !c.IsUntargetable()).ToList();
        if (valid.Count == 0) return new List<EffectTarget>();

        var pick = valid[UnityEngine.Random.Range(0, valid.Count)];
        return new List<EffectTarget> { EffectTarget.FromCard(pick) };
    }

    private static List<EffectTarget> HandleRandomEnemies(string type, Owner owner, EffectContext ctx, CardDisplay source)
    {
        int count = ExtractNumberInParentheses(type);
        var valid = BattleManager.Instance.GetEnemyUnits(owner).Where(c => !c.IsUntargetable()).ToList();
        count = Mathf.Min(count, valid.Count);

        var pool = new List<CardDisplay>(valid);
        var results = new List<EffectTarget>();

        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int index = UnityEngine.Random.Range(0, pool.Count);
            results.Add(EffectTarget.FromCard(pool[index]));
            pool.RemoveAt(index);
        }
        return results;
    }

    private static List<EffectTarget> HandleHitTarget(string type, Owner owner, EffectContext ctx, CardDisplay source)
    {
        if (ctx?.attacker != null)
            return new List<EffectTarget> { EffectTarget.FromCard(ctx.attacker) };
        return new List<EffectTarget>();
    }

    private static List<EffectTarget> HandleAllAllies(string type, Owner owner, EffectContext ctx, CardDisplay source)
    {
        return BattleManager.Instance.GetAllyUnits(owner)
            .Where(c => !c.IsUntargetable())
            .Select(EffectTarget.FromCard)
            .ToList();
    }

    private static List<EffectTarget> HandleNearbyAllies(string type, Owner owner, EffectContext ctx, CardDisplay sourceCard)
    {
        if (sourceCard == null) return new List<EffectTarget>();

        var results = new List<EffectTarget>();
        var allies = BattleManager.Instance.GetAllyUnits(owner);
        FieldSlot selfSlot = sourceCard.GetComponentInParent<FieldSlot>();

        if (selfSlot == null) return results;

        foreach (var ally in allies)
        {
            var allySlot = ally.GetComponentInParent<FieldSlot>();
            if (allySlot != null && Mathf.Abs(allySlot.slotIndex - selfSlot.slotIndex) == 1)
            {
                results.Add(EffectTarget.FromCard(ally));
            }
        }
        return results;
    }

    private static List<EffectTarget> HandleSelf(string type, Owner owner, EffectContext ctx, CardDisplay sourceCard)
    {
        if (sourceCard != null)
            return new List<EffectTarget> { EffectTarget.FromCard(sourceCard) };
        return new List<EffectTarget>();
    }

    private static List<EffectTarget> HandleAreaAroundSelf(string type, Owner owner, EffectContext ctx, CardDisplay source)
    {
        var results = new List<EffectTarget>();
        results.AddRange(HandleAllAllies(type, owner, ctx, source));
        results.AddRange(HandleAllEnemies(type, owner, ctx, source));
        return results;
    }

    private static List<EffectTarget> HandleLeader(string type, Owner owner, EffectContext ctx, CardDisplay source)
    {
        var leader = owner == Owner.Player ? BattleManager.Instance.enemyHealth : BattleManager.Instance.playerHealth;
        return new List<EffectTarget> { EffectTarget.FromLeader(leader) };
    }

    private static List<EffectTarget> HandleAll(string type, Owner owner, EffectContext ctx, CardDisplay source)
    {
        var results = new List<EffectTarget>();
        results.AddRange(HandleAllAllies(type, owner, ctx, source));
        results.AddRange(HandleAllEnemies(type, owner, ctx, source));
        return results;
    }

    private static List<EffectTarget> HandleSingleAlly(string type, Owner owner, EffectContext ctx, CardDisplay source)
    {
        var allies = BattleManager.Instance.GetAllyUnits(owner).Where(c => !c.IsUntargetable()).ToList();
        if (allies.Count == 0) return new List<EffectTarget>();

        var pick = allies[UnityEngine.Random.Range(0, allies.Count)];
        return new List<EffectTarget> { EffectTarget.FromCard(pick) };
    }

    private static int ExtractNumberInParentheses(string input)
    {
        var m = System.Text.RegularExpressions.Regex.Match(input, @"\((\d+)\)");
        return m.Success ? int.Parse(m.Groups[1].Value) : 1;
    }

    public static List<CardDisplay> GetSpreadTargets(Owner owner, int count = 2, CardDisplay exclude = null)
    {
        var allies = BattleManager.Instance.GetAllyUnits(owner);
        if (exclude != null) allies.Remove(exclude);

        var selected = new List<CardDisplay>();
        count = Mathf.Min(count, allies.Count);

        var pool = new List<CardDisplay>(allies);
        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int index = UnityEngine.Random.Range(0, pool.Count);
            selected.Add(pool[index]);
            pool.RemoveAt(index);
        }
        return selected;
    }

    public static bool HasValidTargets(string targetType, Owner owner, EffectContext context = null, CardDisplay sourceCard = null)
    {
        return GetTargets(targetType, owner, context, sourceCard).Count > 0;
    }
}
