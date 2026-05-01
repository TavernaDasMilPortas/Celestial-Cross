using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor
{
    public class GraphSaveUtility
    {
        private AbilityGraphView _targetGraphView;
        private AbilityGraphSO _containerAsset;

        private List<Edge> Edges => _targetGraphView.edges.ToList();
        private List<AbilityNode> Nodes => _targetGraphView.nodes.ToList().Cast<AbilityNode>().ToList();

        public static GraphSaveUtility GetInstance(AbilityGraphView graphView)
        {
            return new GraphSaveUtility
            {
                _targetGraphView = graphView
            };
        }

        public void SaveGraph(AbilityGraphSO graphSO)
        {
            if (graphSO == null) return;

            // Limpar dados antigos
            graphSO.NodeLinks.Clear();
            graphSO.NodeData.Clear();

            // Salvar Conexões
            var connectedPorts = Edges.Where(x => x.input.node != null).ToArray();
            for (int i = 0; i < connectedPorts.Length; i++)
            {
                var outputNode = connectedPorts[i].output.node as AbilityNode;
                var inputNode = connectedPorts[i].input.node as AbilityNode;

                graphSO.NodeLinks.Add(new NodeLinkData
                {
                    BaseNodeGuid = outputNode.GUID,
                    PortName = connectedPorts[i].output.portName,
                    TargetNodeGuid = inputNode.GUID,
                    TargetPortName = connectedPorts[i].input.portName
                });
            }

            // Salvar Nós
            foreach (var node in Nodes)
            {
                var data = new AbilityNodeData
                {
                    Guid = node.GUID,
                    NodeType = node.GetType().Name,
                    NodeTitle = node.title,
                    Position = node.GetPosition().position,
                    JsonData = node.GetJsonData()
                };
                
                node.OnSave(data);
                graphSO.NodeData.Add(data);
            }

            // Gerar Descrição Automática
            graphSO.GeneratedDescription = GenerateAutoDescription();

            EditorUtility.SetDirty(graphSO);
            AssetDatabase.SaveAssets();
            Debug.Log($"[GraphSaveUtility] Grafo salvo com sucesso em: {graphSO.name}");
        }

        private string GenerateAutoDescription()
        {
            var startNode = Nodes.FirstOrDefault(x => x.EntryPoint);
            if (startNode == null) return "Grafo sem ponto de início.";

            var sb = new System.Text.StringBuilder();
            var visited = new HashSet<AbilityNode>();
            
            sb.AppendLine("<b>--- DESCRIÇÃO AUTOMÁTICA ---</b>");
            
            GenerateNodeDescriptionRecursive(startNode, sb, visited, 0);

            return sb.ToString();
        }

        private void GenerateNodeDescriptionRecursive(AbilityNode node, System.Text.StringBuilder sb, HashSet<AbilityNode> visited, int indent)
        {
            if (node == null || visited.Contains(node)) return;
            visited.Add(node);

            string desc = node.GetDescription();
            if (!string.IsNullOrEmpty(desc))
            {
                string indentation = new string(' ', indent * 2);
                sb.AppendLine($"{indentation}- {desc}");
            }

            // Seguir para os próximos nós
            var outputPorts = node.outputContainer.Query<Port>().ToList();
            foreach (var port in outputPorts)
            {
                var connections = port.connections.ToList();
                foreach (var edge in connections)
                {
                    var nextNode = edge.input.node as AbilityNode;
                    if (nextNode != null)
                    {
                        // Se for uma ramificação, aumentamos o indent
                        int nextIndent = (node is ConditionalFlowNode) ? indent + 1 : indent;
                        
                        if (node is ConditionalFlowNode)
                        {
                            sb.AppendLine($"{new string(' ', indent * 2)}  * Se for '{port.portName}':");
                        }

                        GenerateNodeDescriptionRecursive(nextNode, sb, visited, nextIndent);
                    }
                }
            }
        }

        public void LoadGraph(AbilityGraphSO graphSO)
        {
            if (graphSO == null) return;

            ClearGraph();

            if (graphSO.NodeData.Count == 0)
            {
                // Grafo vazio — semear com StartNode + LevelBranchNode
                _targetGraphView.SeedDefaultNodes();
                return;
            }

            CreateNodes(graphSO);
            ConnectNodes(graphSO);
        }

        private void ClearGraph()
        {
            foreach (var node in Nodes)
            {
                _targetGraphView.RemoveElement(node);
            }

            foreach (var edge in Edges)
            {
                _targetGraphView.RemoveElement(edge);
            }
        }

        private void CreateNodes(AbilityGraphSO graphSO)
        {
            foreach (var nodeData in graphSO.NodeData)
            {
                // Instanciar pelo tipo salvo via reflexão
                var type = Type.GetType($"Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes.{nodeData.NodeType}");
                if (type == null) continue;

                var tempNode = Activator.CreateInstance(type) as AbilityNode;
                tempNode.Initialize(nodeData.Guid, nodeData.Position);
                tempNode.title = nodeData.NodeTitle;
                
                _targetGraphView.AddElement(tempNode);
                
                // Restaurar dados internos (dropdowns, etc)
                tempNode.LoadFromJson(nodeData.JsonData);
                tempNode.OnLoad(nodeData);
            }
        }

        private void ConnectNodes(AbilityGraphSO graphSO)
        {
            var nodes = Nodes;
            for (int i = 0; i < graphSO.NodeLinks.Count; i++)
            {
                var link = graphSO.NodeLinks[i];
                var baseNode = nodes.FirstOrDefault(x => x.GUID == link.BaseNodeGuid);
                var targetNode = nodes.FirstOrDefault(x => x.GUID == link.TargetNodeGuid);

                if (baseNode == null || targetNode == null) continue;

                // Encontrar as portas certas pelos nomes
                var outputPort = baseNode.outputContainer.Query<Port>().ToList().FirstOrDefault(x => x.portName == link.PortName);
                var inputPort = targetNode.inputContainer.Query<Port>().ToList().FirstOrDefault(x => x.portName == link.TargetPortName);

                if (outputPort != null && inputPort != null)
                {
                    var edge = outputPort.ConnectTo(inputPort);
                    _targetGraphView.AddElement(edge);
                }
            }
        }
    }
}
