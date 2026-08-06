using System;
using System.Collections.Generic;
using CombatSimulator.Core.Models;

namespace CombatSimulator.Core.Interpreter.NodeProcessors
{
    public static class VariableProcessors
    {
        public static string ProcessModifier(NodeExport node, SimCombatContext context)
        {
            var data = HeadlessGraphInterpreter.Deserialize<VariableModifierNodeData>(node.JsonData);
            if (data == null || string.IsNullOrEmpty(data.variableName)) return "Out";

            float modVal = data.value;
            if (!string.IsNullOrEmpty(data.valueVariableReference) && context.Variables.TryGetValue(data.valueVariableReference, out float refVal))
                modVal = refVal;

            if (data.useCasterAttribute)
            {
                var source = context.GetSource();
                if (source != null)
                {
                    float statVal = DamageNodeProcessor.GetStatValue(source, data.casterAttribute);
                    modVal = statVal * (modVal / 100f);
                }
            }

            if (!context.Variables.ContainsKey(data.variableName))
                context.Variables[data.variableName] = 0;

            float current = context.Variables[data.variableName];
            context.Variables[data.variableName] = data.operation switch
            {
                VariableModifierNodeData.Operation.Set => modVal,
                VariableModifierNodeData.Operation.Add => current + modVal,
                VariableModifierNodeData.Operation.Multiply => current * modVal,
                VariableModifierNodeData.Operation.Divide => modVal != 0 ? current / modVal : current,
                _ => modVal
            };

            return "Out";
        }

        public static string ProcessUnitVariable(NodeExport node, SimCombatContext context)
        {
            var data = HeadlessGraphInterpreter.Deserialize<UnitVariableNodeData>(node.JsonData);
            if (data == null) return "Out";

            var source = context.GetSource();
            if (source == null) return "Out";

            string varKey = data.variable.ToString();

            if (data.operation == UnitVariableOperation.Get)
            {
                float readVal = source.UnitVariables.TryGetValue(varKey, out float v) ? v : 0f;
                if (!string.IsNullOrEmpty(data.outputVariable))
                    context.Variables[data.outputVariable] = readVal;
            }
            else
            {
                float writeVal = data.value;
                if (!string.IsNullOrEmpty(data.contextVariableReference) && context.Variables.TryGetValue(data.contextVariableReference, out float ctxVal))
                    writeVal = ctxVal;

                float currentVal = source.UnitVariables.TryGetValue(varKey, out float cv) ? cv : 0f;
                float newVal = data.operation switch
                {
                    UnitVariableOperation.Set => writeVal,
                    UnitVariableOperation.Add => currentVal + writeVal,
                    UnitVariableOperation.Subtract => currentVal - writeVal,
                    UnitVariableOperation.Multiply => currentVal * writeVal,
                    UnitVariableOperation.Divide => writeVal != 0 ? currentVal / writeVal : currentVal,
                    _ => writeVal
                };

                source.UnitVariables[varKey] = newVal;
            }

            return "Out";
        }
    }
}
