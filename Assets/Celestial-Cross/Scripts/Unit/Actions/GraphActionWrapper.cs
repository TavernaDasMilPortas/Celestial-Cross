using System;
using System.Linq;
using UnityEngine;
using Celestial_Cross.Scripts.Abilities.Graph;
using Celestial_Cross.Scripts.Combat.Execution;
using CelestialCross.Combat;
using Celestial_Cross.Scripts.Units.Enemy;

namespace Celestial_Cross.Scripts.Units
{
    public class GraphActionWrapper : IUnitAction
    {
        private readonly global::Unit caster;
        public readonly AbilityGraphSO Graph;

        public string ActionName => string.IsNullOrEmpty(Graph.abilityName) ? Graph.name : Graph.abilityName;
        public Sprite ActionIcon => Graph.abilityIcon;
        public string ActionDescription => Graph.abilityDescription;
        public int Range
        {
            get
            {
                if (Graph != null && Graph.NodeData != null)
                {
                    foreach (var node in Graph.NodeData)
                    {
                        if (node.NodeType == "TargetNode")
                        {
                            var targetData = JsonUtility.FromJson<Celestial_Cross.Scripts.Abilities.Graph.Runtime.TargetNodeData>(node.JsonData);
                            if (targetData != null && targetData.range > 0)
                                return targetData.range;
                        }
                        if (node.NodeType == "MoveEffectNode")
                        {
                            var moveData = JsonUtility.FromJson<Celestial_Cross.Scripts.Abilities.Graph.Runtime.MoveEffectNodeData>(node.JsonData);
                            if (moveData != null && moveData.range > 0)
                                return moveData.range;
                        }
                    }
                }
                return Graph != null && Graph.displayRange > 0 ? Graph.displayRange : 1;
            }
        }

        public int Level { get; set; } = 1;

        private Vector2Int target = new Vector2Int(-999, -999);
        public Vector2Int Target { get => target; set => target = value; }
        public string SlotId { get; set; } = "";

        public event Action<ActionForecast> OnForecastUpdated;

        public AbilitySubtype Subtype
        {
            get
            {
                var startNode = Graph.NodeData.FirstOrDefault(n => n.NodeType == "StartNode");
                if (startNode != null) {
                    var data = JsonUtility.FromJson<Celestial_Cross.Scripts.Abilities.Graph.Runtime.StartNodeData>(startNode.JsonData);
                    return data.subtype;
                }
                return AbilitySubtype.None;
            }
        }

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
                Vector2Int? presetTarget = (target.x != -999 && target.y != -999) ? target : (Vector2Int?)null;
                
                // Limpa o target para não influenciar execuções futuras da mesma ação
                target = new Vector2Int(-999, -999);
                AbilityExecutor.Instance.ExecuteGraph(caster, Graph, CombatHook.OnManualCast, () => {
                    // Reset focus
                    CameraController.Instance?.ResetFocus();

                    // Obter subtipo do StartNode do Grafo
                    var startNode = Graph.NodeData.FirstOrDefault(n => n.NodeType == "StartNode");
                    AbilitySubtype subtype = AbilitySubtype.None;
                    if (startNode != null) {
                        var data = JsonUtility.FromJson<Celestial_Cross.Scripts.Abilities.Graph.Runtime.StartNodeData>(startNode.JsonData);
                        subtype = data.subtype;
                    }

                    bool isEnemy = caster is Celestial_Cross.Scripts.Units.Enemy.EnemyUnit;
                    if (!isEnemy && subtype == AbilitySubtype.Movement && !caster.hasMovedThisTurn)
                    {
                        caster.hasMovedThisTurn = true;
                        // Movimento gratuito (0 AP)
                    }
                    else
                    {
                        caster.CurrentAP--;
                    }

                    AbilityExecutor.Instance.StartCoroutine(HandleTurnEnd(caster));
                }, Level, SlotId, presetTarget);
            }
            else
            {
                Debug.LogError("[GraphActionWrapper] AbilityExecutor não encontrado na cena!");
            }
        }

        private System.Collections.IEnumerator HandleTurnEnd(global::Unit caster)
        {
            // Espera até que TODAS as execuções filhas (como passivas disparadas por hooks) terminem
            yield return new WaitUntil(() => !AbilityExecutor.Instance.IsExecuting);

            if (caster.CurrentAP <= 0)
            {
                if (caster is Celestial_Cross.Scripts.Units.Enemy.EnemyUnit)
                    TurnManager.Instance.EndTurn();
                else    
                    PlayerController.Instance.EndTurn();
            }
            else
            {
                PlayerController.Instance?.RefreshUI();
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
