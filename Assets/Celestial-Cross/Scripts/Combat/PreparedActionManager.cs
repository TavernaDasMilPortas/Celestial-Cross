using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CelestialCross.Combat;
using Celestial_Cross.Scripts.Abilities.Graph;
using Celestial_Cross.Scripts.Abilities.Graph.Runtime;

public class PreparedActionManager : MonoBehaviour
{
    public static PreparedActionManager Instance { get; private set; }

    public class PreparedAction
    {
        public Unit Caster;
        public AbilityGraphSO Graph;
        public CombatContext ContextSnapshot;
        public AbilityNodeData NextNode;
        public int TurnsRemaining;
        public int MaxTurns;
    }

    private List<PreparedAction> scheduledActions = new List<PreparedAction>();

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        TurnManager.OnTurnStarted += OnTurnStarted;
        if (PhaseManager.Instance != null)
            PhaseManager.Instance.OnPhaseEnded += HandlePhaseEnded;
    }

    void OnDisable()
    {
        TurnManager.OnTurnStarted -= OnTurnStarted;
        if (PhaseManager.Instance != null)
            PhaseManager.Instance.OnPhaseEnded -= HandlePhaseEnded;
    }

    private void HandlePhaseEnded(Team team)
    {
        scheduledActions.Clear();
        GridMap.Instance?.ClearAllTelegraphs();
    }

    public void ScheduleAction(Unit caster, AbilityGraphSO graph, CombatContext context, AbilityNodeData nextNode, int delayTurns)
    {
        var action = new PreparedAction
        {
            Caster = caster,
            Graph = graph,
            ContextSnapshot = context.Clone(),
            NextNode = nextNode,
            TurnsRemaining = delayTurns,
            MaxTurns = delayTurns
        };

        scheduledActions.Add(action);
        UpdateTelegraphVisuals();
        CombatLogger.Log($"<color=#ffaa00>[Telegraph]</color> <b>{caster.DisplayName}</b> agendou um ataque para daqui {delayTurns} turnos.", LogCategory.System);
    }

    private void OnTurnStarted(Unit currentUnit)
    {
        for (int i = scheduledActions.Count - 1; i >= 0; i--)
        {
            var action = scheduledActions[i];
            if (action.Caster == currentUnit)
            {
                action.TurnsRemaining--;
                if (action.TurnsRemaining <= 0)
                {
                    scheduledActions.RemoveAt(i);
                    StartCoroutine(ExecutePreparedAction(action));
                }
            }
        }
        UpdateTelegraphVisuals();
    }

    private IEnumerator ExecutePreparedAction(PreparedAction action)
    {
        CombatLogger.Log($"<color=#ff0000>[Telegraph]</color> Ação preparada de <b>{action.Caster.DisplayName}</b> caiu!", LogCategory.System);
        yield return StartCoroutine(AbilityGraphInterpreter.Instance.ExecuteFromNode(action.Caster, action.Graph, action.NextNode, action.ContextSnapshot));
    }

    public void UpdateTelegraphVisuals()
    {
        if (GridMap.Instance == null) return;

        GridMap.Instance.ClearAllTelegraphs();

        foreach (var action in scheduledActions)
        {
            foreach (var targetUnit in action.ContextSnapshot.targets)
            {
                if (targetUnit != null)
                {
                    var tile = GridMap.Instance.GetTile(targetUnit.GridPosition);
                    if (tile != null)
                    {
                        tile.ApplyTelegraph(action.TurnsRemaining);
                    }
                }
            }
        }
    }
}
