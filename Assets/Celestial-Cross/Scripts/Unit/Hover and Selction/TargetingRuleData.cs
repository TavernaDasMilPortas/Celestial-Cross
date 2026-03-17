using System;
using UnityEngine;

[Serializable]
public class TargetingRuleData
{
    public TargetingMode mode = TargetingMode.Unit;
    public TargetOrigin origin = TargetOrigin.Point;
    public bool allowMultiple;
    [Min(1)] public int minTargets = 1;
    [Min(1)] public int maxTargets = 1;
    public bool canTargetSelf;
    public TargetFaction targetFaction = TargetFaction.Any;

    public TargetingRuleData Clone()
    {
        return new TargetingRuleData
        {
            mode = mode,
            origin = origin,
            allowMultiple = allowMultiple,
            minTargets = minTargets,
            maxTargets = maxTargets,
            canTargetSelf = canTargetSelf,
            targetFaction = targetFaction
        };
    }
}
