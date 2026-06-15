using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CelestialCross.Combat;
using Celestial_Cross.Scripts.Combat.Execution;
using Celestial_Cross.Scripts.Units;
using Celestial_Cross.Scripts.Units.Enemy;
using Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree;
using Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime;

public class AIBrain : MonoBehaviour
{
    private EnemyUnit enemy;
    private GridMap gridMap;
    private AIBlackboard blackboard;
    private BehaviorTreeRunner runner;

    public Unit MyUnit { get; private set; }

    void Awake()
    {
        enemy = GetComponent<EnemyUnit>();
        MyUnit = enemy;
        blackboard = new AIBlackboard();
        runner = new BehaviorTreeRunner();
    }

    void Start()
    {
        gridMap = GridMap.Instance;
        
        if (enemy.BehaviorTree != null)
        {
            runner.Initialize(enemy.BehaviorTree);
        }
    }

    public void ReinitializeTree()
    {
        if (enemy != null && enemy.BehaviorTree != null && runner != null)
        {
            runner.Initialize(enemy.BehaviorTree);
        }
    }

    void OnEnable()
    {
        TurnManager.OnTurnEnded += HandleTurnEnded;
    }

    void OnDisable()
    {
        TurnManager.OnTurnEnded -= HandleTurnEnded;
    }

    void HandleTurnEnded()
    {
        if (TurnManager.Instance.CurrentUnit == MyUnit)
        {
            blackboard.turnsAlive++;
            blackboard.currentTurnNumber++;
            
            var keys = new List<string>(blackboard.abilityCooldowns.Keys);
            foreach (var key in keys)
            {
                if (blackboard.abilityCooldowns[key] > 0)
                {
                    blackboard.abilityCooldowns[key]--;
                }
            }
        }
    }

    public void ExecuteTurn()
    {
        CelestialCross.Combat.CombatLogger.Log($"Iniciando turno de IA de <b>{enemy.DisplayName}</b>", CelestialCross.Combat.LogCategory.AI);

        if (enemy.BehaviorTree == null)
        {
            TurnManager.Instance.EndTurn();
            return;
        }

        StartCoroutine(AILoopRoutine());
    }

    private IEnumerator AILoopRoutine()
    {
        // Limite de segurança para não travar num loop infinito se algo der errado
        int maxIterations = 5; 
        int iterations = 0;

        while (enemy.CurrentAP > 0 && iterations < maxIterations)
        {
            iterations++;
            UpdateBlackboard();

            var result = runner.Evaluate(blackboard);

            if (result == BTResult.Success && blackboard.bestPlan != null)
            {
                yield return StartCoroutine(ExecutePlanRoutine(blackboard.bestPlan));
                
                // O AP já é consumido pelas Actions (GraphActionWrapper, etc.)
                // Não subtrair aqui para evitar débito duplo!
                
                // Pequeno delay entre ações
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                CelestialCross.Combat.CombatLogger.Log($"<b>{enemy.DisplayName}</b>: Nenhuma ação viável. Encerrando turno com {enemy.CurrentAP} AP sobrando.", CelestialCross.Combat.LogCategory.AI);
                break;
            }
        }

        // Só chama EndTurn se a unidade ainda for a ativa.
        // Se a AP chegou a 0, as Actions (HandleTurnEnd) já podem ter chamado o EndTurn.
        if (TurnManager.Instance.CurrentUnit == enemy)
        {
            TurnManager.Instance.EndTurn();
        }
    }

