using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes
{
    public class VfxNode : AbilityNode
    {
        private ObjectField vfxField;
        private TextField sfxNameField;
        private EnumField spawnOriginDropdown;

        public enum SpawnOrigin { Source, Target, Ground }

        [System.Serializable]
        public class VfxData
        {
            public SpawnOrigin origin = SpawnOrigin.Target;
            public string sfxName = "";
        }

        private VfxData nodeData = new VfxData();

        public override void Initialize(string nodeGuid, Vector2 position)
        {
            base.Initialize(nodeGuid, position);
            title = "VFX / Animation";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.9f, 0.4f, 0.7f, 0.9f));

            var inputPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Input, Port.Capacity.Multi, typeof(float));
            inputPort.portName = "In";
            inputContainer.Add(inputPort);

            var outputPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Single, typeof(float));
            outputPort.portName = "Out";
            outputContainer.Add(outputPort);

            vfxField = new ObjectField("VFX Prefab") { objectType = typeof(GameObject) };
            extensionContainer.Add(vfxField);

            sfxNameField = new TextField("SFX Event Name");
            sfxNameField.RegisterValueChangedCallback(evt => nodeData.sfxName = evt.newValue);
            extensionContainer.Add(sfxNameField);

            spawnOriginDropdown = new EnumField("Spawn At", SpawnOrigin.Target);
            spawnOriginDropdown.RegisterValueChangedCallback(evt => nodeData.origin = (SpawnOrigin)evt.newValue);
            extensionContainer.Add(spawnOriginDropdown);

            RefreshExpandedState();
            RefreshPorts();
        }

        public override string GetJsonData() => JsonUtility.ToJson(nodeData);

        public override void LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            nodeData = JsonUtility.FromJson<VfxData>(json);
            sfxNameField.value = nodeData.sfxName;
            spawnOriginDropdown.value = nodeData.origin;
        }
    }
}
