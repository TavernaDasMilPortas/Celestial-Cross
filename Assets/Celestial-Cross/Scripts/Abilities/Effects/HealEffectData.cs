using System;
using UnityEngine;
using CelestialCross.Combat;
using System.Collections.Generic;

namespace Celestial_Cross.Scripts.Abilities
{
    [Serializable]
    public class HealEffectData : EffectData
    {
        public int healAmount = 10;

        public override void Execute(CombatContext context)
        {
            if (context.target != null && context.target.Health != null)
            {
                context.target.Health.Heal(healAmount);
            }
        }
    }
}