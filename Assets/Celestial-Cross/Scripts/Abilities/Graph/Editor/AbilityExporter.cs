using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Celestial_Cross.Scripts.Abilities.Graph;
using Newtonsoft.Json;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor
{
    public class AbilityExporter
    {
        [MenuItem("Celestial Cross/Export Ability Graphs")]
        public static void ExportAll()
        {
            // Find all AbilityGraphSO assets
            string[] guids = AssetDatabase.FindAssets("t:AbilityGraphSO");
            var allGraphs = new List<AbilityGraphSO>();
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var graph = AssetDatabase.LoadAssetAtPath<AbilityGraphSO>(path);
                if (graph != null)
                {
                    allGraphs.Add(graph);
                }
            }

            var exportData = new Dictionary<string, AbilityGraphExport>();

            foreach (var graph in allGraphs)
            {
                var deps = new Dictionary<string, string>();
                foreach (var dep in graph.Dependencies)
                {
                    if (dep != null && !string.IsNullOrEmpty(dep.id))
                    {
                        string depName = dep.asset != null ? dep.asset.name : dep.id;
                        deps[dep.id] = depName;
                    }
                }

                exportData[graph.name] = new AbilityGraphExport
                {
                    Name = graph.abilityName,
                    Type = graph.GetAbilityType().ToString(),
                    Nodes = graph.NodeData.Select(n => new NodeExport
                    {
                        Guid = n.Guid,
                        NodeType = n.NodeType,
                        JsonData = n.JsonData
                    }).ToList(),
                    Links = graph.NodeLinks.Select(l => new LinkExport
                    {
                        BaseNodeGuid = l.BaseNodeGuid,
                        PortName = l.PortName,
                        TargetNodeGuid = l.TargetNodeGuid,
                        TargetPortName = l.TargetPortName
                    }).ToList(),
                    Variables = graph.Variables.Select(v => new VarExport
                    {
                        Name = v.name,
                        InitialValue = v.initialValue
                    }).ToList(),
                    Dependencies = deps,
                    Duration = graph.GetDuration(),
                    IsBuff = graph.GetIsBuff(),
                    CanStack = graph.GetCanStack(),
                    MaxStacks = graph.GetMaxStacks(),
                    IsPersistent = graph.GetIsPersistent(),
                    MaxLevel = graph.MaxLevel
                };
            }

            // Export to backend folder
            string backendPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Backend/gamedata_abilities.json"));
            
            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(backendPath));

            string json = JsonConvert.SerializeObject(exportData, Formatting.Indented);
            File.WriteAllText(backendPath, json);

            Debug.Log($"[AbilityExporter] Exported {allGraphs.Count} abilities to {backendPath}");
            EditorUtility.DisplayDialog("Export Complete", $"Exported {allGraphs.Count} abilities to backend folder.", "OK");
        }
    }

    [System.Serializable]
    public class AbilityGraphExport
    {
        public string Name;
        public string Type;
        public List<NodeExport> Nodes = new List<NodeExport>();
        public List<LinkExport> Links = new List<LinkExport>();
        public List<VarExport> Variables = new List<VarExport>();
        public Dictionary<string, string> Dependencies = new Dictionary<string, string>();
        
        public int Duration;
        public bool IsBuff;
        public bool CanStack;
        public int MaxStacks;
        public bool IsPersistent;
        public int MaxLevel;
    }

    [System.Serializable]
    public class NodeExport
    {
        public string Guid;
        public string NodeType;
        public string JsonData;
    }

    [System.Serializable]
    public class LinkExport
    {
        public string BaseNodeGuid;
        public string PortName;
        public string TargetNodeGuid;
        public string TargetPortName;
    }

    [System.Serializable]
    public class VarExport
    {
        public string Name;
        public float InitialValue;
    }
}
