using System.Linq;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Editor
{
    public class BTSaveUtility
    {
        private BTGraphView _graphView;

        public static BTSaveUtility GetInstance(BTGraphView graphView)
        {
            return new BTSaveUtility { _graphView = graphView };
        }

        public void SaveGraph(BehaviorTreeSO asset)
        {
            if (asset == null) return;

            var nodes = _graphView.nodes.ToList().Cast<BTEditorNode>().ToList();
            var edges = _graphView.edges.ToList();

            asset.NodeData.Clear();
            asset.NodeLinks.Clear();

            // Save Nodes
            foreach (var node in nodes)
            {
                var nodeData = new BTNodeData
                {
                    Guid = node.Guid,
                    NodeType = node.NodeType,
                    NodeTitle = node.title,
                    Position = node.GetPosition().position,
                    JsonData = node.GetJsonData()
                };
                asset.NodeData.Add(nodeData);
            }

            // Save Links
            foreach (var edge in edges)
            {
                var inputNode = edge.input.node as BTEditorNode;
                var outputNode = edge.output.node as BTEditorNode;

                if (inputNode == null || outputNode == null) continue;

                var linkData = new BTLinkData
                {
                    ParentGuid = outputNode.Guid,
                    ParentPort = edge.output.portName,
                    ChildGuid = inputNode.Guid,
                    ChildPort = edge.input.portName
                };
                asset.NodeLinks.Add(linkData);
            }

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            Debug.Log($"[BTSaveUtility] Behavior Tree '{asset.treeName}' saved successfully.");
        }

        public void LoadGraph(BehaviorTreeSO asset)
        {
            if (asset == null) return;
            _graphView.ClearGraph();

            if (asset.NodeData == null || asset.NodeData.Count == 0) return;

            var generatedNodes = new Dictionary<string, BTEditorNode>();

            // Recreate Nodes
            foreach (var nodeData in asset.NodeData)
            {
                var typeName = "Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Editor.Nodes." + nodeData.NodeType;
                var type = System.Type.GetType(typeName + ", Assembly-CSharp-Editor");
                if (type == null)
                {
                    foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                    {
                        type = assembly.GetType(typeName);
                        if (type != null) break;
                    }
                }

                if (type == null)
                {
                    Debug.LogError($"Could not find node type: {nodeData.NodeType}");
                    continue;
                }

                BTEditorNode tempNode = System.Activator.CreateInstance(type) as BTEditorNode;

                tempNode.Initialize(nodeData.NodeTitle, nodeData.Position); 
                tempNode.Guid = nodeData.Guid;
                tempNode.NodeType = nodeData.NodeType;
                tempNode.LoadFromJson(nodeData.JsonData);

                _graphView.CreateNode(tempNode, nodeData.Position);
                generatedNodes.Add(nodeData.Guid, tempNode);
            }

            // Recreate Links
            foreach (var linkData in asset.NodeLinks)
            {
                if (!generatedNodes.ContainsKey(linkData.ParentGuid) || !generatedNodes.ContainsKey(linkData.ChildGuid)) continue;

                var parentNode = generatedNodes[linkData.ParentGuid];
                var childNode = generatedNodes[linkData.ChildGuid];

                var outputPorts = parentNode.outputContainer.Query<Port>().ToList();
                var inputPorts = childNode.inputContainer.Query<Port>().ToList();

                var outputPort = outputPorts.FirstOrDefault(p => p.portName == linkData.ParentPort);
                var inputPort = inputPorts.FirstOrDefault(p => p.portName == linkData.ChildPort);

                if (outputPort != null && inputPort != null)
                {
                    var edge = outputPort.ConnectTo(inputPort);
                    _graphView.AddElement(edge);
                }
            }
        }
    }
}
