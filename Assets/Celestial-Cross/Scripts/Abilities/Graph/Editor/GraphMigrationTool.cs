using UnityEngine;
using UnityEditor;
using Celestial_Cross.Scripts.Abilities.Graph;
using Celestial_Cross.Scripts.Abilities.Graph.Runtime;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor
{
    public class GraphMigrationTool : EditorWindow
    {
        [MenuItem("Celestial Cross/Tools/Migrate Graph Area Patterns")]
        public static void ShowWindow()
        {
            GetWindow<GraphMigrationTool>("Graph Migration");
        }

        private void OnGUI()
        {
            GUILayout.Label("Migração de AreaPatternData", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Migrar e Remover [DEPRECATED] areaPattern"))
            {
                MigrateAreaPatterns();
            }
        }

        private void MigrateAreaPatterns()
        {
            string[] guids = AssetDatabase.FindAssets("t:AbilityGraphSO");
            int migratedCount = 0;
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AbilityGraphSO graph = AssetDatabase.LoadAssetAtPath<AbilityGraphSO>(path);
                
                bool changed = false;
                foreach (var node in graph.NodeData)
                {
                    if (node.NodeType == "TargetNode" && node.areaPattern != null)
                    {
                        var targetData = JsonUtility.FromJson<TargetNodeData>(node.JsonData);
                        
                        if (string.IsNullOrEmpty(targetData.patternReferenceId) || targetData.patternReferenceId != node.areaPattern.name)
                        {
                            string assetId = node.areaPattern.name; // Simples ID baseado no nome
                            targetData.patternReferenceId = assetId;
                            
                            // Adicionar a dependência se não existir
                            bool hasDep = false;
                            foreach(var dep in graph.Dependencies)
                            {
                                if (dep.id == assetId) { hasDep = true; break; }
                            }
                            
                            if (!hasDep)
                            {
                                graph.Dependencies.Add(new AbilityGraphSO.Dependency { id = assetId, asset = node.areaPattern, type = "AreaPattern" });
                            }
                            
                            node.JsonData = JsonUtility.ToJson(targetData);
                            changed = true;
                        }
                        
                        // Remover a referência deprecated
                        node.areaPattern = null;
                        changed = true;
                    }
                }
                
                if (changed)
                {
                    EditorUtility.SetDirty(graph);
                    migratedCount++;
                }
            }
            
            AssetDatabase.SaveAssets();
            Debug.Log($"[Migration] Migração concluída! {migratedCount} grafos modificados.");
        }
    }
}
