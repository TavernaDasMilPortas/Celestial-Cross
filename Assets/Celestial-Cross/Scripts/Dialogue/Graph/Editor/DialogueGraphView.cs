using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor.UIElements;

namespace CelestialCross.Dialogue.Graph.Editor
{
    public class DialogueGraphView : GraphView
    {
        public readonly Vector2 DefaultNodeSize = new Vector2(250, 300);
        public Blackboard Blackboard;
        public List<ExposedProperty> ExposedProperties = new List<ExposedProperty>();

        public DialogueGraphView()
        {
            var styleSheet = Resources.Load<StyleSheet>("DialogueGraph");
            if (styleSheet != null)
                styleSheets.Add(styleSheet);
                
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            AddElement(GenerateEntryPointNode());
        }

        public void ClearGraph()
        {
            foreach (var node in nodes.ToList().Cast<DialogueNode>())
            {
                if (node.entryPoint) continue;
                edges.Where(x => x.input.node == node).ToList().ForEach(edge => RemoveElement(edge));
                RemoveElement(node);
            }
            ClearBlackboard();
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            Vector2 mousePosition = viewTransform.matrix.inverse.MultiplyPoint(evt.localMousePosition);

            evt.menu.AppendAction("Create Node/Speech Node", action => CreateNode("Speech Node", mousePosition));
            evt.menu.AppendAction("Create Node/Choice Node", action => CreateChoiceNode(mousePosition));
            evt.menu.AppendAction("Create Node/Condition Node", action => CreateConditionNode(mousePosition));
            evt.menu.AppendAction("Create Node/Action Node", action => CreateActionNode(mousePosition));
            evt.menu.AppendAction("Create Node/End Node", action => CreateEndNode(mousePosition));
        }

        public void CreateChoiceNode(Vector2 position = default)
        {
            AddElement(CreateDialogueNode("Player Choices", NodeType.Choice, position));
        }

        public void CreateConditionNode(Vector2 position = default)
        {
            AddElement(CreateDialogueNode("Condition Check", NodeType.Condition, position));
        }

        public void CreateActionNode(Vector2 position = default)
        {
            AddElement(CreateDialogueNode("Set Variable", NodeType.Action, position));
        }

        public void CreateEndNode(Vector2 position = default)
        {
            AddElement(CreateDialogueNode("End Dialogue", NodeType.End, position));
        }
        public void ClearBlackboard()
        {
            ExposedProperties.Clear();
            Blackboard.Clear();
        }

        public void AddPropertyToBlackboard(ExposedProperty exposedProperty)
        {
            var localPropertyValue = exposedProperty.propertyValue;
            var localPropertyName = exposedProperty.propertyName;
            var localPropertyType = exposedProperty.type;

            while (ExposedProperties.Any(x => x.propertyName == localPropertyName))
            {
                localPropertyName = $"{localPropertyName}(1)";
            }

            var property = new ExposedProperty();
            property.propertyName = localPropertyName;
            property.propertyValue = localPropertyValue;
            property.type = localPropertyType;
            ExposedProperties.Add(property);

            var container = new VisualElement();
            var field = new BlackboardField { text = property.propertyName, typeText = property.type.ToString() };
            container.Add(field);

            var propertyValueField = new TextField("Value:")
            {
                value = localPropertyValue
            };
            propertyValueField.RegisterValueChangedCallback(evt =>
            {
                var index = ExposedProperties.FindIndex(x => x.propertyName == property.propertyName);
                ExposedProperties[index].propertyValue = evt.newValue;
            });

            // Seletor de Tipo
            var typeField = new EnumField("Type:", property.type);
            typeField.RegisterValueChangedCallback(evt =>
            {
                var index = ExposedProperties.FindIndex(x => x.propertyName == property.propertyName);
                ExposedProperties[index].type = (PropertyType)evt.newValue;
                field.typeText = evt.newValue.ToString();
            });

            var sa = new BlackboardRow(field, new VisualElement());
            sa.Add(typeField);
            sa.Add(propertyValueField);
            Blackboard.Add(sa);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            ports.ForEach(port =>
            {
                if (startPort != port && startPort.node != port.node)
                {
                    compatiblePorts.Add(port);
                }
            });
            return compatiblePorts;
        }

        private DialogueNode GenerateEntryPointNode()
        {
            var node = new DialogueNode
            {
                title = "START",
                guid = global::System.Guid.NewGuid().ToString(),
                dialogueText = "Entry Point",
                entryPoint = true
            };

            var generatedPort = GeneratePort(node, UnityEditor.Experimental.GraphView.Direction.Output);
            generatedPort.portName = "Next";
            node.outputContainer.Add(generatedPort);

            node.capabilities &= ~Capabilities.Movable;
            node.capabilities &= ~Capabilities.Deletable;

            node.RefreshExpandedState();
            node.RefreshPorts();

            node.SetPosition(new Rect(100, 200, 100, 150));
            return node;
        }

