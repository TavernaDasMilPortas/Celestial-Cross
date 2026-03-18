using UnityEngine;
using System.Collections.Generic;

public class CombatUIManager : MonoBehaviour
{
    public static CombatUIManager Instance { get; private set; }

    [SerializeField] private UnitPanelUI unitPanelUI;
    [SerializeField] private ActionBarUI actionBarUI;
    [SerializeField] private CombatForecastUI combatForecastUI;
    [SerializeField] private TurnTimelineUI turnTimelineUI;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        TurnManager.OnTurnStarted += HandleTurnStarted;
        TurnManager.OnQueueChanged += HandleQueueChanged;
    }

    private Unit currentActiveUnit;

    private void OnDestroy()
    {
        TurnManager.OnTurnStarted -= HandleTurnStarted;
        TurnManager.OnQueueChanged -= HandleQueueChanged;
        if (currentActiveUnit != null)
            currentActiveUnit.OnActionChanged -= HandleActionChanged;
    }

    private void HandleTurnStarted(Unit unit)
    {
        if (currentActiveUnit != null)
            currentActiveUnit.OnActionChanged -= HandleActionChanged;

        currentActiveUnit = unit;
        
        if (currentActiveUnit != null)
            currentActiveUnit.OnActionChanged += HandleActionChanged;

        // Update player UI if it's player's turn
        bool isPlayerTurn = unit is Pet || unit.CompareTag("Player");
        
        if (isPlayerTurn)
        {
            if (unitPanelUI != null) unitPanelUI.UpdatePanel(unit);
            if (actionBarUI != null) actionBarUI.GenerateButtons(unit);
        }
        else
        {
            if (actionBarUI != null) actionBarUI.ClearButtons();
            if (combatForecastUI != null) combatForecastUI.Hide();
        }
    }

    private void HandleActionChanged(IUnitAction newAction)
    {
        if (combatForecastUI != null)
            combatForecastUI.SetAction(newAction);
    }

    private void HandleQueueChanged(IEnumerable<Unit> queue)
    {
        if (turnTimelineUI != null)
            turnTimelineUI.UpdateTimeline(queue);
    }
}
