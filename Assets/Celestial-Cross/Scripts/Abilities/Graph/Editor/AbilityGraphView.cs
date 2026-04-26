using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor
{
    public class AbilityGraphView : GraphView
    {
        public readonly Vector2 defaultNodeSize = new Vector2(150, 200);
        private AbilityNodeSearchWindow searchWindow;

        public AbilityGraphView(AbilityGraphWindow editorWindow)
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var grid = new GridBackground();
            grid.StretchToParentSize();
            Insert(0, grid);

            AddElement(GenerateEntryPointNode());
            AddSearchWindow(editorWindow);
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
