using Celestial_Cross.Scripts.Abilities;
using UnityEngine;
using System.Collections.Generic;


public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    private Unit activeUnit;
    private List<Vector2Int> _currentArea = new();

    void Awake()
    {
        Instance = this;
    }

    public void StartTurn(Unit unit)
    {
        activeUnit = unit;
     
        Debug.Log($"Turno de {unit.name}");
    }

    public void SelectAction(int index)
    {
        if (activeUnit != null)
            activeUnit.SelectAction(index);
    }

    void Update()
    {
        if (activeUnit == null)
            return;

        // Seleção de ações (teclas numéricas)
        if (Input.GetKeyDown(KeyCode.Alpha0)) activeUnit.SelectAction(0);
        if (Input.GetKeyDown(KeyCode.Alpha1)) activeUnit.SelectAction(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) activeUnit.SelectAction(2);

        UpdateTargeting();

        // O 'ConfirmAction()' original pelo Enter foi removido para priorizar toques ou botões de UI
        // que chamem explicitamente activeUnit.ConfirmAction() ou cliques duplos (OnExecuteRequested)

        if (Input.GetKeyDown(KeyCode.Escape))
            activeUnit.CancelAction();
    }

    private void UpdateTargeting()
    {
        if (activeUnit == null || activeUnit.CurrentAction == null) return;

        var targetPos = GridManager.Instance.GetMouseGridPosition();
        if (targetPos == activeUnit.CurrentAction.Target) return;

        activeUnit.CurrentAction.Target = targetPos;
        
        var pattern = activeUnit.CurrentAction.GetAreaPattern();
        if (pattern != null && pattern.canRotate)
        {
            var direction = GetDirection(activeUnit.GridPosition, targetPos);
            _currentArea = AreaResolver.ResolveCells(targetPos, pattern, direction);
        }
        else
        {
            _currentArea = AreaResolver.ResolveCells(targetPos, pattern);
        }
        
        GridManager.Instance.HighlightArea(_currentArea);
    }

    private Direction GetDirection(Vector2Int origin, Vector2Int target)
    {
        Vector2Int delta = target - origin;
        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

        if (angle < 0) angle += 360;

        if (angle >= 337.5 || angle < 22.5) return Direction.E;
        if (angle >= 22.5 && angle < 67.5) return Direction.NE;
        if (angle >= 67.5 && angle < 112.5) return Direction.N;
        if (angle >= 112.5 && angle < 157.5) return Direction.NW;
        if (angle >= 157.5 && angle < 202.5) return Direction.W;
        if (angle >= 202.5 && angle < 247.5) return Direction.SW;
        if (angle >= 247.5 && angle < 292.5) return Direction.S;
        if (angle >= 292.5 && angle < 337.5) return Direction.SE;

        return Direction.E; // Fallback
    }

    public void EndTurn()
    {
        activeUnit = null;
        TurnManager.Instance.EndTurn();
    }
}
