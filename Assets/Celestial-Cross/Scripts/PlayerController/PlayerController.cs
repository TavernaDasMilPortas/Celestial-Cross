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
        if (unit == null)
        {
            Debug.LogWarning("[PlayerController] StartTurn called with null unit.");
            return;
        }

        activeUnit = unit;
     
        Debug.Log($"Turno de {unit.DisplayName}");
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

        // Em dispositivos mobile, só atualizamos o alvo ao ocorrer um toque/clique
        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            UpdateTargeting();
        }

        // O 'ConfirmAction()' original pelo Enter foi removido para priorizar toques ou botões de UI
        // que chamem explicitamente activeUnit.ConfirmAction() ou cliques duplos (OnExecuteRequested)

        if (Input.GetKeyDown(KeyCode.Escape))
            activeUnit.CancelAction();
    }

    private void UpdateTargeting()
    {
        if (activeUnit == null || activeUnit.CurrentAction == null) return;
        
        // Proteção contra objeto destruído guardado como interface
        if (activeUnit.CurrentAction is UnityEngine.Object obj && obj == null) return;

        if (GridMap.Instance == null) return;

        var targetPos = GridMap.Instance.GetMouseGridPosition();
        
        // Se estamos clicando fora da grid, podemos simplesmente ignorar
        if (targetPos.x == -1 && targetPos.y == -1) return;

        if (targetPos == activeUnit.CurrentAction.Target)
        {
            // Even if the target hasn't changed, we might want to ensure the highlight is still visible
            // GridMap.Instance.HighlightArea(_currentArea);
            return;
        }

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
        
        if (GridMap.Instance != null)
        {
            GridMap.Instance.HighlightArea(_currentArea);
        }
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
