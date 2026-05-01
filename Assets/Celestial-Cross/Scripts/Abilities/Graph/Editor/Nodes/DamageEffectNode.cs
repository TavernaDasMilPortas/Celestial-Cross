using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Celestial_Cross.Scripts.Abilities.Conditions;
using Celestial_Cross.Scripts.Abilities.Graph.Runtime;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes
{
    public class DamageEffectNode : AbilityNode
    {
        private EnumField baseAttributeDropdown;
        private Toggle scaleWithDistanceToggle;
        private FloatField distanceScaleFactorField;
        private TextField variableReferenceField;

        private Celestial_Cross.Scripts.Abilities.Graph.Runtime.DamageNodeData nodeData = new Celestial_Cross.Scripts.Abilities.Graph.Runtime.DamageNodeData();

        public override void Initialize(string nodeGuid, Vector2 position)
        {
            base.Initialize(nodeGuid, position);
            title = "Damage Effect";
            
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.8f, 0.2f, 0.2f, 0.9f));

            // Porta de Entrada
            var inputPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Input, Port.Capacity.Multi, typeof(float));
            inputPort.portName = "In";
            inputContainer.Add(inputPort);

            // Porta de Saída
            var outputPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Single, typeof(float));
            outputPort.portName = "Out";
            outputContainer.Add(outputPort);

            // UI Elements
            variableReferenceField = new TextField("Multiplier Var");
            variableReferenceField.value = nodeData.variableReference;
            variableReferenceField.RegisterValueChangedCallback(evt => nodeData.variableReference = evt.newValue);
            extensionContainer.Add(variableReferenceField);

            baseAttributeDropdown = new EnumField("Base Attribute", AttributeCondition.AttributeType.Attack);
            baseAttributeDropdown.RegisterValueChangedCallback(evt => {
                nodeData.baseAttribute = (int)(AttributeCondition.AttributeType)evt.newValue;
            });

            scaleWithDistanceToggle = new Toggle("Scale With Distance");
            scaleWithDistanceToggle.value = nodeData.scaleWithDistance;
            scaleWithDistanceToggle.RegisterValueChangedCallback(evt => {
                nodeData.scaleWithDistance = evt.newValue;
                UpdateDynamicFields();
            });
            extensionContainer.Add(scaleWithDistanceToggle);

            distanceScaleFactorField = new FloatField("Distance Factor");
            distanceScaleFactorField.value = nodeData.distanceScaleFactor;
            distanceScaleFactorField.RegisterValueChangedCallback(evt => nodeData.distanceScaleFactor = evt.newValue);

            UpdateDynamicFields();

            RefreshExpandedState();
            RefreshPorts();
        }

        private void UpdateDynamicFields()
        {
            if (!extensionContainer.Contains(baseAttributeDropdown))
                extensionContainer.Insert(0, baseAttributeDropdown);

            // Lógica dinâmica para Scale With Distance
            if (nodeData.scaleWithDistance)
            {
                if (!extensionContainer.Contains(distanceScaleFactorField))
                    extensionContainer.Add(distanceScaleFactorField);
            }
            else
            {
                if (extensionContainer.Contains(distanceScaleFactorField))
                    extensionContainer.Remove(distanceScaleFactorField);
            }

            RefreshExpandedState();
        }

        public override string GetJsonData()
        {
            return JsonUtility.ToJson(nodeData);
        }

        public override void LoadFromJson(string json)
        {
            if (!string.IsNullOrEmpty(json))
            {
                nodeData = JsonUtility.FromJson<Celestial_Cross.Scripts.Abilities.Graph.Runtime.DamageNodeData>(json);
                variableReferenceField.value = nodeData.variableReference;
                scaleWithDistanceToggle.value = nodeData.scaleWithDistance;
                distanceScaleFactorField.value = nodeData.distanceScaleFactor;
                UpdateDynamicFields();
            }
        }

        public override string GetDescription()
        {
            string scaleText = nodeData.scaleWithDistance ? " (escala com a distância)" : "";
            var attr = (AttributeCondition.AttributeType)nodeData.baseAttribute;
            return $"Dano escalado pelo atributo {attr} multiplicado pela variável '{nodeData.variableReference}'{scaleText}.";
        }

        public void SetVariableReference(string varName)
        {
            nodeData.variableReference = varName;
            if (variableReferenceField != null) variableReferenceField.value = varName;
        }
    }
}
