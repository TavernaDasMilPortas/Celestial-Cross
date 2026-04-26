using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor
{
    public class AbilityNodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private AbilityGraphView _graphView;
        private EditorWindow _window;

        public void Init(EditorWindow window, AbilityGraphView graphView)
        {
            _window = window;
            _graphView = graphView;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node"), 0),
                
                // Grupos
                new SearchTreeGroupEntry(new GUIContent("Context / Flow"), 1),
                new SearchTreeEntry(new GUIContent("Start Node")) { userData = typeof(StartNode), level = 2 },
                new SearchTreeEntry(new GUIContent("Trigger Event")) { userData = typeof(TriggerNode), level = 2 },
                new SearchTreeEntry(new GUIContent("Targeting")) { userData = typeof(TargetNode), level = 2 },
                new SearchTreeEntry(new GUIContent("Branch (Conditional)")) { userData = typeof(ConditionalFlowNode), level = 2 },
                new SearchTreeEntry(new GUIContent("Duration / Expiry")) { userData = typeof(DurationNode), level = 2 },

                new SearchTreeGroupEntry(new GUIContent("Effects"), 1),
                new SearchTreeEntry(new GUIContent("Damage Effect")) { userData = typeof(DamageEffectNode), level = 2 },
                new SearchTreeEntry(new GUIContent("Heal Effect")) { userData = typeof(HealEffectNode), level = 2 },
                new SearchTreeEntry(new GUIContent("Move Effect")) { userData = typeof(MoveEffectNode), level = 2 },
                new SearchTreeEntry(new GUIContent("Stat Modifier")) { userData = typeof(StatModifierEffectNode), level = 2 },
                new SearchTreeEntry(new GUIContent("Apply Status")) { userData = typeof(ApplyModifierNode), level = 2 },
                new SearchTreeEntry(new GUIContent("Cleanse / Remove Status")) { userData = typeof(CleanseStatusNode), level = 2 },
                new SearchTreeEntry(new GUIContent("VFX / Animation")) { userData = typeof(VfxNode), level = 2 },
                new SearchTreeEntry(new GUIContent("Cost / Cooldown")) { userData = typeof(CostNode), level = 2 },

                new SearchTreeGroupEntry(new GUIContent("Conditions / Data"), 1),
                new SearchTreeEntry(new GUIContent("Attribute Condition")) { userData = typeof(AttributeConditionNode), level = 2 },
                new SearchTreeEntry(new GUIContent("Distance & Faction")) { userData = typeof(DistanceConditionNode), level = 2 },
                new SearchTreeEntry(new GUIContent("Faction Condition")) { userData = typeof(FactionConditionNode), level = 2 },
                new SearchTreeEntry(new GUIContent("Range Count Condition")) { userData = typeof(RangeConditionNode), level = 2 },
                new SearchTreeEntry(new GUIContent("Speed Advantage")) { userData = typeof(SpeedAdvantageConditionNode), level = 2 },
                new SearchTreeEntry(new GUIContent("Turn Order Condition")) { userData = typeof(TurnOrderConditionNode), level = 2 }
            };

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            var windowMousePosition = _window.rootVisualElement.ChangeCoordinatesTo(_window.rootVisualElement.parent, context.screenMousePosition - _window.position.position);
            var graphMousePosition = _graphView.contentViewContainer.WorldToLocal(windowMousePosition);

            if (SearchTreeEntry.userData is Type type)
            {
                var node = Activator.CreateInstance(type) as AbilityNode;
                if (node != null)
                {
                    _graphView.AddElement(node);
                    node.Initialize(Guid.NewGuid().ToString(), graphMousePosition);
                    return true;
                }
            }

            return false;
        }
    }
}
