using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Celestial_Cross.Scripts.Abilities.Graph.Runtime;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes
{
    public class SkillBranchNode : AbilityNode
    {
        private TextField branchIdField;
        private SkillBranchNodeData nodeData = new SkillBranchNodeData();

        public override void Initialize(string nodeGuid, Vector2 position)
        {
            base.Initialize(nodeGuid, position);
            title = "Skill Branch Check";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.1f, 0.6f, 0.4f, 0.9f));

            // Input Port
            var inPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Input, Port.Capacity.Multi, typeof(float));
            inPort.portName = "In";
            inputContainer.Add(inPort);

            // Output Ports
            var activeOutPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Single, typeof(float));
            activeOutPort.portName = "Active";
            outputContainer.Add(activeOutPort);

            var inactiveOutPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Single, typeof(float));
            inactiveOutPort.portName = "Inactive";
            outputContainer.Add(inactiveOutPort);

            // Fields
            branchIdField = new TextField("Branch ID");
            branchIdField.value = nodeData.branchId;
            branchIdField.RegisterValueChangedCallback(evt => nodeData.branchId = evt.newValue);
            extensionContainer.Add(branchIdField);

            RefreshExpandedState();
            RefreshPorts();
        }

        public override string GetJsonData() => JsonUtility.ToJson(nodeData);

        public override void LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            nodeData = JsonUtility.FromJson<SkillBranchNodeData>(json);
            branchIdField.value = nodeData.branchId;
        }

        public override string GetDescription()
        {
            return $"Desvia o fluxo se o ramo '{nodeData.branchId}' estiver ativo na unidade.";
        }
    }
}
