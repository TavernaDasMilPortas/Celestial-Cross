using System;
using System.Linq;
using System.Collections.Generic;
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
        public List<Vector2Int> PresetTargetPositions { get; set; }
        public string SlotId { get; set; } = "";

        public event Action<ActionForecast> OnForecastUpdated;

        public AbilitySubtype Subtype
        {
            get
            {
                var startNode = Graph.NodeData.FirstOrDefault(n => n.NodeType == "StartNode");
                if (startNode != null && !string.IsNullOrEmpty(startNode.JsonData)) {
                    var data = JsonUtility.FromJson<Celestial_Cross.Scripts.Abilities.Graph.Runtime.StartNodeData>(startNode.JsonData);
                    if (data != null) {
                        return data.subtype;
                    }
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

        public int GetMaxTargets()
        {
            foreach (var node in Graph.NodeData)
            {
                if (node.NodeType == "TargetNode")
                {
                    var targetData = JsonUtility.FromJson<Celestial_Cross.Scripts.Abilities.Graph.Runtime.TargetNodeData>(node.JsonData);
                    if (targetData != null && targetData.multipleTargets) return targetData.maxTargets;
                }
            }
            return 1;
        }

        public bool GetAllowSameTargetMultipleTimes()
        {
            foreach (var node in Graph.NodeData)
            {
                if (node.NodeType == "TargetNode")
                {
                    var targetData = JsonUtility.FromJson<Celestial_Cross.Scripts.Abilities.Graph.Runtime.TargetNodeData>(node.JsonData);
                    if (targetData != null && targetData.multipleTargets) return targetData.allowSameTargetMultipleTimes;
                }
            }
            return false;
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

                    var startNode = Graph.NodeData.FirstOrDefault(n => n.NodeType == "StartNode");
                    AbilitySubtype subtype = AbilitySubtype.None;
                    if (startNode != null && !string.IsNullOrEmpty(startNode.JsonData)) {
                        var data = JsonUtility.FromJson<Celestial_Cross.Scripts.Abilities.Graph.Runtime.StartNodeData>(startNode.JsonData);
                        if (data != null) {
                            subtype = data.subtype;
                        }
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
                }, Level, SlotId, presetTarget, presetTargetPositions: PresetTargetPositions);
                
                PresetTargetPositions = null;
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

        private GridTile lastHoveredTile;
        private UnitGhostPreview ghostPreview;
        private List<Vector2Int> currentPath;

        public void UpdateAction()
        {
            if (Graph == null) return;

            // Tentamos descobrir qual o efeito principal de movimento desse grafo
            var previewData = Graph.GetPreviewMoveData();
            if (previewData == null) return; // Nao é movimento, não desenha caminho

            if (GridMap.Instance == null) return;

            Vector2Int mousePos = GridMap.Instance.GetMouseGridPosition();
            if (mousePos.x == -1 && mousePos.y == -1) return;

            GridTile hoveredTile = GridMap.Instance.GetTile(mousePos);

            if (hoveredTile != lastHoveredTile)
            {
                lastHoveredTile = hoveredTile;
                
                // Se o tile não for válido na whitelist do TargetSelector, ignorar
                var targetSelector = caster.GetComponent<TargetSelector>();
                if (targetSelector == null || !targetSelector.ValidTiles.Contains(hoveredTile))
                {
                    ClearPreviews();
                    return;
                }

                if (previewData.moveMode == Celestial_Cross.Scripts.Abilities.Graph.Runtime.MoveEffectNodeData.MoveMode.MoveCaster)
                {
                    // Movimento do próprio caster (Andar/Teleport)
                    currentPath = GridMap.Instance.FindPath(caster.GridPosition, hoveredTile.GridPosition, targetSelector.ValidTiles);
                    
                    if (currentPath != null && currentPath.Count > 0)
                    {
                        if (previewData.moveType != Celestial_Cross.Scripts.Abilities.Graph.Runtime.MoveEffectNodeData.MoveType.TeleportToTarget)
                            if (PathVisualizer.Instance != null) PathVisualizer.Instance.DrawPath(currentPath, caster.GridPosition);
                        
                        caster.lastCalculatedPath = currentPath; // <-- GUARDA O PATH EXATO AQUI
                        ShowGhost(caster, GridMap.Instance.GridToWorld(hoveredTile.GridPosition), caster.GridPosition.x > hoveredTile.GridPosition.x);
                    }
                    else
                    {
                        ClearPreviews();
                    }
                }
                else
                {
                    // Movimento do Alvo (Push / Pull)
                    if (hoveredTile.OccupyingUnit != null)
                    {
                        Unit targetUnit = hoveredTile.OccupyingUnit;
                        Vector2Int dir = (targetUnit.GridPosition - caster.GridPosition);
                        
                        // Normaliza a direção
                        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y)) dir = new Vector2Int(Mathf.Sign(dir.x) > 0 ? 1 : -1, 0);
                        else dir = new Vector2Int(0, Mathf.Sign(dir.y) > 0 ? 1 : -1);

                        if (previewData.moveType == Celestial_Cross.Scripts.Abilities.Graph.Runtime.MoveEffectNodeData.MoveType.Pull)
                            dir = -dir;

                        Vector2Int pushDest = targetUnit.GridPosition + (dir * previewData.range);
                        currentPath = GridMap.Instance.FindPath(targetUnit.GridPosition, pushDest, null);

                        if (currentPath != null && currentPath.Count > 0)
                        {
                            if (PathVisualizer.Instance != null) PathVisualizer.Instance.DrawPath(currentPath, targetUnit.GridPosition);
                            ShowGhost(targetUnit, GridMap.Instance.GridToWorld(pushDest), targetUnit.GridPosition.x > pushDest.x);
                        }
                    }
                    else
                    {
                        ClearPreviews();
                    }
                }
            }
        }

        private void ShowGhost(Unit targetUnit, Vector3 worldPos, bool flip)
        {
            if (ghostPreview == null)
            {
                ghostPreview = caster.gameObject.GetComponent<UnitGhostPreview>();
                if (ghostPreview == null) ghostPreview = caster.gameObject.AddComponent<UnitGhostPreview>();
            }
            ghostPreview.Initialize(targetUnit);
            ghostPreview.ShowAt(worldPos, flip);
        }

        private void ClearPreviews()
        {
            if (PathVisualizer.Instance != null) PathVisualizer.Instance.ClearPath();
            if (ghostPreview != null) ghostPreview.Hide();
            currentPath = null;
        }

        public void Confirm()
        {
            ClearPreviews();
        }

        public void Cancel()
        {
            ClearPreviews();
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
