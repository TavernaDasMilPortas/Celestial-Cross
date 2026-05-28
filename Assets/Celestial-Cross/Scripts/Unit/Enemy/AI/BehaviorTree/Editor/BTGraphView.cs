using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Editor
{
    public class BTGraphView : GraphView
    {
        private EditorWindow _window;
        private BTNodeSearchWindow _searchWindow;
        public BehaviorTreeSO currentAsset;

        public BTGraphView(EditorWindow window)
        {
            _window = window;

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            AddSearchWindow();
        }

        private void AddSearchWindow()
        {
            _searchWindow = ScriptableObject.CreateInstance<BTNodeSearchWindow>();
            _searchWindow.Init(_window as BTEditorWindow, this); // Can cast to null if it's the Wizard, handled inside
            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
        }

        public void SetGraphAsset(BehaviorTreeSO asset)
        {
            currentAsset = asset;
        }

        public override System.Collections.Generic.List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new System.Collections.Generic.List<Port>();
            ports.ForEach((port) =>
            {
                if (startPort != port && startPort.node != port.node && startPort.direction != port.direction && startPort.portType == port.portType)
                {
                    compatiblePorts.Add(port);
                }
            });
            return compatiblePorts;
        }

        public void CreateNode(BTEditorNode node, Vector2 position)
        {
            node.SetPosition(new Rect(position, Vector2.zero));
            AddElement(node);
        }

        public void ClearGraph()
        {
            graphElements.ForEach(RemoveElement);
        }
    }
}
