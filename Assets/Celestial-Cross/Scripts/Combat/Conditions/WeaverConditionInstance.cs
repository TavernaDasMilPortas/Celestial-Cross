using UnityEngine;
using System.Collections.Generic;

namespace CelestialCross.Combat
{
    public class WeaverConditionInstance
    {
        public WeaverConditionData Data { get; private set; }
        public Unit Target { get; private set; }
        public Unit Source { get; private set; }
        public int RemainingTurns { get; private set; }
        public bool IsExpired { get; private set; }

        public WeaverConditionInstance(WeaverConditionData data, Unit target, Unit source)
        {
            Data = data;
            Target = target;
            Source = source;
            RemainingTurns = data.duration;
        }

        public void OnApplied()
        {
            CombatContext context = new CombatContext(Source, Target);
            foreach (var effect in Data.onApplyEffects)
                effect?.Execute(context);
        }

        public void OnRemoved()
        {
            CombatContext context = new CombatContext(Source, Target);
            foreach (var effect in Data.onExpireEffects)
                effect?.Execute(context);
        }

        public void OnHookTriggered(CombatHook hook, CombatContext context)
        {
            if (IsExpired) return;

            // Duração reduz no início do turno da unidade atingida
            if (hook == CombatHook.OnTurnStart && context.source == Target)
            {
                // Executar efeitos de Tick
                Data.ExecuteTick(new CombatContext(Source, Target));

                RemainingTurns--;
                if (RemainingTurns <= 0)
                {
                    IsExpired = true;
                    // O PassiveManager deve remover isto
                }
            }
        }
    }
}
