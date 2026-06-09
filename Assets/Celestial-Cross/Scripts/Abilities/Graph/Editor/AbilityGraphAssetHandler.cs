using UnityEditor;
using UnityEditor.Callbacks;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor
{
    public class AbilityGraphAssetHandler
    {
        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceID) as AbilityGraphSO;
            if (asset != null)
            {
                AbilityGraphWindow.OpenWithAsset(asset);
                return true;
            }
            return false;
        }
    }
}
