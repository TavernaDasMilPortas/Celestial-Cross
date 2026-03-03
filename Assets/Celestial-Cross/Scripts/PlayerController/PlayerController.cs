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

    void Update()
    {
        if (activeUnit == null)
            return;

        // Seleção de ações (teclas numéricas)
        if (Input.GetKeyDown(KeyCode.Alpha0)) activeUnit.SelectAction(0);
        if (Input.GetKeyDown(KeyCode.Alpha1)) activeUnit.SelectAction(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) activeUnit.SelectAction(2);

        activeUnit.UpdateAction();

        if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("[PlayerController] ENTER detectado");
            activeUnit.ConfirmAction();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            activeUnit.CancelAction();
    }

    public void EndTurn()
    {
        activeUnit = null;
        TurnManager.Instance.EndTurn();
    }
}