    private void UpdateBlackboard()
    {
        blackboard.myPosition = enemy.GridPosition;
        blackboard.MyUnit = enemy;
        blackboard.myBaseRange = enemy.UnitData != null ? 1 : 1; 

        blackboard.allies = new List<Unit>();
        blackboard.enemies = new List<Unit>();

        Unit closestTarget = null;
        float minTargetDist = float.MaxValue;
        
        foreach (var unit in FindObjectsOfType<Unit>().Where(u => u.Health.CurrentHealth > 0))
        {
            if (unit == enemy) continue;

            if (unit.Team == enemy.Team)
            {
                blackboard.allies.Add(unit);
            }
            else
            {
                blackboard.enemies.Add(unit);
                float dist = AIGridUtility.ChebyshevDistance(enemy.GridPosition, unit.GridPosition);
                if (dist < minTargetDist)
                {
                    minTargetDist = dist;
                    closestTarget = unit;
                }
            }
        }

        blackboard.closestEnemy = closestTarget;
        blackboard.aliveAllyCount = blackboard.allies.Count;
        blackboard.isAlone = blackboard.aliveAllyCount == 0;
        blackboard.myHpPercent = (float)enemy.Health.CurrentHealth / enemy.MaxHealth * 100f;

        var moveAction = enemy.Actions.FirstOrDefault(a => a is GraphActionWrapper gw && gw.Subtype == AbilitySubtype.Movement) 
                      ?? enemy.Actions.FirstOrDefault(a => a.GetType().Name == "MoveAction");
            
        int moveRange = moveAction != null ? moveAction.Range : 3;
        blackboard.reachableTiles = AIGridUtility.GetReachableTiles(enemy.GridPosition, moveRange, gridMap);

        blackboard.availableAbilities.Clear();
        foreach (var act in enemy.Actions)
        {
            if (act is GraphActionWrapper gw && gw.Graph != null)
            {
                var hint = gw.Graph.aiHint;
                
                // Filtro de Cooldown
                if (blackboard.abilityCooldowns.TryGetValue(act.ActionName, out int cd) && cd > 0)
                {
                    continue;
                }

                blackboard.availableAbilities.Add(new AIBlackboard.AbilityInfo { 
                    action = act, 
                    hint = hint,
                    subtype = gw.Subtype,
                    range = gw.Range,
                    areaPattern = gw.GetAreaPattern(),
                    maxTargets = gw.GetMaxTargets(),
                    allowSameTargetMultipleTimes = gw.GetAllowSameTargetMultipleTimes()
                });
            }
            else if (act is MoveAction)
            {
                blackboard.availableAbilities.Add(new AIBlackboard.AbilityInfo { 
                    action = act, 
                    hint = null,
                    subtype = AbilitySubtype.Movement,
                    range = act.Range,
                    areaPattern = null
                });
            }
            else if (act is AttackAction || act.ActionName == "Ataque Básico")
            {
                var h = new Celestial_Cross.Scripts.Units.Enemy.AI.AIAbilityHint { category = Celestial_Cross.Scripts.Units.Enemy.AI.AIAbilityHint.AbilityCategory.Damage, basePriority = 10 };
                blackboard.availableAbilities.Add(new AIBlackboard.AbilityInfo { 
                    action = act, 
                    hint = h,
                    subtype = AbilitySubtype.Attack,
                    range = act.Range,
                    areaPattern = null,
                    maxTargets = 1,
                    allowSameTargetMultipleTimes = false
                });
            }
        }

        // Extrai dinamicamente o maior range das habilidades ofensivas para uso em verificações de range da BT
        var offensiveAbil = blackboard.availableAbilities.Where(a => a.subtype == AbilitySubtype.Attack || a.subtype == AbilitySubtype.Debuff).ToList();
        if (offensiveAbil.Count > 0)
        {
            blackboard.myBaseRange = offensiveAbil.Max(a => a.range);
        }
        else
        {
            blackboard.myBaseRange = 1;
        }
    }

