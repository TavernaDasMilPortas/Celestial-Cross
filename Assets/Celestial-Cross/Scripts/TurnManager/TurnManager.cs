using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CelestialCross.Combat;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;
    public Unit CurrentUnit { get; private set; }

    public static event System.Action<Unit> OnTurnStarted;
    public static event System.Action OnTurnEnded;
    public static event System.Action<int> OnRoundStarted;
    public static event System.Action<IEnumerable<Unit>> OnQueueChanged;

    public int RoundCounter { get; private set; } = 1;
    private Unit roundStartUnit;

    Queue<Unit> turnQueue = new();
    bool combatStarted;

    void Awake()
    {
        Instance = this;
    }

    // =============================
    // COMBAT FLOW
    // =============================

    public void StartCombat(List<Unit> units)
    {
        if (units == null || units.Count == 0)
        {
            Debug.LogWarning("[TurnManager] Nenhuma unit para iniciar combate.");
            return;
        }

        var ordered = units
            .OrderByDescending(u => u.Speed)
            .ToList();

        turnQueue = new Queue<Unit>(ordered);
        roundStartUnit = ordered.FirstOrDefault();
        RoundCounter = 1;

        Debug.Log($"[TurnManager] Combate iniciado. Rodada {RoundCounter}");
        OnQueueChanged?.Invoke(turnQueue);
        OnRoundStarted?.Invoke(RoundCounter);
        NextTurn();
    }

    void NextTurn()
    {
        if (turnQueue.Count == 0)
            return;

        Unit current = turnQueue.Dequeue();
        turnQueue.Enqueue(current);
        CurrentUnit = current;

        Debug.Log($"[TurnManager] Turno de {current.DisplayName}");

        if (current == roundStartUnit && combatStarted)
        {
            RoundCounter++;
            Debug.Log($"[TurnManager] Nova Rodada: {RoundCounter}");
            OnRoundStarted?.Invoke(RoundCounter);
        }

        combatStarted = true;
        OnQueueChanged?.Invoke(turnQueue);
        OnTurnStarted?.Invoke(current);

        if (current is EnemyUnit enemy)
        {
            AIBrain brain = enemy.GetComponent<AIBrain>();
            if (brain != null)
                brain.ExecuteTurn();
            else
                EndTurn();
        }
        else if (current is Pet)
        {
            PlayerController.Instance.StartTurn(current);
        }
        else
        {
            EndTurn();
        }
    }

    public void EndTurn()
    {
        OnTurnEnded?.Invoke();
        Invoke(nameof(NextTurn), 0.5f);
    }

    public IEnumerable<Unit> GetTurnQueue()
    {
        return turnQueue;
    }
}
