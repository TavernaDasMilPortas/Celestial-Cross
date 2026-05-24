using UnityEngine;
using System.Collections.Generic;

namespace Celestial_Cross.Scripts.Abilities.SkillTree
{
    [System.Serializable]
    public class SkillBranchOption
    {
        public string branchId;
        public string name;
        [TextArea]
        public string description;
        public Sprite icon;
    }

    [System.Serializable]
    public class SkillBranchTier
    {
        public int tierIndex;
        public List<SkillBranchOption> options = new List<SkillBranchOption>();
    }

    [System.Serializable]
    public class SkillBranchSelection
    {
        public string skillId;
        public List<string> selectedBranchIds = new List<string>();

        public SkillBranchSelection() {}
        public SkillBranchSelection(string skillId)
        {
            this.skillId = skillId;
            this.selectedBranchIds = new List<string>();
        }
    }

    [CreateAssetMenu(fileName = "NewSkillBranchTree", menuName = "Celestial Cross/Skill Tree/Branch Tree")]
    public class SkillBranchTree : ScriptableObject
    {
        public string treeId;
        public List<SkillBranchTier> tiers = new List<SkillBranchTier>();
    }
}
