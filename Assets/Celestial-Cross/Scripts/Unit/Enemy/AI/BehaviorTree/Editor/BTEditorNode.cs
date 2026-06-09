using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Editor
{
    public abstract class BTEditorNode : Node
    {
        public string Guid;
        public string NodeType;
        public string JsonData;

        public Port InputPort;
        public Port OutputPort;

        public virtual void Initialize(string nodeName, Vector2 position)
        {
            title = nodeName;
            Guid = System.Guid.NewGuid().ToString();
            NodeType = this.GetType().Name;
            SetPosition(new Rect(position, new Vector2(200, 150)));

            // Basic styling
            mainContainer.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f, 0.9f));
        }

        public virtual void LoadFromJson(string json)
        {
            JsonData = json;
        }

        public virtual string GetJsonData()
        {
            return JsonData;
        }

        protected void AddOutputPort(string portName, Port.Capacity capacity)
        {
            OutputPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, capacity, typeof(bool));
            OutputPort.portName = portName;
            outputContainer.Add(OutputPort);
        }

        protected void AddInputPort(string portName, Port.Capacity capacity)
        {
            InputPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Input, capacity, typeof(bool));
            InputPort.portName = portName;
            inputContainer.Add(InputPort);
        }

        protected Port AddDataOutputPort(string portName, Port.Capacity capacity)
        {
            var port = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, capacity, typeof(object));
            port.portName = portName;
            port.portColor = new Color(0.2f, 0.8f, 0.9f); // Cyan for data
            outputContainer.Add(port);
            return port;
        }

        protected Port AddDataInputPort(string portName, Port.Capacity capacity)
        {
            var port = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Input, capacity, typeof(object));
            port.portName = portName;
            port.portColor = new Color(0.2f, 0.8f, 0.9f); // Cyan for data
            inputContainer.Add(port);
            return port;
        }
    }
}
