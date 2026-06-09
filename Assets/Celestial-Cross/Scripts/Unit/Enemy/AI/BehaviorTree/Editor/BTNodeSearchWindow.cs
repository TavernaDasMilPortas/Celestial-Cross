using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Editor.Nodes;

namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Editor
{
    public class BTNodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private BTEditorWindow _window;
        private BTGraphView _graphView;

        public void Init(BTEditorWindow window, BTGraphView graphView)
        {
            _window = window;
            _graphView = graphView;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Criar Nó"), 0),

                new SearchTreeGroupEntry(new GUIContent("Root"), 1),
                new SearchTreeEntry(new GUIContent("BTRootNode")) { level = 2, userData = typeof(BTRootEditorNode) },

                new SearchTreeGroupEntry(new GUIContent("Composites (Controle)"), 1),
                new SearchTreeEntry(new GUIContent("Selector")) { level = 2, userData = typeof(BTSelectorEditorNode) },
                new SearchTreeEntry(new GUIContent("Sequence")) { level = 2, userData = typeof(BTSequenceEditorNode) },
                new SearchTreeEntry(new GUIContent("Switch (Dinâmico)")) { level = 2, userData = typeof(BTValueSwitchEditorNode) },

                new SearchTreeGroupEntry(new GUIContent("Decorators (Modificadores)"), 1),
                new SearchTreeEntry(new GUIContent("Inverter")) { level = 2, userData = typeof(BTInverterEditorNode) },
                new SearchTreeEntry(new GUIContent("Repeater")) { level = 2, userData = typeof(BTRepeaterEditorNode) },
                new SearchTreeEntry(new GUIContent("Cooldown")) { level = 2, userData = typeof(BTCooldownEditorNode) },
                new SearchTreeEntry(new GUIContent("Random Chance")) { level = 2, userData = typeof(BTRandomChanceEditorNode) },

                new SearchTreeGroupEntry(new GUIContent("Data (Provedores)"), 1),
                new SearchTreeEntry(new GUIContent("Obter Alvo (Get Target)")) { level = 2, userData = typeof(BTGetTargetEditorNode) },
                new SearchTreeEntry(new GUIContent("Obter Valor (Get Numeric Data)")) { level = 2, userData = typeof(BTGetNumericDataEditorNode) },

                new SearchTreeGroupEntry(new GUIContent("Actions (Folhas)"), 1),
                new SearchTreeEntry(new GUIContent("Mover/Aproximar/Recuar/Wander")) { level = 2, userData = typeof(BTActionMoveEditorNode) },
                new SearchTreeEntry(new GUIContent("Usar Habilidade/Atacar")) { level = 2, userData = typeof(BTActionUseAbilityEditorNode) },
                new SearchTreeEntry(new GUIContent("Esperar")) { level = 2, userData = typeof(BTActionWaitEditorNode) },

                new SearchTreeGroupEntry(new GUIContent("Conditions (Folhas)"), 1),
                new SearchTreeEntry(new GUIContent("Checar Valor Genérico")) { level = 2, userData = typeof(BTCheckValueEditorNode) },
                new SearchTreeEntry(new GUIContent("Habilidade Pronta?")) { level = 2, userData = typeof(BTConditionAbilityReadyEditorNode) },
                new SearchTreeEntry(new GUIContent("AoE Atingiria N+")) { level = 2, userData = typeof(BTConditionAoEHitCountEditorNode) },
                new SearchTreeEntry(new GUIContent("Alvo com Buff?")) { level = 2, userData = typeof(BTConditionTargetHasBuffEditorNode) },
                new SearchTreeEntry(new GUIContent("Alvo no Alcance")) { level = 2, userData = typeof(BTConditionTargetInRangeEditorNode) }
            };
            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            var type = SearchTreeEntry.userData as Type;
            var node = Activator.CreateInstance(type) as BTEditorNode;
            
            // Usamos context.screenMousePosition
            Vector2 graphMousePosition = _graphView.contentViewContainer.WorldToLocal(context.screenMousePosition);

            node.Initialize(SearchTreeEntry.name, graphMousePosition); 
            
            _graphView.CreateNode(node, graphMousePosition);
            return true;
        }
    }
}
