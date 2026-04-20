using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CelestialCross.Dialogue.Graph.Editor
{
    public class GraphSaveUtility
    {
        private DialogueGraphView _targetGraphView;
        private DialogueGraph _containerCache;

        private List<Edge> Edges => _targetGraphView.edges.ToList();
        private List<DialogueNode> Nodes => _targetGraphView.nodes.ToList().Cast<DialogueNode>().ToList();

        public static GraphSaveUtility GetInstance(DialogueGraphView targetGraphView)
        {
            return new GraphSaveUtility
            {
                _targetGraphView = targetGraphView
            };
        }

        public void SaveGraph(DialogueGraph targetGraph)
        {
            if (targetGraph == null) return;
            
            // Limpa dados antigos antes de salvar
            targetGraph.nodeLinks.Clear();
            targetGraph.nodeData.Clear();
            targetGraph.exposedProperties.Clear();

            if (!SaveNodes(targetGraph)) return;

            targetGraph.exposedProperties = _targetGraphView.ExposedProperties;

            EditorUtility.SetDirty(targetGraph);
            AssetDatabase.SaveAssets();
        }

        private bool SaveNodes(DialogueGraph dialogueGraph)
        {
            if (!Edges.Any()) return true;

            var connectedSockets = Edges.Where(x => x.input.node != null).ToArray();
            for (var i = 0; i < connectedSockets.Count(); i++)
            {
                var outputNode = connectedSockets[i].output.node as DialogueNode;
                var inputNode = connectedSockets[i].input.node as DialogueNode;

                dialogueGraph.nodeLinks.Add(new NodeLinkData
                {
                    baseNodeGuid = outputNode.guid,
                    portName = connectedSockets[i].output.portName,
                    targetNodeGuid = inputNode.guid
                });
            }

            foreach (var dialogueNode in Nodes.Where(node => !node.entryPoint))
            {
                dialogueGraph.nodeData.Add(new DialogueNodeData
                {
                    guid = dialogueNode.guid,
                    nodeType = dialogueNode.nodeType,
                    speakerName = dialogueNode.speakerName,
                    dialogueText = dialogueNode.dialogueText,
                    characterSprite = dialogueNode.characterSprite,
                    variableName = dialogueNode.variableName,
                    compareValue = dialogueNode.compareValue,
                    conditionType = dialogueNode.conditionType,
                    actionType = dialogueNode.actionType,
                    position = dialogueNode.GetPosition().position
                });
            }

            return true;
        }

        public void LoadGraph(DialogueGraph targetGraph)
        {
            _containerCache = targetGraph;
            if (_containerCache == null)
            {
                EditorUtility.DisplayDialog("File Not Found", "Target dialogue graph file does not exist.", "OK");
                return;
            }

            ClearGraph();
            CreateNodes();
            ConnectNodes();
            LoadExposedProperties();
        }

        private void LoadExposedProperties()
        {
            _targetGraphView.ClearBlackboard();
            foreach (var exposedProperty in _containerCache.exposedProperties)
            {
                _targetGraphView.AddPropertyToBlackboard(exposedProperty);
            }
        }

        private void ClearGraph()
        {
            var entryNode = Nodes.FirstOrDefault(x => x.entryPoint);
            if (entryNode != null && _containerCache.nodeLinks.Count > 0)
            {
                entryNode.guid = _containerCache.nodeLinks[0].baseNodeGuid;
            }

            foreach (var node in Nodes)
            {
                if (node.entryPoint) continue;
                Edges.Where(x => x.input.node == node).ToList().ForEach(edge => _targetGraphView.RemoveElement(edge));
                _targetGraphView.RemoveElement(node);
            }
        }

        private void CreateNodes()
        {
            foreach (var nodeData in _containerCache.nodeData)
            {
                var tempNode = _targetGraphView.CreateDialogueNode(nodeData.dialogueText, nodeData.nodeType);
                tempNode.guid = nodeData.guid;
                tempNode.speakerName = nodeData.speakerName;
                tempNode.dialogueText = nodeData.dialogueText;
                tempNode.characterSprite = nodeData.characterSprite;
                tempNode.variableName = nodeData.variableName;
                tempNode.compareValue = nodeData.compareValue;
                tempNode.conditionType = nodeData.conditionType;
                tempNode.actionType = nodeData.actionType;
                
                // Atualizar campos visuais
                if (nodeData.nodeType == NodeType.Speech)
                {
                    tempNode.mainContainer.Q<TextField>("speaker-field")?.SetValueWithoutNotify(nodeData.speakerName);
                    tempNode.mainContainer.Q<TextField>("dialogue-field")?.SetValueWithoutNotify(nodeData.dialogueText);
                    tempNode.mainContainer.Q<ObjectField>()?.SetValueWithoutNotify(nodeData.characterSprite);
                }
                else if (nodeData.nodeType == NodeType.Condition || nodeData.nodeType == NodeType.Action)
                {
                    tempNode.mainContainer.Q<TextField>("Variable:")?.SetValueWithoutNotify(nodeData.variableName);
                    tempNode.mainContainer.Q<TextField>("Value:")?.SetValueWithoutNotify(nodeData.compareValue);
                    tempNode.mainContainer.Q<TextField>("Target Value:")?.SetValueWithoutNotify(nodeData.compareValue);
                }

                _targetGraphView.AddElement(tempNode);

                var nodePorts = _containerCache.nodeLinks.Where(x => x.baseNodeGuid == nodeData.guid).ToList();
                
                // Para Choice nodes, precisamos recriar as portas dinamicamente
                if (nodeData.nodeType == NodeType.Choice || nodeData.nodeType == NodeType.Speech)
                {
                    // Limpar portas padrão se necessário e adicionar as salvas
                    nodePorts.ForEach(x => _targetGraphView.AddChoicePort(tempNode, x.portName));
                }

                tempNode.SetPosition(new Rect(nodeData.position, _targetGraphView.DefaultNodeSize));
            }
        }

        private void ConnectNodes()
        {
            for (var i = 0; i < Nodes.Count; i++)
            {
                var connections = _containerCache.nodeLinks.Where(x => x.baseNodeGuid == Nodes[i].guid).ToList();
                for (var j = 0; j < connections.Count; j++)
                {
                    var targetNodeGuid = connections[j].targetNodeGuid;
                    var targetNode = Nodes.First(x => x.guid == targetNodeGuid);
                    LinkNodes(Nodes[i].outputContainer[j].Q<Port>(), (Port)targetNode.inputContainer[0]);

                    targetNode.SetPosition(new Rect(
                        _containerCache.nodeData.First(x => x.guid == targetNodeGuid).position,
                        _targetGraphView.DefaultNodeSize
                    ));
                }
            }
        }

        private void LinkNodes(Port output, Port input)
        {
            var tempEdge = new Edge
            {
                output = output,
                input = input
            };

            tempEdge.input.Connect(tempEdge);
            tempEdge.output.Connect(tempEdge);
            _targetGraphView.AddElement(tempEdge);
        }
    }
}
