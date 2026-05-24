using UnityEngine;
using System.Collections.Generic;

namespace Celestial_Cross.Scripts.Abilities.SkillTree
{
    [CreateAssetMenu(fileName = "NewSkillTreeConfig", menuName = "Celestial Cross/Skill Tree/Config")]
    public class SkillTreeConfigSO : ScriptableObject
    {
        public string characterId;
        
        [Header("Habilidade Básica")]
        public SkillEntry basicAttack;
        
        [Header("Habilidade de Movimentação")]
        public SkillEntry movementSkill;
        
        [Header("Pool de Habilidades para os Slots de Combate (Slot1 e Slot2)")]
        public List<SkillEntry> combatSkills = new List<SkillEntry>();
    }
}
