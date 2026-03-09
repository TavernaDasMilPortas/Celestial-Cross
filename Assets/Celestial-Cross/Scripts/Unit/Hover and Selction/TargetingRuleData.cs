using System;
using UnityEngine;

[Serializable]
public class TargetingRuleData
{
    public TargetingMode mode = TargetingMode.Single;
    [Min(1)] public int minTargets = 1;
    [Min(1)] public int maxTargets = 1;
    public bool canTargetSelf;
    public TargetFaction targetFaction = TargetFaction.Any;

    public bool AllowMultiple => mode == TargetingMode.Multiple || mode == TargetingMode.AreaFromTarget || mode == TargetingMode.AreaFromPoint;
}
