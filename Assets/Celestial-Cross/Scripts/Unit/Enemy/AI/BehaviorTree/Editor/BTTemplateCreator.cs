#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Editor
{
    public static class BTTemplateCreator
    {
        [MenuItem("Celestial Cross/2. Data & Assets/AI/Generate BT Templates")]
        public static void GenerateTemplates()
        {
            string folder = "Assets/Celestial-Cross/Data/AI";
            if (!AssetDatabase.IsValidFolder(folder))
            {
                System.IO.Directory.CreateDirectory(Application.dataPath + "/Celestial-Cross/Data/AI");
                AssetDatabase.Refresh();
            }
            
            string templateFolder = "Assets/Celestial-Cross/Data/AI/Templates";
            if (!AssetDatabase.IsValidFolder(templateFolder))
            {
                System.IO.Directory.CreateDirectory(Application.dataPath + "/Celestial-Cross/Data/AI/Templates");
                AssetDatabase.Refresh();
            }

            CreateTemplate(templateFolder, "AggressiveMelee");
            CreateTemplate(templateFolder, "RangedKite");
            CreateTemplate(templateFolder, "HealerSupport");
            CreateTemplate(templateFolder, "BossPhased");

            AssetDatabase.SaveAssets();
            Debug.Log("BT Templates generated at " + templateFolder);
        }

        private static void CreateTemplate(string folder, string name)
        {
            string path = $"{folder}/{name}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<BehaviorTreeSO>(path);
            BehaviorTreeSO so = existing != null ? existing : ScriptableObject.CreateInstance<BehaviorTreeSO>();

            if (name == "AggressiveMelee")
            {
                so.treeName = "Aggressive Melee";
                so.description = "Tenta atacar se o alvo estiver no alcance. Se não estiver, move-se agressivamente.";

                so.NodeData = new System.Collections.Generic.List<BTNodeData>
                {
                    new BTNodeData { Guid = "root", NodeTitle = "Root", NodeType = "BTRootEditorNode", Position = new Vector2(100, 200), JsonData = "" },
                    new BTNodeData { Guid = "sel", NodeTitle = "Selector", NodeType = "BTSelectorEditorNode", Position = new Vector2(400, 200), JsonData = "{\"ports\":[\"Passo_0\",\"Passo_1\"]}" },
                    new BTNodeData { Guid = "seq", NodeTitle = "Sequence", NodeType = "BTSequenceEditorNode", Position = new Vector2(700, 50), JsonData = "{\"ports\":[\"Passo_0\",\"Passo_1\"]}" },
                    new BTNodeData { Guid = "cond_range", NodeTitle = "Alvo no Alcance", NodeType = "BTConditionTargetInRangeEditorNode", Position = new Vector2(1000, -50), JsonData = "" },
                    new BTNodeData { Guid = "act_atk", NodeTitle = "Cast Damage Skill", NodeType = "BTActionUseAbilityEditorNode", Position = new Vector2(1000, 150), JsonData = "{\"category\":0}" },
                    new BTNodeData { Guid = "act_mov", NodeTitle = "Move to Target", NodeType = "BTActionMoveEditorNode", Position = new Vector2(700, 350), JsonData = "{\"intent\":0}" }
                };

                so.NodeLinks = new System.Collections.Generic.List<BTLinkData>
                {
                    new BTLinkData { ParentGuid = "root", ParentPort = "Child", ChildGuid = "sel", ChildPort = "Parent" },
                    new BTLinkData { ParentGuid = "sel", ParentPort = "Passo_0", ChildGuid = "seq", ChildPort = "Parent" },
                    new BTLinkData { ParentGuid = "sel", ParentPort = "Passo_1", ChildGuid = "act_mov", ChildPort = "Parent" },
                    new BTLinkData { ParentGuid = "seq", ParentPort = "Passo_0", ChildGuid = "cond_range", ChildPort = "Parent" },
                    new BTLinkData { ParentGuid = "seq", ParentPort = "Passo_1", ChildGuid = "act_atk", ChildPort = "Parent" }
                };
                
                EditorUtility.SetDirty(so);
            }

            if (existing == null)
            {
                AssetDatabase.CreateAsset(so, path);
            }
        }
    }
}
#endif
