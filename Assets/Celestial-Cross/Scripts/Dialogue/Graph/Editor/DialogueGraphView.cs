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
            var node = CreateDialogueNode("Condition Check", NodeType.Condition, position);
            RebuildConditionPorts(node);
            AddElement(node);
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

            // Container para o campo de valor dinâmico
            var valueContainer = new VisualElement();
            valueContainer.name = "value-container";

            // Seletor de Tipo
            var typeField = new EnumField("Type:", property.type);
            typeField.RegisterValueChangedCallback(evt =>
            {
                var index = ExposedProperties.FindIndex(x => x.propertyName == property.propertyName);
                if (index >= 0)
                {
                    ExposedProperties[index].type = (PropertyType)evt.newValue;
                    field.typeText = evt.newValue.ToString();
                    // Recriar o campo de valor baseado no novo tipo
                    RebuildBlackboardValueField(valueContainer, property);
                }
            });

            var sa = new BlackboardRow(field, new VisualElement());
            sa.Add(typeField);
            sa.Add(valueContainer);
            Blackboard.Add(sa);

            // Criar o campo de valor inicial
            RebuildBlackboardValueField(valueContainer, property);
        }

        /// <summary>
        /// Reconstrói o campo de valor no Blackboard baseado no PropertyType.
        /// </summary>
        private void RebuildBlackboardValueField(VisualElement container, ExposedProperty property)
        {
            container.Clear();

            switch (property.type)
            {
                case PropertyType.Bool:
                    bool boolVal = property.propertyValue == "True" || property.propertyValue == "true";
                    var toggle = new Toggle("Value:") { value = boolVal };
                    toggle.RegisterValueChangedCallback(evt =>
                    {
                        property.propertyValue = evt.newValue.ToString();
                    });
                    container.Add(toggle);
                    break;

                case PropertyType.Int:
                    int intVal = 0;
                    int.TryParse(property.propertyValue, out intVal);
                    var intField = new IntegerField("Value:") { value = intVal };
                    intField.RegisterValueChangedCallback(evt =>
                    {
                        property.propertyValue = evt.newValue.ToString();
                    });
                    container.Add(intField);
                    break;

                default: // String
                    var textField = new TextField("Value:") { value = property.propertyValue ?? "" };
                    textField.RegisterValueChangedCallback(evt =>
                    {
                        property.propertyValue = evt.newValue;
                    });
                    container.Add(textField);
                    break;
            }
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
            varField.RegisterValueChangedCallback(evt =>
            {
                dialogueNode.variableName = evt.newValue;
                RebuildConditionPorts(dialogueNode);
            });
            varField.schedule.Execute(() => 
            {
                var currentVars = ExposedProperties.Select(x => x.propertyName).ToList();
                if (currentVars.Count == 0) currentVars.Add("No Variables");
                varField.choices = currentVars;
            }).Every(1000);
            dialogueNode.mainContainer.Add(varField);

            // Botão para adicionar portas (visível apenas para Int/String)
            var addBtn = new Button(() => AddNewConditionPort(dialogueNode))
            {
                text = "+ Condição",
                name = "condition-add-btn"
            };
            dialogueNode.titleContainer.Add(addBtn);
            // Portas serão criadas por RebuildConditionPorts ou RestoreConditionPorts
        }

        /// <summary>
        /// Reconstrói as portas de saída do Condition Node baseado no tipo da variável.
        /// Chamado quando o usuário muda a variável selecionada ou ao criar um novo nó.
        /// </summary>
        public void RebuildConditionPorts(DialogueNode node)
        {
            ClearOutputPorts(node);

            var selectedProp = ExposedProperties.FirstOrDefault(x => x.propertyName == node.variableName);
            var propType = selectedProp?.type ?? PropertyType.Bool;

            // Mostrar/esconder botão de adicionar baseado no tipo
            var addBtn = node.titleContainer.Q<Button>("condition-add-btn");

            switch (propType)
            {
                case PropertyType.Bool:
                    AddConditionPortFixed(node, "True");
                    AddConditionPortFixed(node, "False");
                    if (addBtn != null) addBtn.style.display = DisplayStyle.None;
                    break;

                case PropertyType.Int:
                    AddConditionPortInt(node, "==", 0);
                    AddConditionPortFixed(node, "Default");
                    if (addBtn != null)
                    {
                        addBtn.text = "+ Condição";
                        addBtn.style.display = DisplayStyle.Flex;
                    }
                    break;

                default: // String
                    AddConditionPortString(node, "valor");
                    AddConditionPortFixed(node, "Default");
                    if (addBtn != null)
                    {
                        addBtn.text = "+ Match";
                        addBtn.style.display = DisplayStyle.Flex;
                    }
                    break;
            }

            node.RefreshPorts();
            node.RefreshExpandedState();
        }

        /// <summary>
        /// Restaura portas de condição a partir de nomes salvos (usado durante o load).
        /// </summary>
        public void RestoreConditionPorts(DialogueNode node, List<string> portNames)
        {
            ClearOutputPorts(node);

            var selectedProp = ExposedProperties.FirstOrDefault(x => x.propertyName == node.variableName);
            var propType = selectedProp?.type ?? PropertyType.Bool;

            // Mostrar/esconder botão de adicionar
            var addBtn = node.titleContainer.Q<Button>("condition-add-btn");

            foreach (var portName in portNames)
            {
                if (portName == "True" || portName == "False" || portName == "Default")
                {
                    AddConditionPortFixed(node, portName);
                }
                else if (propType == PropertyType.Int)
                {
                    // Parsear formato "== 5"
                    var spaceIdx = portName.IndexOf(' ');
                    if (spaceIdx > 0)
                    {
                        string op = portName.Substring(0, spaceIdx);
                        int.TryParse(portName.Substring(spaceIdx + 1), out int val);
                        AddConditionPortInt(node, op, val);
                    }
                    else
                    {
                        AddConditionPortFixed(node, portName);
                    }
                }
                else // String
                {
                    AddConditionPortString(node, portName);
                }
            }

            if (addBtn != null)
            {
                if (propType == PropertyType.Bool)
                    addBtn.style.display = DisplayStyle.None;
                else
                {
                    addBtn.text = propType == PropertyType.Int ? "+ Condição" : "+ Match";
                    addBtn.style.display = DisplayStyle.Flex;
                }
            }

            node.RefreshPorts();
            node.RefreshExpandedState();
        }

        /// <summary>
        /// Adiciona nova porta de condição baseado no tipo da variável atual.
        /// </summary>
        private void AddNewConditionPort(DialogueNode node)
        {
            var selectedProp = ExposedProperties.FirstOrDefault(x => x.propertyName == node.variableName);
            var propType = selectedProp?.type ?? PropertyType.Bool;

            if (propType == PropertyType.Bool) return; // Bool só tem True/False

            if (propType == PropertyType.Int)
            {
                AddConditionPortInt(node, "==", 0, true);
            }
            else
            {
                AddConditionPortString(node, "", true);
            }
        }

        /// <summary>
        /// Limpa todas as portas de saída de um nó, desconectando edges.
        /// </summary>
        private void ClearOutputPorts(DialogueNode node)
        {
            var existingPorts = node.outputContainer.Query<Port>().ToList();
            foreach (var port in existingPorts)
            {
                var connectedEdges = edges.ToList().Where(e => e.output == port).ToList();
                foreach (var edge in connectedEdges)
                {
                    edge.input.Disconnect(edge);
                    RemoveElement(edge);
                }
                node.outputContainer.Remove(port);
            }
        }

        /// <summary>
        /// Porta fixa sem campos editáveis (True, False, Default).
        /// </summary>
        public Port AddConditionPortFixed(DialogueNode node, string portName)
        {
            var port = GeneratePort(node, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Single);
            port.portName = portName;
            node.outputContainer.Add(port);
            return port;
        }

        /// <summary>
        /// Porta de condição numérica com dropdown de operador e campo de valor.
        /// Nome da porta = "{operador} {valor}" (ex: "== 5", "< 10")
        /// </summary>
        public Port AddConditionPortInt(DialogueNode node, string op = "==", int value = 0, bool insertBeforeDefault = false)
        {
            var port = GeneratePort(node, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Single);

            var oldLabel = port.contentContainer.Q<Label>("type");
            if (oldLabel != null) port.contentContainer.Remove(oldLabel);

            var operators = new List<string> { "==", "!=", "<", ">", "<=", ">="  };
            int opIndex = operators.IndexOf(op);
            if (opIndex < 0) opIndex = 0;

            var opField = new DropdownField(operators, opIndex);
            opField.style.width = 60;

            var valField = new IntegerField() { value = value };
            valField.style.width = 50;

            port.portName = $"{op} {value}";

            opField.RegisterValueChangedCallback(evt => port.portName = $"{evt.newValue} {valField.value}");
            valField.RegisterValueChangedCallback(evt => port.portName = $"{opField.value} {evt.newValue}");

            port.contentContainer.Add(new Label(" "));
            port.contentContainer.Add(opField);
            port.contentContainer.Add(valField);

            var deleteBtn = new Button(() => RemovePort(node, port)) { text = "X" };
            port.contentContainer.Add(deleteBtn);

            if (insertBeforeDefault)
            {
                // Inserir antes da porta Default (ultimo elemento)
                int insertIndex = node.outputContainer.childCount > 0 ? node.outputContainer.childCount - 1 : 0;
                node.outputContainer.Insert(insertIndex, port);
            }
            else
            {
                node.outputContainer.Add(port);
            }

            node.RefreshPorts();
            node.RefreshExpandedState();
            return port;
        }

        /// <summary>
        /// Porta de condição de texto com campo editável.
        /// Nome da porta = texto digitado (ex: "sim", "não")
        /// </summary>
        public Port AddConditionPortString(DialogueNode node, string matchText = "", bool insertBeforeDefault = false)
        {
            var port = GeneratePort(node, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Single);

            var oldLabel = port.contentContainer.Q<Label>("type");
            if (oldLabel != null) port.contentContainer.Remove(oldLabel);

            var textField = new TextField { value = matchText };
            textField.RegisterValueChangedCallback(evt => port.portName = evt.newValue);

            port.portName = matchText;

            port.contentContainer.Add(new Label("  "));
            port.contentContainer.Add(textField);

            var deleteBtn = new Button(() => RemovePort(node, port)) { text = "X" };
            port.contentContainer.Add(deleteBtn);

            if (insertBeforeDefault)
            {
                int insertIndex = node.outputContainer.childCount > 0 ? node.outputContainer.childCount - 1 : 0;
                node.outputContainer.Insert(insertIndex, port);
            }
            else
            {
                node.outputContainer.Add(port);
            }

            node.RefreshPorts();
            node.RefreshExpandedState();
            return port;
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

            var typeField = new EnumField("Operation:", dialogueNode.actionType);
            typeField.RegisterValueChangedCallback(evt => dialogueNode.actionType = (ActionType)evt.newValue);

            // Container para o campo de valor dinâmico
            var valueContainer = new VisualElement();
            valueContainer.name = "action-value-container";

            var varField = new DropdownField("Variable:", choices, 0);
            varField.RegisterValueChangedCallback(evt =>
            {
                dialogueNode.variableName = evt.newValue;
                RebuildNodeValueField(valueContainer, dialogueNode, true);
            });
            varField.schedule.Execute(() => 
            {
                var currentVars = ExposedProperties.Select(x => x.propertyName).ToList();
                if (currentVars.Count == 0) currentVars.Add("No Variables");
                varField.choices = currentVars;
            }).Every(1000);

            dialogueNode.mainContainer.Add(varField);
            dialogueNode.mainContainer.Add(typeField);
            dialogueNode.mainContainer.Add(valueContainer);

            // Inicializar o campo de valor com o tipo correto
            RebuildNodeValueField(valueContainer, dialogueNode, true);

            var port = GeneratePort(dialogueNode, UnityEditor.Experimental.GraphView.Direction.Output);
            port.portName = "Next";
            dialogueNode.outputContainer.Add(port);
        }

        /// <summary>
        /// Reconstrói o campo de valor nos nós de Condition/Action baseado no tipo da variável selecionada.
        /// Público para ser acessado pelo GraphSaveUtility durante o carregamento.
        /// </summary>
        public void RebuildNodeValueFieldPublic(VisualElement container, DialogueNode node, bool isAction)
        {
            RebuildNodeValueField(container, node, isAction);
        }

        private void RebuildNodeValueField(VisualElement container, DialogueNode node, bool isAction)
        {
            container.Clear();

            // Encontrar o tipo da variável selecionada
            var selectedProp = ExposedProperties.FirstOrDefault(x => x.propertyName == node.variableName);
            var propType = selectedProp?.type ?? PropertyType.String;

            string label = isAction ? "Value:" : "Target Value:";

            switch (propType)
            {
                case PropertyType.Bool:
                    bool boolVal = node.compareValue == "True" || node.compareValue == "true";
                    var toggle = new Toggle(label) { value = boolVal };
                    toggle.RegisterValueChangedCallback(evt =>
                    {
                        node.compareValue = evt.newValue.ToString();
                    });
                    container.Add(toggle);
                    break;

                case PropertyType.Int:
                    int intVal = 0;
                    int.TryParse(node.compareValue, out intVal);
                    var intField = new IntegerField(label) { value = intVal };
                    intField.RegisterValueChangedCallback(evt =>
                    {
                        node.compareValue = evt.newValue.ToString();
                    });
                    container.Add(intField);
                    break;

                default: // String
                    var textField = new TextField(label) { value = node.compareValue ?? "" };
                    textField.RegisterValueChangedCallback(evt =>
                    {
                        node.compareValue = evt.newValue;
                    });
                    container.Add(textField);
                    break;
            }
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
