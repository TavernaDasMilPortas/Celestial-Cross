using System;
using UnityEngine;
using CelestialCross.Combat;

namespace Celestial_Cross.Scripts.Abilities
{
    [Serializable]
    public class StatModifierEffectData : EffectData
    {
        public CombatStats modifiers;

        public override void Execute(CombatContext context)
        {
            if (context.target != null)
            {
                Debug.Log($"StatModifierEffect: Modificando status de {context.target.name}");
                // Logica do stats base
            }
        }
    }
}
