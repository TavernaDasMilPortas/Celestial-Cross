using System.Collections.Generic;

namespace Celestial_Cross.Scripts.Abilities.SkillTree
{
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
}
