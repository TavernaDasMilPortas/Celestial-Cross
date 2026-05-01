using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor
{
    public class AbilityGraphView : GraphView
    {
        public readonly Vector2 defaultNodeSize = new Vector2(150, 200);
        private AbilityNodeSearchWindow searchWindow;
        private Blackboard blackboard;
        private AbilityGraphSO currentSO;

        public AbilityGraphView(AbilityGraphWindow editorWindow)
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var grid = new GridBackground();
            grid.StretchToParentSize();
            Insert(0, grid);

            AddSearchWindow(editorWindow);
            AddBlackboard();
        }

        private void AddBlackboard()
        {
            blackboard = new Blackboard(this);
            blackboard.Add(new BlackboardSection { title = "Variables" });
            blackboard.addItemRequested = _ => { AddVariableToSO(); };
            blackboard.editTextRequested = (bb, element, newValue) => { RenameVariable(element, newValue); };
            
            // Posicionamento
            blackboard.SetPosition(new Rect(10, 30, 200, 300));
            Add(blackboard);
        }

        public void SetGraphAsset(AbilityGraphSO so)
        {
            currentSO = so;
            RefreshBlackboard();
        }

        private void AddVariableToSO()
        {
            if (currentSO == null) return;
            string varName = "NewVariable_" + currentSO.Variables.Count;
            currentSO.Variables.Add(new AbilityGraphSO.GraphVariable { name = varName, initialValue = 0 });
            RefreshBlackboard();
        }

        private void RenameVariable(VisualElement element, string newName)
        {
            if (currentSO == null) return;
            var field = element as BlackboardField;
            var variable = currentSO.Variables.Find(v => v.name == field.text);
            if (variable != null)
            {
                variable.name = newName;
                RefreshBlackboard();
            }
        }

        public void RefreshBlackboard()
        {
            blackboard.Clear();
            var section = new BlackboardSection { title = "Variables" };
            blackboard.Add(section);

            if (currentSO == null) return;

            foreach (var variable in currentSO.Variables)
            {
                var field = new BlackboardField(null, variable.name, "float");
                section.Add(field);
            }
        }

        public void HandleAutoVariableGeneration(AbilityNode node)
        {
            if (currentSO == null) return;
            string shortGuid = node.GUID.Substring(0, Mathf.Min(4, node.GUID.Length));

            if (node is DamageEffectNode dmgNode)
            {
                string varName = EnsureVariableExists("dmg_amount_" + shortGuid, 10);
                dmgNode.SetVariableReference(varName);
            }
            else if (node is HealEffectNode healNode)
            {
                string varName = EnsureVariableExists("heal_amount_" + shortGuid, 10);
                healNode.SetVariableReference(varName);
            }
            else if (node is CostNode costNode)
            {
                string varName1 = EnsureVariableExists("mana_cost_" + shortGuid, 5);
                string varName2 = EnsureVariableExists("stamina_cost_" + shortGuid, 5);
                costNode.SetVariableReference(varName1, varName2);
            }
            else if (node is StatModifierEffectNode statNode)
            {
                string varName = EnsureVariableExists("buff_base_" + shortGuid, 1);
                statNode.SetVariableReference(varName);
            }
            else if (node is TargetNode targetNode)
            {
                string varName = EnsureVariableExists("max_targets_" + shortGuid, 1);
                targetNode.SetVariableReference(varName);
            }
        }

        private string EnsureVariableExists(string baseName, float defaultValue)
        {
            if (currentSO == null) return baseName;

            if (!currentSO.Variables.Any(v => v.name == baseName))
            {
                currentSO.Variables.Add(new AbilityGraphSO.GraphVariable { name = baseName, initialValue = defaultValue });
                RefreshBlackboard();
            }
            return baseName;
        }

        private void AddSearchWindow(AbilityGraphWindow editorWindow)
        {
            searchWindow = ScriptableObject.CreateInstance<AbilityNodeSearchWindow>();
            searchWindow.Init(editorWindow, this);
            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            ports.ForEach((port) =>
            {
                if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
                {
                    compatiblePorts.Add(port);
                }
            });
            return compatiblePorts;
        }

        private StartNode GenerateEntryPointNode()
        {
            var node = new StartNode();
            node.Initialize(Guid.NewGuid().ToString(), new Vector2(100, 200));
            return node;
        }

        /// <summary>
        /// Semeia um grafo vazio com StartNode + LevelBranchNode conectados.
        /// Chamado ao abrir um grafo que não possui nenhum nó.
        /// </summary>
        public void SeedDefaultNodes()
        {
            // Criar StartNode
            var startNode = new StartNode();
            startNode.Initialize(Guid.NewGuid().ToString(), new Vector2(100, 200));
            AddElement(startNode);

            // Criar LevelBranchNode
            var levelNode = new LevelBranchNode();
            levelNode.Initialize(Guid.NewGuid().ToString(), new Vector2(450, 200));
            AddElement(levelNode);

            // Conectar Start.Out -> LevelBranch.In
            var startOutPort = startNode.outputContainer.Q<Port>();
            var levelInPort = levelNode.inputContainer.Q<Port>();

            if (startOutPort != null && levelInPort != null)
            {
                var edge = startOutPort.ConnectTo(levelInPort);
                AddElement(edge);
            }

            Debug.Log("[AbilityGraphView] Grafo vazio semeado com StartNode + LevelBranchNode.");
        }

        public void CreateNode(string nodeName, Vector2 position)
        {
            // Para testes, vamos sempre criar um Damage Node quando clica em Create Node
            AddElement(CreateDamageNode(position));
        }

        public DamageEffectNode CreateDamageNode(Vector2 position)
        {
            var damageNode = new DamageEffectNode();
            damageNode.Initialize(Guid.NewGuid().ToString(), position);
            return damageNode;
        }
    }
}
