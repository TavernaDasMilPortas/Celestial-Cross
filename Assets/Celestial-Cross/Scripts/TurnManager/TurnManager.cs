using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Celestial_Cross.Scripts.Units.Enemy;
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
    public Unit RoundStartUnit => roundStartUnit;

    private List<Unit> turnQueue = new();
    private List<Unit> actedUnits = new();
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

        turnQueue = units.OrderByDescending(u => u.Speed).ToList();
        actedUnits.Clear();
        roundStartUnit = turnQueue.FirstOrDefault();
        RoundCounter = 1;
        combatStarted = false;

        Debug.Log($"[TurnManager] Combate iniciado. Rodada {RoundCounter}");
        OnQueueChanged?.Invoke(turnQueue);
        OnRoundStarted?.Invoke(RoundCounter);
        NextTurn();
    }

    private void RefreshQueueOrder()
    {
        // Re-sortear apenas quem ainda não agiu nesta rodada
        turnQueue = turnQueue.OrderByDescending(u => u.Speed).ToList();
        OnQueueChanged?.Invoke(turnQueue.Concat(actedUnits));
    }

    void NextTurn()
    {
        // 1. Limpar unidades mortas ou inativas de ambas as listas
        turnQueue.RemoveAll(u => u == null || !u.gameObject.activeInHierarchy);
        actedUnits.RemoveAll(u => u == null || !u.gameObject.activeInHierarchy);

        // 2. Se a fila de quem ainda não agiu está vazia, iniciamos uma nova rodada
        if (turnQueue.Count == 0)
        {
            if (actedUnits.Count == 0) 
            {
                Debug.LogWarning("[TurnManager] Nenhuma unidade ativa para continuar o combate.");
                return;
            }

            // Nova Rodada: todos que agiram voltam para a fila de espera
            turnQueue = actedUnits.OrderByDescending(u => u.Speed).ToList();
            actedUnits.Clear();
            
            RoundCounter++;
            roundStartUnit = turnQueue.FirstOrDefault();
            
            CombatLogger.Log($"<color=#ffd700>[Sistema]</color> Iniciando Rodada <b>{RoundCounter}</b>", LogCategory.System);
            OnRoundStarted?.Invoke(RoundCounter);
        }

        // 3. Re-ordenar quem falta agir para garantir que buffs de velocidade recentes sejam aplicados
        RefreshQueueOrder();

        if (turnQueue.Count == 0) return;

        // 4. Pega a próxima unidade
        Unit current = turnQueue[0];
        turnQueue.RemoveAt(0);
        actedUnits.Add(current);
        
        CurrentUnit = current;
        
        current.GetComponentInChildren<UnitVisualController>()?.SetCombatState(true);
        current.hasMovedThisTurn = false;
        current.hasActedThisTurn = false;

        CombatLogger.CurrentUnit = current;
        CombatLogger.Log($"<color=#00ffff>[Turno]</color> Início do turno de <b>{current.DisplayName}</b>", LogCategory.Ability);

        combatStarted = true;
        OnQueueChanged?.Invoke(turnQueue.Concat(actedUnits));
        OnTurnStarted?.Invoke(current);

        // Lógica de execução (Tutorial, AI ou Player)
        ProcessUnitTurn(current);
    }

    private void ProcessUnitTurn(Unit current)
    {
        if (CelestialCross.Tutorial.TutorialManager.Instance != null && CelestialCross.Tutorial.TutorialManager.Instance.IsActive)
        {
            CelestialCross.Tutorial.TutorialManager.Instance.NotifyTurnEnded();
            if (current is Pet) PlayerController.Instance.StartTurn(current);
            return; 
        }

        if (current is EnemyUnit enemy)
        {
            AIBrain brain = enemy.GetComponent<AIBrain>();
            if (brain != null) brain.ExecuteTurn();
            else EndTurn();
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
        StartCoroutine(CoEndTurn());
    }

    private IEnumerator CoEndTurn()
    {
        if (CurrentUnit != null)
            CurrentUnit.GetComponentInChildren<UnitVisualController>()?.SetCombatState(false);
            
        OnTurnEnded?.Invoke();

        // Espera todos os popups de dano sumirem antes de seguir para o próximo turno
        if (DamagePopupManager.Instance != null)
        {
            yield return new WaitUntil(() => !DamagePopupManager.Instance.HasActivePopups);
        }
        
        // Pequeno delay para a UI respirar antes do próximo turno
        yield return new WaitForSeconds(0.5f);

        NextTurn();
    }

    public IEnumerable<Unit> GetTurnQueue()
    {
        return turnQueue.Concat(actedUnits);
    }
}
