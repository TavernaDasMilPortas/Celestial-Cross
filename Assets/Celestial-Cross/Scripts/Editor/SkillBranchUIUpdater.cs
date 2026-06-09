using UnityEngine;
using UnityEditor;
using CelestialCross.UI.Skills;
using UnityEngine.UI;
using TMPro;

namespace CelestialCross.Editor
{
    public class SkillBranchUIUpdater : MonoBehaviour
    {
        [MenuItem("Celestial Cross/3. UI Builders/2. Modals/Update Skill Branch Modal Prefab")]
        public static void UpdateSkillBranchModalPrefab()
        {
            // As this is a generic project update utility we can guide the designer here
            // to modify the optionPrefab inside their SkillBranchModal to include 
            // an Icon Image and a Description TextMeshProUGUI.
            
            Debug.Log("<color=green>Update Skill Branch Modal</color>: Please ensure that your OptionPrefab inside the SkillBranchModal has:");
            Debug.Log("1. An Image component for the icon.");
            Debug.Log("2. A TextMeshProUGUI for the Name.");
            Debug.Log("3. (Optional) A TextMeshProUGUI for the Description.");
            
            // To actually modify the scene/prefab, one would load the prefab via AssetDatabase
            // and modify its structure. Since UI setups are often heavily customized by designers,
            // logging the required structure is a safe first step.
        }
    }
}
