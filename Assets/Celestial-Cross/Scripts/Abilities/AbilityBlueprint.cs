using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Celestial_Cross.Scripts.Abilities
{
    [CreateAssetMenu(fileName = "NewAbilityBlueprint", menuName = "Celestial Cross/Abilities/Ability Blueprint")]
    public class AbilityBlueprint : ScriptableObject
    {
        [Header("UI & Identity")]
        public string abilityName = "New Ability";
        public Sprite abilityIcon;
        [TextArea] public string abilityDescription;
        public int displayRange = 1;
        public AbilityType abilityType = AbilityType.Active;
        [Tooltip("Is this a passive ability? Passive abilities are not shown in the action bar.")]
        public bool isPassive = false;
        public bool isBuff = true;


        [Header("Node System (Optional)")]
        [Tooltip("If assigned, the execution will use this graph instead of the steps below.")]
        public Celestial_Cross.Scripts.Abilities.Graph.AbilityGraphSO abilityGraph;

        [Header("Condition Settings")]
        [Tooltip("If true, this ability will be applied as a persistent condition on the target.")]
        public bool isPersistentCondition = false;
        [Tooltip("Duration in turns. 0 means infinite until cleansed.")]
        public int durationInTurns = 0;
        
        [Header("Stacking")]
        [Tooltip("If true, applying this condition again will add a stack instead of just refreshing.")]
        public bool canStack = false;
        [Tooltip("Maximum number of stacks this condition can have. (0 = infinite)")]
        [ShowIf("canStack")]
        public int maxStacks = 1;

        [Header("Effects & Modifiers")]
        [Title("Active Effects", "Executed when the ability is manually cast.")]
        [ListDrawerSettings(ShowPaging = false, ShowItemCount = false, ShowFoldout = true)]
        [SerializeReference]
        public List<EffectStep> effectSteps = new List<EffectStep>();

        [Title("Passive Modifiers", "Triggered by game events (hooks).")]
        [ListDrawerSettings(ShowPaging = false, ShowItemCount = false, ShowFoldout = true)]
        [SerializeReference]
        public List<EffectStep> modifierSteps = new List<EffectStep>();

        [Header("Passive Modifiers")]
        [Tooltip("List of synchronous modifiers listening to hooks.")]
        [SerializeReference]
        public List<Celestial_Cross.Scripts.Abilities.Modifiers.ModifierData> modifiers = new List<Celestial_Cross.Scripts.Abilities.Modifiers.ModifierData>();

        public bool IsPassiveOnly => effectSteps.Count == 0 && modifiers.Count > 0;
    }
}
