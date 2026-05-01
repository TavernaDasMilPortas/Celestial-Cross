using System.Collections.Generic;
using UnityEngine;
using CelestialCross.Data;
using System.Linq;

public static class XPDistributor
{
    public static Dictionary<string, XPGainResult> DistributeXP(int totalXP, List<Unit> participatingUnits, LevelingConfig config)
    {
        Dictionary<string, XPGainResult> results = new Dictionary<string, XPGainResult>();
        if (participatingUnits == null || participatingUnits.Count == 0) return results;

        // Filtramos para ter apenas unidades únicas (caso haja duplicatas por erro) e com data válido
        var validUnits = participatingUnits.Where(u => u != null && u.runtimeUnitData != null).ToList();
        if (validUnits.Count == 0) return results;

        int xpPerUnit = totalXP / validUnits.Count;

        foreach (var unit in validUnits)
        {
            XPGainResult result = new XPGainResult();
            result.xpGained = xpPerUnit;
            result.oldLevel = unit.runtimeUnitData.Level;
            
            unit.runtimeUnitData.CurrentXP += xpPerUnit;

            // Loop de Level Up
            while (unit.runtimeUnitData.Level < unit.unitData.maxLevel)
            {
                int needed = config.GetXPForLevel(unit.runtimeUnitData.Level);
                if (unit.runtimeUnitData.CurrentXP >= needed)
                {
                    unit.runtimeUnitData.CurrentXP -= needed;
                    unit.runtimeUnitData.Level++;
                }
                else break;
            }

            result.newLevel = unit.runtimeUnitData.Level;
            result.currentXP = unit.runtimeUnitData.CurrentXP;
            result.xpToNextLevel = config.GetXPForLevel(unit.runtimeUnitData.Level);

            results[unit.runtimeUnitData.UnitID] = result;
            
            Debug.Log($"[XPDistributor] {unit.DisplayName}: {result.oldLevel} -> {result.newLevel} (XP: {result.currentXP}/{result.xpToNextLevel})");
        }

        return results;
    }
}

[System.Serializable]
public class XPGainResult
{
    public int xpGained;
    public int oldLevel;
    public int newLevel;
    public int currentXP;
    public int xpToNextLevel;
}
