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
        [Tooltip("Quando este passo ser� ativado? Use OnManualCast para Habilidades Ativas padr�o, ou outros hooks para Passivas.")]
        public CombatHook trigger = CombatHook.OnManualCast;

        [Title("Targeting", null, TitleAlignments.Centered)]
        [Tooltip("If true, this step will reuse the targets from the previous step instead of running its own targeting strategy.")]
        public bool reusePreviousTargets = false;

        [HideIf("reusePreviousTargets")]
        [SerializeReference]
        public TargetingStrategyData targetingStrategy;

        [Title("Effects", null, TitleAlignments.Centered)]
        [ListDrawerSettings(ShowPaging = false, ShowFoldout = true)]
        [SerializeReference]
        public List<EffectData> effects = new List<EffectData>();
    }
}
