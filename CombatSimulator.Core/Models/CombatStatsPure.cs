using System;

namespace CombatSimulator.Core.Models
{
    public struct CombatStatsPure
    {
        public int Health { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int Speed { get; set; }
        public int CriticalChance { get; set; }
        public int CriticalDamage { get; set; }
        public int EffectAccuracy { get; set; }
        public int EffectResistance { get; set; }
    }
}
