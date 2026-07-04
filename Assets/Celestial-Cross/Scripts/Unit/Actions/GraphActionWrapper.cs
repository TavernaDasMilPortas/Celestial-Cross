using System;
using System.Linq;
using UnityEngine;
using Celestial_Cross.Scripts.Abilities;
using Celestial_Cross.Scripts.Abilities.Graph;
using Celestial_Cross.Scripts.Combat.Execution;

namespace Celestial_Cross.Scripts.Units
{
    public class GraphActionWrapper : IUnitAction
    {
        private readonly global::Unit caster;
        public readonly AbilityGraphSO Graph;
        public int Level { get; set; } = 1;
        public string SlotId { get; set; } = "";
        public Vector2Int Target { get; set; }
        public System.Collections.Generic.List<Vector2Int> PresetTargetPositions { get; set; }
        public event Action<ActionForecast> OnForecastUpdated;

        public string ActionName => string.IsNullOrEmpty(Graph.abilityName) ? Graph.name : Graph.abilityName;
        public Sprite ActionIcon => Graph.abilityIcon;
        public string ActionDescription => Graph.abilityDescription;
        public int Range => Graph.displayRange;
        public AbilitySubtype Subtype => Graph.GetAbilitySubtype();

        public GraphActionWrapper(global::Unit caster, AbilityGraphSO graph, string slotId = "")
        {
            this.caster = caster;
            this.Graph = graph;
            this.SlotId = slotId;
            AbilityExecutor.OnTargetPreviewChanged += HandleTargetPreview;
        }

        private void HandleTargetPreview(AbilityGraphSO runningGraph, System.Collections.Generic.List<global::Unit> targets)
        {
            if (Graph != runningGraph) return;

            if (targets == null || targets.Count == 0)
            {
                OnForecastUpdated?.Invoke(default);
                return;
            }

            global::Unit lastTarget = targets[targets.Count - 1];
            if (lastTarget == null) return;
            
            Debug.Log($"[GraphActionWrapper] Gerando forecast para: {lastTarget.name}");

            // Simulação de dano simplificada (idealmente leria de nós de dano)
            int simulatedBaseDamage = 0;
            
            AttackResult sample = caster.CalculateAttack(lastTarget);

            ActionForecast forecast = new ActionForecast
            {
                Source = caster,
                Target = lastTarget,
                Damage = sample.damage + simulatedBaseDamage,
                IsCritical = sample.isCritical,
                AttackCount = caster.GetAttacksAgainst(lastTarget),
                CriticalChance = caster.Stats.criticalChance
            };

            OnForecastUpdated?.Invoke(forecast);
        }

        public void EnterAction()
        {
            PlayerController.Instance?.ClearGhost();
            PathVisualizer.Instance?.ClearPath();

            if (AbilityExecutor.Instance != null)
            {
                AbilityExecutor.Instance.ExecuteGraph(caster, Graph, CelestialCross.Combat.CombatHook.OnManualCast, () => {
                    CameraController.Instance?.ResetFocus();
                    
                    bool isEnemy = caster is Celestial_Cross.Scripts.Units.Enemy.EnemyUnit;
                    if (!isEnemy && Subtype == AbilitySubtype.Movement)
                    {
                        if (!caster.hasMovedThisTurn)
                        {
                            caster.hasMovedThisTurn = true;
                        }
                        else
                        {
                            caster.CurrentAP--;
                        }
                    }
                    else if (Graph.IsPassive == false)
                    {
                        caster.CurrentAP--;
                    }

                    AbilityExecutor.Instance.StartCoroutine(HandleTurnEnd(caster));
                }, Level, SlotId, null, PresetTargetPositions);
            }
            else
            {
                Debug.LogError("[GraphActionWrapper] AbilityExecutor não encontrado na cena!");
            }
        }

        private System.Collections.IEnumerator HandleTurnEnd(global::Unit caster)
        {
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

        public AreaPatternData GetAreaPattern()
        {
            if (Graph == null || Graph.NodeData == null) return null;
            var targetNode = Graph.NodeData.FirstOrDefault(n => n.NodeType == "TargetNode");
            if (targetNode != null)
            {
                var targetData = JsonUtility.FromJson<Celestial_Cross.Scripts.Abilities.Graph.Runtime.TargetNodeData>(targetNode.JsonData);
                return Graph.GetAsset<AreaPatternData>(targetData.patternReferenceId);
            }
            return null;
        }

        public int GetMaxTargets()
        {
            if (Graph == null || Graph.NodeData == null) return 1;
            var targetNode = Graph.NodeData.FirstOrDefault(n => n.NodeType == "TargetNode");
            if (targetNode != null)
            {
                var targetData = JsonUtility.FromJson<Celestial_Cross.Scripts.Abilities.Graph.Runtime.TargetNodeData>(targetNode.JsonData);
                return targetData.maxTargets;
            }
            return 1;
        }

        public bool GetAllowSameTargetMultipleTimes()
        {
            if (Graph == null || Graph.NodeData == null) return false;
            var targetNode = Graph.NodeData.FirstOrDefault(n => n.NodeType == "TargetNode");
            if (targetNode != null)
            {
                return JsonUtility.FromJson<Celestial_Cross.Scripts.Abilities.Graph.Runtime.TargetNodeData>(targetNode.JsonData).allowSameTargetMultipleTimes;
            }
            return false;
        }

        public void Cancel() => AbilityExecutor.Instance?.AbortCurrentAbility();
        public void UpdateAction() { }
        public void Confirm() { }
        public string GetDetailStats() => $"Range: {Range}";
    }
}
