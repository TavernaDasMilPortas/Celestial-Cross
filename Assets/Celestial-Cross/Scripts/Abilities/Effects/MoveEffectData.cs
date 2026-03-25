using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Celestial_Cross.Scripts.Abilities;
using Celestial_Cross.Scripts.Combat.Execution;
using CelestialCross.Combat;

namespace Celestial_Cross.Scripts.Abilities.Effects
{
    [Serializable]
    public class MoveEffectData : EffectData
    {
        [Tooltip("If true, the unit to be moved is the target of the step. If false, it moves the source (caster).")]
        public bool moveTarget = false;

        [Tooltip("Maximum distance of the movement.")]
        public int range = 3;

        [Tooltip("If true, requires the player to click on a destination tile during effect execution.")]
        public bool manualDestination = true;

        [Tooltip("When selecting destination, can we choose a tile that is already occupied?")]
        public bool allowOccupiedTiles = false;

        public override void Execute(CombatContext context)
        {
            // O código síncrono agora não faz nada perigoso aqui
        }

        public override IEnumerator ExecuteCoroutine(CombatContext context)
        {
            Unit unitToMove = moveTarget ? context.target : context.source;
            if (unitToMove == null) yield break;

            Vector2Int destination = unitToMove.GridPosition;

            if (manualDestination)
            {
                // Criar um TargetSelector temporário para escolher o destino
                TargetSelector selector = unitToMove.gameObject.AddComponent<TargetSelector>();
                
                // Configuração para selecionar 1 tile vazio no range
                TargetingRuleData moveRule = new TargetingRuleData();
                moveRule.mode = TargetingMode.Area; // Permite selecionar tiles
                moveRule.origin = TargetOrigin.Point;
                moveRule.maxTargets = 1;
                moveRule.canTargetSelf = false;

                // Restringir a tiles válidos (não ocupados se allowOccupiedTiles for false)
                var gridMap = GridMap.Instance;
                List<GridTile> whitelist = new List<GridTile>();
                if (gridMap != null)
                {
                    foreach (var tile in gridMap.GetAllTiles())
                    {
                        if (tile == null) continue;
                        
                        // Verifica se o tile está dentro do range usando Manhattan Distance
                        int dist = Math.Abs(tile.GridPosition.x - unitToMove.GridPosition.x) + Math.Abs(tile.GridPosition.y - unitToMove.GridPosition.y);
                        if (dist > range) continue;

                        if (!allowOccupiedTiles && tile.IsOccupied && tile.GridPosition != unitToMove.GridPosition)
                            continue;
                            
                        whitelist.Add(tile);
                    }
                }

                bool confirmed = false;
                List<Unit> selected = new List<Unit>();

                selector.OnTargetsConfirmed += (targets) => { 
                    selected = targets; 
                    confirmed = true; 
                };
                
                selector.Begin(unitToMove, range, moveRule, null, 0, whitelist);

                // Feedback visual: escurecer tiles fora do range ou ocupados
                if (gridMap != null)
                {
                    foreach (var tile in gridMap.GetAllTiles())
                    {
                        if (!whitelist.Contains(tile)) tile.Darken();
                        else tile.ClearDarken(); // Garante que os válidos NÃO estão escuros
                    }
                }

                // IMPORTANTE: O loop de execução da habilidade agora ESPERA este clique
                yield return new WaitUntil(() => confirmed);

                if (selector.SelectedPoints.Count > 0)
                {
                    destination = selector.SelectedPoints[0];
                }
                
                if (gridMap != null) gridMap.ResetAllTileVisuals();
                UnityEngine.Object.Destroy(selector);
            }
            else
            {
                destination = context.target.GridPosition;
            }

            if (destination != unitToMove.GridPosition)
            {
                GridMap gridMap = GridMap.Instance;
                if (gridMap != null)
                {
                    var oldTile = gridMap.GetTile(unitToMove.GridPosition);
                    if (oldTile != null) { oldTile.IsOccupied = false; oldTile.OccupyingUnit = null; }

                    unitToMove.GridPosition = destination;
                    unitToMove.transform.position = new Vector3(destination.x, unitToMove.transform.position.y, destination.y);

                    var newTile = gridMap.GetTile(destination);
                    if (newTile != null) { newTile.IsOccupied = true; newTile.OccupyingUnit = unitToMove; }
                }
            }

            Debug.Log($"[MoveEffect] {unitToMove.name} movido para {destination}");
        }
    }
}