        public Port GeneratePort(DialogueNode node, UnityEditor.Experimental.GraphView.Direction portDirection, Port.Capacity capacity = Port.Capacity.Single)
        {
            return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float)); // Arbitrary type
        }

        public void CreateNode(string nodeName, Vector2 position = default)
        {
            AddElement(CreateDialogueNode(nodeName, NodeType.Speech, position));
        }

        public DialogueNode CreateDialogueNode(string nodeName, NodeType type = NodeType.Speech, Vector2 position = default)
        {
            var dialogueNode = new DialogueNode
            {
                title = nodeName,
                guid = global::System.Guid.NewGuid().ToString(),
                nodeType = type
            };

            var inputPort = GeneratePort(dialogueNode, UnityEditor.Experimental.GraphView.Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "Input";
            dialogueNode.inputContainer.Add(inputPort);

            if (type == NodeType.Speech)
            {
                BuildSpeechNode(dialogueNode);
            }
            else if (type == NodeType.Choice)
            {
                BuildChoiceNode(dialogueNode);
            }
            else if (type == NodeType.Condition)
            {
                BuildConditionNode(dialogueNode);
            }
            else if (type == NodeType.Action)
            {
                BuildActionNode(dialogueNode);
            }
            else if (type == NodeType.End)
            {
                BuildEndNode(dialogueNode);
            }

            dialogueNode.RefreshExpandedState();
            dialogueNode.RefreshPorts();
            dialogueNode.SetPosition(new Rect(position, DefaultNodeSize));

            return dialogueNode;
        }

        private void BuildSpeechNode(DialogueNode dialogueNode)
        {
            // --- CAMPOS DE DADOS ---
            // Nome do Personagem
            var speakerField = new TextField("Persona:");
            speakerField.name = "speaker-field";
            speakerField.RegisterValueChangedCallback(evt => dialogueNode.speakerName = evt.newValue);
            speakerField.SetValueWithoutNotify(dialogueNode.speakerName);
            dialogueNode.mainContainer.Add(speakerField);

            // Texto do Diálogo
            var dialogueField = new TextField("Dialogue:");
            dialogueField.name = "dialogue-field";
            dialogueField.multiline = true;
            dialogueField.RegisterValueChangedCallback(evt => dialogueNode.dialogueText = evt.newValue);
            dialogueField.SetValueWithoutNotify(dialogueNode.dialogueText);
            dialogueNode.mainContainer.Add(dialogueField);

            // Sprite
            var spriteField = new UnityEditor.UIElements.ObjectField("Sprite:")
            {
                objectType = typeof(Sprite),
                allowSceneObjects = false
            };
            spriteField.RegisterValueChangedCallback(evt => dialogueNode.characterSprite = evt.newValue as Sprite);
            spriteField.SetValueWithoutNotify(dialogueNode.characterSprite);
            dialogueNode.mainContainer.Add(spriteField);

            // Removemos o botão de escolhas múltiplas daqui, pois usaremos o Choice Node
            var generatedPort = GeneratePort(dialogueNode, UnityEditor.Experimental.GraphView.Direction.Output);
            generatedPort.portName = "Next";
            dialogueNode.outputContainer.Add(generatedPort);
        }

        private void BuildConditionNode(DialogueNode dialogueNode)
        {
            dialogueNode.title = "Condition Check";

            var choices = ExposedProperties.Select(x => x.propertyName).ToList();
            if (choices.Count == 0) choices.Add("No Variables");

            var varField = new DropdownField("Variable:", choices, 0);
            varField.RegisterValueChangedCallback(evt => dialogueNode.variableName = evt.newValue);
            varField.schedule.Execute(() => 
            {
                var currentVars = ExposedProperties.Select(x => x.propertyName).ToList();
                if (currentVars.Count == 0) currentVars.Add("No Variables");
                varField.choices = currentVars;
            }).Every(1000); // Atualiza os choices de tempo em tempo caso mude no blackboard
            dialogueNode.mainContainer.Add(varField);

            var typeField = new EnumField("Type:", ConditionType.Equals);
            typeField.RegisterValueChangedCallback(evt => dialogueNode.conditionType = (ConditionType)evt.newValue);
            dialogueNode.mainContainer.Add(typeField);

            var valueField = new TextField("Target Value:");
            valueField.RegisterValueChangedCallback(evt => dialogueNode.compareValue = evt.newValue);
            dialogueNode.mainContainer.Add(valueField);
            
            AddChoicePort(dialogueNode, "True");
            AddChoicePort(dialogueNode, "False");
        }

        private void BuildChoiceNode(DialogueNode dialogueNode)
        {
            dialogueNode.title = "Player Choices";

            // Botão para múltiplas portas de ENTRADA (conforme solicitado para facilitar visualização de loops)
            var addInputBtn = new Button(() => { AddChoicePort(dialogueNode, "Input Route", UnityEditor.Experimental.GraphView.Direction.Input); });
            addInputBtn.text = "+ Entrada";
            dialogueNode.titleContainer.Add(addInputBtn);

            var button = new Button(() => { AddChoicePort(dialogueNode); });
            button.text = "Add Option";
            dialogueNode.titleContainer.Add(button);
        }

        private void BuildActionNode(DialogueNode dialogueNode)
        {
            dialogueNode.title = "Set Variable";

            var choices = ExposedProperties.Select(x => x.propertyName).ToList();
            if (choices.Count == 0) choices.Add("No Variables");

            var varField = new DropdownField("Variable:", choices, 0);
            varField.RegisterValueChangedCallback(evt => dialogueNode.variableName = evt.newValue);
            varField.schedule.Execute(() => 
            {
                var currentVars = ExposedProperties.Select(x => x.propertyName).ToList();
                if (currentVars.Count == 0) currentVars.Add("No Variables");
                varField.choices = currentVars;
            }).Every(1000); // Atualiza dinamicamente as variáveis disponíveis

            dialogueNode.mainContainer.Add(varField);

            var typeField = new EnumField("Operation:", ActionType.Set);
            typeField.RegisterValueChangedCallback(evt => dialogueNode.actionType = (ActionType)evt.newValue);
            dialogueNode.mainContainer.Add(typeField);

            var valueField = new TextField("Value:");
            valueField.RegisterValueChangedCallback(evt => dialogueNode.compareValue = evt.newValue);
            dialogueNode.mainContainer.Add(valueField);

            var port = GeneratePort(dialogueNode, UnityEditor.Experimental.GraphView.Direction.Output);
            port.portName = "Next";
            dialogueNode.outputContainer.Add(port);
        }

        private void BuildEndNode(DialogueNode dialogueNode)
        {
            dialogueNode.title = "END";
            // No outputs
        }

        public void AddChoicePort(DialogueNode dialogueNode, string overriddenPortName = "", UnityEditor.Experimental.GraphView.Direction direction = UnityEditor.Experimental.GraphView.Direction.Output)
        {
            var generatedPort = GeneratePort(dialogueNode, direction, Port.Capacity.Multi);

            var oldLabel = generatedPort.contentContainer.Q<Label>("type");
            generatedPort.contentContainer.Remove(oldLabel);

            var portCount = (direction == UnityEditor.Experimental.GraphView.Direction.Output) 
                ? dialogueNode.outputContainer.Query("connector").ToList().Count 
                : dialogueNode.inputContainer.Query("connector").ToList().Count - 1; // -1 to ignore default Input

            var defaultName = (direction == UnityEditor.Experimental.GraphView.Direction.Output) ? $"Choice {portCount + 1}" : $"Route {portCount + 1}";
            var choicePortName = string.IsNullOrEmpty(overriddenPortName) ? defaultName : overriddenPortName;

            var textField = new TextField
            {
                name = string.Empty,
                value = choicePortName
            };
            textField.RegisterValueChangedCallback(evt => generatedPort.portName = evt.newValue);
            generatedPort.contentContainer.Add(new Label("  "));
            generatedPort.contentContainer.Add(textField);
            var deleteButton = new Button(() => RemovePort(dialogueNode, generatedPort, direction))
            {
                text = "X"
            };
            generatedPort.contentContainer.Add(deleteButton);

            generatedPort.portName = choicePortName;

            if (direction == UnityEditor.Experimental.GraphView.Direction.Output)
                dialogueNode.outputContainer.Add(generatedPort);
            else
                dialogueNode.inputContainer.Add(generatedPort);

            dialogueNode.RefreshPorts();
            dialogueNode.RefreshExpandedState();
        }

        private void RemovePort(DialogueNode dialogueNode, Port generatedPort, UnityEditor.Experimental.GraphView.Direction direction = UnityEditor.Experimental.GraphView.Direction.Output)
        {
            var targetEdge = edges.ToList().Where(x => (direction == UnityEditor.Experimental.GraphView.Direction.Output) ? x.output == generatedPort : x.input == generatedPort);

            if (targetEdge.Any())
            {
                var edge = targetEdge.First();
                if (direction == UnityEditor.Experimental.GraphView.Direction.Output)
                    edge.input.Disconnect(edge);
                else
                    edge.output.Disconnect(edge);
                RemoveElement(edge);
            }

            if (direction == UnityEditor.Experimental.GraphView.Direction.Output)
                dialogueNode.outputContainer.Remove(generatedPort);
            else
                dialogueNode.inputContainer.Remove(generatedPort);

            dialogueNode.RefreshPorts();
            dialogueNode.RefreshExpandedState();
        }
    }
}
