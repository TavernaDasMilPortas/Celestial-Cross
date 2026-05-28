using System;

namespace Celestial_Cross.Scripts.Abilities.Graph.Runtime
{
    public enum UnitVariable
    {
        // === Atributos Base (somente leitura pelo nó) ===
        Health, Attack, Defense, Speed,
        CriticalChance, CriticalDamage, EffectAccuracy, EffectResistance,

        // === Amplificação e Modificação ===
        BonusDamagePercent,
        DamageReductionPercent,
        HealingBonusPercent,

        // === Alcance & Movimento ===
        ExtraRange,
        ExtraMoveRange,

        // === Contadores Livres ===
        Counter1, Counter2, Counter3
    }

    public enum UnitVariableScope
    {
        Global,
        Slot
    }

    public enum UnitVariableOperation
    {
        Get,
        Set,
        Add,
        Subtract,
        Multiply,
        Divide
    }

    public static class UnitVariableHelper
    {
        public static bool IsReadOnly(UnitVariable variable)
        {
            switch (variable)
            {
                case UnitVariable.Health:
                case UnitVariable.Attack:
                case UnitVariable.Defense:
                case UnitVariable.Speed:
                case UnitVariable.CriticalChance:
                case UnitVariable.CriticalDamage:
                case UnitVariable.EffectAccuracy:
                case UnitVariable.EffectResistance:
                    return true;
                default:
                    return false;
            }
        }
    }
}
