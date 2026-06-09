using System.Collections.Generic;
using UnityEngine;
using CelestialCross.Combat;

namespace Celestial_Cross.Scripts.Abilities.Modifiers
{
    /// <summary>
    /// Instância viva de um Modifier rodando numa Unit.
    /// Gerencia tempo de vida (Duração/Cargas) e segura a referência para o Modificador Data original.
    /// </summary>
    public class ActiveModifier
    {
        public ModifierData sourceModifier { get; private set; }
        public global::Unit targetUnit { get; private set; }
        public global::Unit casterUnit { get; private set; }
        
        // Cópia local da pool de condições para que o modifier consiga avaliar independente depois que a skill fechar
        public List<Celestial_Cross.Scripts.Abilities.Conditions.AbilityConditionData> conditionPool;

        public DurationType durationType { get; private set; }
        public int remainingDuration { get; private set; } // Turns or Charges

        public bool isExpired => 
            (durationType == DurationType.Momentary) || 
            (durationType == DurationType.Turns && remainingDuration <= 0) ||
            (durationType == DurationType.Charges && remainingDuration <= 0) ||
            (durationType == DurationType.UntilEndOfTurn && remainingDuration <= 0);

        public ActiveModifier(ModifierData sourceModifier, global::Unit caster, global::Unit target, List<Celestial_Cross.Scripts.Abilities.Conditions.AbilityConditionData> conditionPool)
        {
            this.sourceModifier = sourceModifier;
            this.casterUnit = caster;
            this.targetUnit = target;
            this.conditionPool = conditionPool;

            this.durationType = sourceModifier.durationSettings.type;
            
            if (this.durationType == DurationType.UntilEndOfTurn)
                this.remainingDuration = 1; // 1 round span essentially
            else
                this.remainingDuration = sourceModifier.durationSettings.durationValue;
        }

        public void DecrementDuration()
        {
            if (durationType == DurationType.Turns || durationType == DurationType.UntilEndOfTurn)
            {
                remainingDuration--;
            }
        }

        public void ConsumeCharge()
        {
            if (durationType == DurationType.Charges)
            {
                remainingDuration--;
            }
        }

        public void ExecuteIfValid(CombatContext context)
        {
            if (isExpired) return;

            // Injeta a pool salva no contexto para avaliacao
            context.conditionPool = this.conditionPool;

            if (sourceModifier.EvaluateConditions(context))
            {
                sourceModifier.ApplyModifier(context);

                if (durationType == DurationType.Charges)
                {
                    ConsumeCharge();
                }
            }
        }
    }
}