    private IEnumerator ExecutePlanRoutine(AIBlackboard.PlannedAction plan)
    {
        if (plan.actionToExecute != null)
        {
            int actIdx = enemy.Actions.Select((a, i) => new {a, i}).FirstOrDefault(x => x.a == plan.actionToExecute)?.i ?? -1;
            if (actIdx >= 0)
            {
                Vector2Int targetPos = enemy.GridPosition;
                List<Vector2Int> targetPositions = new List<Vector2Int>();
                
                if (plan.targetPositions != null && plan.targetPositions.Count > 0)
                {
                    targetPositions = plan.targetPositions;
                    targetPos = targetPositions[0];
                }
                else if (plan.targetUnit != null)
                {
                    targetPos = plan.targetUnit.GridPosition;
                    targetPositions.Add(targetPos);
                }
                else if (plan.moveTarget.HasValue)
                {
                    targetPos = plan.moveTarget.Value;
                    targetPositions.Add(targetPos);
                }
                
                // Registro do Cooldown (Fase 1)
                var abInfo = blackboard.availableAbilities.FirstOrDefault(a => a.action == plan.actionToExecute);
                if (abInfo != null && abInfo.hint != null && abInfo.hint.cooldownTurns > 0)
                {
                    blackboard.abilityCooldowns[plan.actionToExecute.ActionName] = abInfo.hint.cooldownTurns;
                }
                
                // --- VISUAL FEEDBACK (Fase 1) ---
                if (GridMap.Instance != null)
                {
                    int range = enemy.Actions[actIdx].Range;
                    
                    // Passo 1: Foca no inimigo (caster) por um tempo x
                    if (CameraController.Instance != null)
                    {
                        CameraController.Instance.Follow(enemy);
                    }
                    yield return new WaitForSeconds(0.8f);
                    
                    // Passo 2: Abre o range da ação. Se o range for grande (>= 4), vai para o zoom máximo (afastado)
                    HashSet<Vector2Int> areaToHighlight;
                    if (plan.moveTarget.HasValue)
                    {
                        areaToHighlight = blackboard.reachableTiles;
                    }
                    else
                    {
                        // Habilidades usam distância radial (Chebyshev) e ignoram obstáculos de movimento
                        areaToHighlight = new HashSet<Vector2Int>();
                        foreach (var tile in GridMap.Instance.GetAllTiles())
                        {
                            if (AIGridUtility.ChebyshevDistance(enemy.GridPosition, tile.GridPosition) <= range)
                            {
                                if (tile.GridPosition != enemy.GridPosition)
                                    areaToHighlight.Add(tile.GridPosition);
                            }
                        }
                    }
                    
                    if (areaToHighlight != null)
                    {
                        foreach (var pos in areaToHighlight)
                        {
                            GridTile tile = GridMap.Instance.GetTile(pos);
                            if (tile != null) tile.Highlight();
                        }
                    }
                    GridMap.Instance.RefreshDynamicHighlights();

                    float originalZoom = CameraController.Instance != null ? CameraController.Instance.TargetZoom : 5f;
                    if (CameraController.Instance != null)
                    {
                        if (range >= 4)
                        {
                            CameraController.Instance.TargetZoom = CameraController.Instance.maxZoom;
                        }
                    }
                    yield return new WaitForSeconds(0.8f);
                    
                    // Passo 3: Foca por um tempo onde está o alvo voltando para o zoom que estava antes
                    foreach (var pos in targetPositions)
                    {
                        GridTile targetTile = GridMap.Instance.GetTile(pos);
                        if (targetTile != null)
                        {
                            if (plan.targetUnit != null || (plan.targetPositions != null && plan.targetPositions.Count > 0))
                                targetTile.PreviewArea(); // Destaca vermelho/agressivo
                            else
                                targetTile.Select(); // Destaca selecionado/movimento
                        }
                    }
                    GridMap.Instance.RefreshDynamicHighlights();

                    if (CameraController.Instance != null)
                    {
                        CameraController.Instance.TargetProjectedPoint = GridMap.Instance.GridToWorld(targetPos);
                        CameraController.Instance.cameraMode = CameraController.CameraMode.Free;
                        CameraController.Instance.TargetZoom = originalZoom;
                    }
                    yield return new WaitForSeconds(0.8f);
                    
                    // Passo 4: Limpa visualizações antes de executar a ação
                    GridMap.Instance.ResetAllTileVisuals();
                    GridMap.Instance.RefreshDynamicHighlights();
                }
                // ---------------------------------
                
                // Configura o alvo ANTES do SelectAction
                enemy.Actions[actIdx].Target = targetPos;
                if (enemy.Actions[actIdx] is GraphActionWrapper wrapper)
                {
                    wrapper.PresetTargetPositions = targetPositions;
                }
                
                // Dispara a execução da habilidade (GraphActionWrapper executa no EnterAction)
                enemy.SelectAction(actIdx);
                
                // Wait for the action to actually start
                yield return new WaitForSeconds(0.1f);
                
                // Wait until the ability executor finishes its coroutine
                while (AbilityExecutor.Instance != null && AbilityExecutor.Instance.IsExecuting)
                {
                    yield return null;
                }
                
                // Small buffer after execution
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}
