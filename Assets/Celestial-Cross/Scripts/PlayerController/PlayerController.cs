using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    private Unit activeUnit;

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

        activeUnit.UpdateAction();

        // O 'ConfirmAction()' original pelo Enter foi removido para priorizar toques ou botões de UI
        // que chamem explicitamente activeUnit.ConfirmAction() ou cliques duplos (OnExecuteRequested)

        if (Input.GetKeyDown(KeyCode.Escape))
            activeUnit.CancelAction();
    }

    public void EndTurn()
    {
        activeUnit = null;
        TurnManager.Instance.EndTurn();
    }
}
