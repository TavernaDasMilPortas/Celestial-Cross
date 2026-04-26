using System;
using UnityEngine;
using Celestial_Cross.Scripts.Abilities.Graph;
using Celestial_Cross.Scripts.Combat.Execution;
using CelestialCross.Combat;

namespace Celestial_Cross.Scripts.Units
{
    public class GraphActionWrapper : IUnitAction
    {
        private readonly global::Unit caster;
        public readonly AbilityGraphSO Graph;

        public string ActionName => string.IsNullOrEmpty(Graph.abilityName) ? Graph.name : Graph.abilityName;
        public Sprite ActionIcon => Graph.abilityIcon;
        public string ActionDescription => Graph.abilityDescription;
        public int Range => Graph.displayRange;

        public int Level { get; set; } = 1;
        public Vector2Int Target { get; set; }

        public event Action<ActionForecast> OnForecastUpdated;

        public GraphActionWrapper(global::Unit caster, AbilityGraphSO graph)
        {
            this.caster = caster;
            this.Graph = graph;
        }

        public AreaPatternData GetAreaPattern()
        {
            // O sistema de grafo pode ter múltiplos padrões ou dinâmicos.
            // Para UI, tentamos achar o primeiro TargetNode e seu padrão.
            foreach (var node in Graph.NodeData)
            {
                if (node.NodeType == "TargetNode")
                {
                    var targetData = JsonUtility.FromJson<Celestial_Cross.Scripts.Abilities.Graph.Runtime.TargetNodeData>(node.JsonData);
                    AreaPatternData pattern = Graph.GetAsset<AreaPatternData>(targetData.patternReferenceId);
                    if (pattern == null) pattern = node.areaPattern;
                    return pattern;
                }
            }
            return null;
        }

        public void EnterAction()
        {
            if (AbilityExecutor.Instance != null)
            {
                AbilityExecutor.Instance.ExecuteGraph(caster, Graph, CombatHook.OnManualCast, () => {
                    CameraController.Instance?.ResetFocus();
                    if (caster is global::EnemyUnit)
                        TurnManager.Instance.EndTurn();
                    else    
                        PlayerController.Instance.EndTurn();
                }, Level);
            }
            else
            {
                Debug.LogError("[GraphActionWrapper] AbilityExecutor não encontrado na cena!");
            }
        }

        public void UpdateAction() { }

        public void Confirm() { }

        public void Cancel()
        {
            if (AbilityExecutor.Instance != null)
            {
                AbilityExecutor.Instance.AbortCurrentAbility();
            }
        }

        public string GetDetailStats()
        {
            return $"Range: {Range}";
        }
    }
}
