using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using CelestialCross.Combat;

namespace Celestial_Cross.Scripts.Abilities
{
    [System.Serializable]
    public class EffectStep
    {
        [Title("Trigger / Timing", null, TitleAlignments.Centered)]
        [Tooltip("Quando este passo será ativado? Use OnManualCast para Habilidades Ativas padrão, ou outros hooks para Passivas.")]
        public CombatHook trigger = CombatHook.OnManualCast;

        [Title("Targeting", null, TitleAlignments.Centered)]
        [SerializeReference]
        public TargetingStrategyData targetingStrategy;

        [Title("Effects", null, TitleAlignments.Centered)]
        [ListDrawerSettings(ShowPaging = false, Expanded = true)]
        [SerializeReference]
        public List<EffectData> effects = new List<EffectData>();
    }
}
