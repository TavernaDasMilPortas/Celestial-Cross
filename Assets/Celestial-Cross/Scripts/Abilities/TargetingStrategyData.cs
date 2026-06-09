using System;
using System.Collections.Generic;
using UnityEngine;
using CelestialCross.Combat;
using Sirenix.OdinInspector;

namespace Celestial_Cross.Scripts.Abilities
{
    [Serializable]
    public abstract class TargetingStrategyData
    {
        [Tooltip("Ativar se este alvo exigir seleção manual do jogador no Grid (Opção D de Multi-Fases).")]
        public bool RequiresManualSelection = false;

        [ShowIf("RequiresManualSelection")]
        [Tooltip("Regras passadas para o TargetSelector durante a seleção manual.")]
        public TargetingRuleData ManualRule = new TargetingRuleData();
        
        [ShowIf("RequiresManualSelection")]
        public AreaPatternData AreaPattern;

        [ShowIf("RequiresManualSelection")]
        public Direction PreferredDirection = Direction.N;

        [ShowIf("RequiresManualSelection")]
        [Tooltip("Ative para que a área de ataque gire automaticamente na direção do alvo selecionado.")]
        public bool AutoRotateArea = false;
        
        [ShowIf("RequiresManualSelection")]
        public int ManualRange = 1;

        public abstract List<global::Unit> GetTargets(CombatContext context);
    }
}
