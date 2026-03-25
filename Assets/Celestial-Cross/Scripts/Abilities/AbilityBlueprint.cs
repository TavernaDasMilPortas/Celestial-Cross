using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Celestial_Cross.Scripts.Abilities
{
    [CreateAssetMenu(fileName = "NewAbilityBlueprint", menuName = "Celestial-Cross/Abilities/Ability Blueprint")]
    public class AbilityBlueprint : ScriptableObject
    {
        [Header("UI & Identity")]
        public string abilityName = "New Ability";
        public Sprite abilityIcon;
        [TextArea] public string abilityDescription;
        public int displayRange = 1;

        [Header("Effects Sequence")]
        [ListDrawerSettings(ShowPaging = false, ShowItemCount = false, ShowFoldout = true)]
        public List<EffectStep> effectSteps = new List<EffectStep>();
    }
}
