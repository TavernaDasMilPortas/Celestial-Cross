using System;

namespace CelestialCross.Combat
{
    [Serializable]
    public struct StatScalingData
    {
        public CelestialCross.Artifacts.StatType statType;
        public float percentage;
        public bool useTargetStat; // false = usa status do Caster, true = usa status do Target
    }
}
