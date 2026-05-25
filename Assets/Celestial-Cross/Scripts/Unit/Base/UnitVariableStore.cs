using System.Collections.Generic;
using CelestialCross.Artifacts;
using UnityEngine;

public class UnitVariableStore
{
    private Unit _unit;
    
    // Dicionário "global" do combatente (ex: damage_mult)
    private Dictionary<string, float> _globalVars = new Dictionary<string, float>();
    
    // Dicionário particionado por slot (ex: "skill1", "skill2")
    private Dictionary<string, Dictionary<string, float>> _slotVars = new Dictionary<string, Dictionary<string, float>>();

    public UnitVariableStore(Unit unit)
    {
        _unit = unit;
    }

    public float GetStat(StatType stat)
    {
        var stats = _unit.Stats; // Obtém o status total da unidade já processado
        return stat switch
        {
            StatType.HealthFlat => stats.health,
            StatType.AttackFlat => stats.attack,
            StatType.DefenseFlat => stats.defense,
            StatType.Speed => stats.speed,
            StatType.CriticalRate => stats.criticalChance,
            StatType.CriticalDamage => stats.criticalDamage,
            StatType.EffectHitRate => stats.effectAccuracy,
            StatType.EffectResistance => stats.effectResistance,
            // Fallback prático (normalmente as habilidades escalam com os atributos flat totais)
            StatType.HealthPercent => stats.health,
            StatType.AttackPercent => stats.attack,
            StatType.DefensePercent => stats.defense,
            _ => 0f
        };
    }

    public void SetGlobalVar(string key, float value) => _globalVars[key] = value;
    
    public float GetGlobalVar(string key)
    {
        return _globalVars.TryGetValue(key, out var val) ? val : 0f;
    }

    public void SetSlotVar(string slotId, string key, float value)
    {
        if (!_slotVars.ContainsKey(slotId))
            _slotVars[slotId] = new Dictionary<string, float>();
        _slotVars[slotId][key] = value;
    }

    public float GetSlotVar(string slotId, string key)
    {
        if (_slotVars.TryGetValue(slotId, out var dict))
        {
            if (dict.TryGetValue(key, out var val)) return val;
        }
        return GetGlobalVar(key); // Fallback para global se a do slot não existir
    }
    
    public void ResetAll()
    {
        _globalVars.Clear();
        _slotVars.Clear();
    }
}
