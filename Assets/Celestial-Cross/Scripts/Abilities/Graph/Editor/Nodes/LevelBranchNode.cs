using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Celestial_Cross.Scripts.Abilities.Graph.Runtime;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes
{
    public class LevelBranchNode : AbilityNode
    {
        private LevelBranchNodeData nodeData = new LevelBranchNodeData();

        public override void Initialize(string nodeGuid, Vector2 position)
        {
            base.Initialize(nodeGuid, position);
            title = "Level Branch";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.5f, 0.2f, 0.5f, 0.9f));

            var inPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Input, Port.Capacity.Multi, typeof(float));
            inPort.portName = "In";
            inputContainer.Add(inPort);

            var addLevelButton = new Button(() => {
                nodeData.levelCount++;
                AddLevelPort(nodeData.levelCount);
                RefreshPorts();
                RefreshExpandedState();
            }) { text = "Add Level" };
            extensionContainer.Add(addLevelButton);

            RebuildPorts();

            RefreshExpandedState();
            RefreshPorts();
        }

        private void RebuildPorts()
        {
            outputContainer.Clear();
            for (int i = 1; i <= nodeData.levelCount; i++)
            {
                AddLevelPort(i);
            }
        }

        private void AddLevelPort(int level)
        {
            var port = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Single, typeof(float));
            port.portName = $"Level {level}";
            outputContainer.Add(port);
        }

        public override string GetJsonData() => JsonUtility.ToJson(nodeData);

        public override void LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            nodeData = JsonUtility.FromJson<LevelBranchNodeData>(json);
            RebuildPorts();
        }

        public override string GetDescription()
        {
            return "Desvia o fluxo dependendo do nível atual da habilidade.";
        }
    }
}
