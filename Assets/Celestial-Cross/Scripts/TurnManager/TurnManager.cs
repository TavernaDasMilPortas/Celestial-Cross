using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public static event System.Action<Unit> OnTurnStarted;
    public static event System.Action<IEnumerable<Unit>> OnQueueChanged;

    Queue<Unit> turnQueue = new();

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

        Debug.Log("[TurnManager] Combate iniciado.");
        OnQueueChanged?.Invoke(turnQueue);
        NextTurn();
    }

    void NextTurn()
    {
        if (turnQueue.Count == 0)
            return;

        Unit current = turnQueue.Dequeue();
        turnQueue.Enqueue(current);

        Debug.Log($"[TurnManager] Turno de {current.DisplayName}");
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
        Invoke(nameof(NextTurn), 0.5f);
    }

    public IEnumerable<Unit> GetTurnQueue()
    {
        return turnQueue;
    }
}
