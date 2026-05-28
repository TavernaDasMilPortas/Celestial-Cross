using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using Celestial_Cross.Scripts.Abilities.Graph.Runtime;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes
{
    public class RamificationSpecNode : AbilityNode
    {
        private TextField specNameField;
        private TextField specDescriptionField;
        private ObjectField iconField;

        private RamificationSpecNodeData nodeData = new RamificationSpecNodeData();

        public override void Initialize(string nodeGuid, Vector2 position)
        {
            base.Initialize(nodeGuid, position);
            title = "Ramification Spec";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.6f, 0.3f, 0.6f, 0.9f));

            var inPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Input, Port.Capacity.Single, typeof(float));
            inPort.portName = "In";
            inputContainer.Add(inPort);

            var outPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Single, typeof(float));
            outPort.portName = "Out";
            outputContainer.Add(outPort);

            specNameField = new TextField("Name");
            specNameField.value = nodeData.specName;
            specNameField.RegisterValueChangedCallback(evt => nodeData.specName = evt.newValue);
            extensionContainer.Add(specNameField);

            specDescriptionField = new TextField("Description");
            specDescriptionField.multiline = true;
            specDescriptionField.style.minHeight = 40;
            specDescriptionField.value = nodeData.specDescription;
            specDescriptionField.RegisterValueChangedCallback(evt => nodeData.specDescription = evt.newValue);
            extensionContainer.Add(specDescriptionField);

            iconField = new ObjectField("Icon");
            iconField.objectType = typeof(Sprite);
            extensionContainer.Add(iconField);

            RefreshExpandedState();
            RefreshPorts();
        }

        public override string GetJsonData() => JsonUtility.ToJson(nodeData);

        public override void LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            nodeData = JsonUtility.FromJson<RamificationSpecNodeData>(json);
            
            specNameField.value = nodeData.specName;
            specDescriptionField.value = nodeData.specDescription;
        }

        public override void OnSave(AbilityNodeData data)
        {
            // Handle Icon saving via Dependency System if needed
            // Currently relies on the caller (GraphSaveUtility/AbilityGraphSO) 
            // to store the icon if we set up a dependency architecture.
            // A common pattern is AbilityGraphSO.Dependencies.
            
            // To properly save the sprite, we'd add it to SO's dependencies,
            // but we don't have direct access to the SO here without passing it.
            // For now, we will save the guid if we implement it later.
            // If the icon is set, we need to map it in AbilityGraphSO.
            
            // Getting the graph SO from the View or doing it in GraphSaveUtility.
            var graphView = GetFirstAncestorOfType<AbilityGraphView>();
            if (graphView != null)
            {
                var iconSprite = iconField.value as Sprite;
                if (iconSprite != null)
                {
                    // For simplicity in this mock, we just generate a generic ID if empty
                    if (string.IsNullOrEmpty(nodeData.iconDependencyId))
                    {
                        nodeData.iconDependencyId = "ramification_spec_" + GUID;
                    }
                    // Updating JSON with new ID
                    data.JsonData = JsonUtility.ToJson(nodeData);
                }
            }
        }

        public override void OnLoad(AbilityNodeData data)
        {
            // We'd load the Sprite from the AbilityGraphSO Dependencies list here
            // This is handled usually by the node interacting with the View or Interpreter.
        }

        public override string GetDescription()
        {
            return $"Spec: {nodeData.specName}";
        }
        
        // Helper method for GraphSaveUtility to read the sprite
        public Sprite GetIconSprite()
        {
            return iconField != null ? iconField.value as Sprite : null;
        }
        
        // Helper method for GraphSaveUtility to set the sprite
        public void SetIconSprite(Sprite sprite)
        {
            if (iconField != null) iconField.value = sprite;
        }
    }
}
