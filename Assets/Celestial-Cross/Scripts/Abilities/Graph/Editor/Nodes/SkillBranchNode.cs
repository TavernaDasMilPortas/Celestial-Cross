using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Celestial_Cross.Scripts.Abilities.Graph.Runtime;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes
{
    public class SkillBranchNode : AbilityNode
    {
        private TextField branchIdField;
        private IntegerField tierIndexField;
        private TextField branchNameField;
        private TextField branchDescriptionField;
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

            tierIndexField = new IntegerField("Tier Index");
            tierIndexField.value = nodeData.tierIndex;
            tierIndexField.RegisterValueChangedCallback(evt => nodeData.tierIndex = evt.newValue);
            extensionContainer.Add(tierIndexField);

            branchNameField = new TextField("Nome (UI)");
            branchNameField.value = nodeData.branchName;
            branchNameField.RegisterValueChangedCallback(evt => nodeData.branchName = evt.newValue);
            extensionContainer.Add(branchNameField);

            branchDescriptionField = new TextField("Descrição (UI)");
            branchDescriptionField.multiline = true;
            branchDescriptionField.value = nodeData.branchDescription;
            branchDescriptionField.RegisterValueChangedCallback(evt => nodeData.branchDescription = evt.newValue);
            extensionContainer.Add(branchDescriptionField);

            RefreshExpandedState();
            RefreshPorts();
        }

        public override string GetJsonData() => JsonUtility.ToJson(nodeData);

        public override void LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            nodeData = JsonUtility.FromJson<SkillBranchNodeData>(json);
            branchIdField.value = nodeData.branchId;
            tierIndexField.value = nodeData.tierIndex;
            branchNameField.value = nodeData.branchName ?? "";
            branchDescriptionField.value = nodeData.branchDescription ?? "";
        }

        public override string GetDescription()
        {
            string name = string.IsNullOrEmpty(nodeData.branchName) ? nodeData.branchId : nodeData.branchName;
            return $"Desvia o fluxo se o ramo '{name}' (Tier {nodeData.tierIndex}) estiver ativo na unidade.";
        }
    }
}
