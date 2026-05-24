using UnityEngine;
using Celestial_Cross.Scripts.Abilities.Graph;

namespace Celestial_Cross.Scripts.Abilities.SkillTree
{
    [CreateAssetMenu(fileName = "NewSkillEntry", menuName = "Celestial Cross/Skill Tree/Skill Entry")]
    public class SkillEntry : ScriptableObject
    {
        public string skillId;
        public string skillName;
        [TextArea]
        public string description;
        public Sprite icon;
        public AbilityGraphSO abilityGraph;
        
        [Header("Árvore de Ramos associada (Modificadores)")]
        public SkillBranchTree branchTree;
    }
}
