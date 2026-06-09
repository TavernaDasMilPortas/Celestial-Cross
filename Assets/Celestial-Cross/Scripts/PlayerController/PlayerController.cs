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
        RefreshUI();
      
        Debug.Log($"Turno de {unit.DisplayName}");
    }

    public void SelectAction(int index)
    {
        if (activeUnit != null)
            activeUnit.SelectAction(index);
    }

    private Vector2 _pointerDownPos;
    private const float DRAG_THRESHOLD = 40f;

    void Update()
    {
        if (activeUnit == null)
            return;

        if (CelestialCross.Tutorial.TutorialManager.Instance != null && CelestialCross.Tutorial.TutorialManager.Instance.IsActive)
            return;

        // Seleção de ações (teclas numéricas)
        if (Input.GetKeyDown(KeyCode.Alpha0)) activeUnit.SelectAction(0);
        if (Input.GetKeyDown(KeyCode.Alpha1)) activeUnit.SelectAction(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) activeUnit.SelectAction(2);

        bool isClick = false;

        if (Input.GetMouseButtonDown(0))
        {
            _pointerDownPos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (Vector2.Distance(_pointerDownPos, Input.mousePosition) < DRAG_THRESHOLD)
                isClick = true;
        }

        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                _pointerDownPos = t.position;
                isClick = false;
            }
            else if (t.phase == TouchPhase.Ended)
            {
                if (Vector2.Distance(_pointerDownPos, t.position) < DRAG_THRESHOLD)
                    isClick = true;
            }
        }

        if (isClick)
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

        // O TargetSelector cuida ativamente do targeting agora.
        // Caching activeUnit.CurrentAction.Target aqui causa conflito com o AbilityExecutor 
        // em turnos subsequentes, pois a ação acha que a IA ou o tutorial pre-setou um alvo!
        // activeUnit.CurrentAction.Target = targetPos; 
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

    public void RefreshUI()
    {
        // Encontrar o CombatUIManager ou ActionBarUI na cena
        FindObjectOfType<ActionBarUI>()?.UpdateInteractability();
    }

    public void EndTurn()
    {
        activeUnit = null;
        FindObjectOfType<ActionBarUI>()?.ClearButtons();
        TurnManager.Instance.EndTurn();
    }

    // ==========================================
    // TUTORIAL HELPERS
    // ==========================================

    public void TutorialForceSelectAction(int index)
    {
        if (activeUnit != null) activeUnit.SelectAction(index);
    }

    public void TutorialForceTargetTile(Vector2Int gridPos)
    {
        if (activeUnit != null && activeUnit.CurrentAction != null)
        {
            activeUnit.CurrentAction.Target = gridPos;
            // Notifica o tutorial se estiver esperando por isso
            CelestialCross.Tutorial.TutorialManager.Instance?.NotifyTargetConfirmed();
        }
    }
}
