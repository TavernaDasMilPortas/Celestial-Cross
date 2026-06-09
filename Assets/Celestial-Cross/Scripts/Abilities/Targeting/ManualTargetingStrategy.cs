using System;
using System.Collections.Generic;
using UnityEngine;
using CelestialCross.Combat;

namespace Celestial_Cross.Scripts.Abilities.Targeting
{
    [Serializable]
    public class ManualTargetingStrategy : TargetingStrategyData
    {
        public ManualTargetingStrategy()
        {
            // Força a exigir seleção manual por padrão
            RequiresManualSelection = true;
        }

        public override List<Unit> GetTargets(CombatContext context)
        {
            // Este método é chamado como fallback ou se algo precisar de alvos estáticos, 
            // mas como é manual, o AbilityExecutor normalmente ignora esse retorno e pega
            // da lista selecionada pelo jogador na tela. Retornamos vazio caso chamado por engano.
            return new List<Unit>();
        }
    }
}