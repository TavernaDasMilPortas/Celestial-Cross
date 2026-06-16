using UnityEngine;
using System.Collections.Generic;
using Celestial_Cross.Scripts.Abilities.Graph;

namespace Celestial_Cross.Scripts.Abilities.SkillTree
{
    [CreateAssetMenu(fileName = "NewSkillTreeConfig", menuName = "Celestial Cross/Skill Tree/Config")]
    public class SkillTreeConfigSO : ScriptableObject
    {
        public string characterId;
        
        [Header("Habilidade Básica")]
        public AbilityGraphSO basicAttack;
        
        [Header("Habilidade de Movimentação")]
        public AbilityGraphSO movementSkill;
        
        [Header("Habilidades Padrão (Auto-equipadas no primeiro acesso)")]
        public AbilityGraphSO defaultSlot1Skill;
        public AbilityGraphSO defaultSlot2Skill;
        
        [Header("Pool de Habilidades para os Slots de Combate (Slot1 e Slot2)")]
        [System.Obsolete("Use slot1Skills and slot2Skills instead")]
        public List<AbilityGraphSO> combatSkills = new List<AbilityGraphSO>();

        [Header("Pool de Habilidades para o Slot 1")]
        public List<AbilityGraphSO> slot1Skills = new List<AbilityGraphSO>();

        [Header("Pool de Habilidades para o Slot 2")]
        public List<AbilityGraphSO> slot2Skills = new List<AbilityGraphSO>();
    }
}
