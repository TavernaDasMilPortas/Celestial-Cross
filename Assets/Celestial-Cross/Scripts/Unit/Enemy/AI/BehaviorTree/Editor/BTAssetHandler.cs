using UnityEditor;
using UnityEditor.Callbacks;
using Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree;

namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Editor
{
    public class BTAssetHandler
    {
        [OnOpenAsset(1)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj is BehaviorTreeSO bt)
            {
                BTEditorWindow.OpenWithAsset(bt);
                return true;
            }
            return false;
        }
    }
}
