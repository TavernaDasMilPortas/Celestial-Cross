using System;
using UnityEngine;
using CelestialCross.Combat;
using Celestial_Cross.Scripts.Abilities.Modifiers;

namespace Celestial_Cross.Scripts.Abilities
{
    [Serializable]
    [Obsolete("Use ApplyModifierEffectData instead.")]
    public class ApplyConditionEffectData : EffectData
    {
        public AbilityBlueprint condition;

        public override void Execute(CombatContext context)
        {
            if (context.target != null && condition != null)
            {
                Debug.LogWarning("ApplyConditionEffectData is obsolete. Please migrate this ability to use ApplyModifierEffectData.");
            }
        }
    }
}
