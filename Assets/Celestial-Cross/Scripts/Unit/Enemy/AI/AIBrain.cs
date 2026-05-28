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
        Debug.Log($"[AIBrain] Iniciando turno de {enemy.DisplayName}");

        if (enemy.BehaviorTree == null)
        {
            TurnManager.Instance.EndTurn();
            return;
        }

        UpdateBlackboard();

        var result = runner.Evaluate(blackboard);

        if (result == BTResult.Success && blackboard.bestPlan != null)
        {
            StartCoroutine(ExecutePlanRoutine(blackboard.bestPlan));
        }
        else
        {
            Debug.Log("[AIBrain] Nenhuma ação viável. Passando turno.");
            TurnManager.Instance.EndTurn();
        }
    }

    private void UpdateBlackboard()
    {
        blackboard.myPosition = enemy.GridPosition;
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

        var moveAction = enemy.Actions.FirstOrDefault(a => a is MoveAction) as MoveAction;
        int moveRange = moveAction != null ? moveAction.Range : 3;
        blackboard.reachableTiles = AIGridUtility.GetReachableTiles(enemy.GridPosition, moveRange, gridMap);

        blackboard.availableAbilities.Clear();
        foreach (var act in enemy.Actions)
        {
            if (act is GraphActionWrapper gw && gw.Graph != null)
            {
                var hint = gw.Graph.aiHint;
                blackboard.availableAbilities.Add(new AIBlackboard.AbilityInfo { action = act, hint = hint });
            }
            else if (act.ActionName == "Ataque Básico")
            {
                var h = new Celestial_Cross.Scripts.Units.Enemy.AI.AIAbilityHint { category = Celestial_Cross.Scripts.Units.Enemy.AI.AIAbilityHint.AbilityCategory.Damage, basePriority = 10 };
                blackboard.availableAbilities.Add(new AIBlackboard.AbilityInfo { action = act, hint = h });
            }
        }
    }

    private IEnumerator ExecutePlanRoutine(AIBlackboard.PlannedAction plan)
    {
        if (plan.moveTarget.HasValue && plan.moveTarget.Value != enemy.GridPosition)
        {
            var moveIndex = enemy.Actions.Select((a, i) => new {a, i}).FirstOrDefault(x => x.a is MoveAction)?.i ?? -1;
            if (moveIndex >= 0)
            {
                enemy.SelectAction(moveIndex);
                var act = enemy.Actions[moveIndex] as MoveAction;
                if (act != null) act.Target = plan.moveTarget.Value;
                
                enemy.ConfirmAction();
                yield return new WaitForSeconds(0.6f);
            }
        }

        if (plan.actionToExecute != null)
        {
            int actIdx = enemy.Actions.Select((a, i) => new {a, i}).FirstOrDefault(x => x.a == plan.actionToExecute)?.i ?? -1;
            if (actIdx >= 0)
            {
                enemy.SelectAction(actIdx);
                
                if (plan.targetUnit != null)
                {
                    enemy.Actions[actIdx].Target = plan.targetUnit.GridPosition;
                }
                
                enemy.ConfirmAction();
                yield return new WaitForSeconds(0.6f);
            }
        }

        TurnManager.Instance.EndTurn();
    }
}
