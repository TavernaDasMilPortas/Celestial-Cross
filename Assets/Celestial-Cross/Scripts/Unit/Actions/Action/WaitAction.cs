using UnityEngine;

public class WaitAction : UnitActionBase
{
    public override int Range { get; set; } = 0;
    
    protected override void Awake()
    {
        base.Awake();
        ActionName = "Esperar";
        ActionDescription = "Encerra o turno da unidade.";
        ActionCategory = UnitActionCategory.Ability; // Categorizado como habilidade para resetar estado
    }

    protected override ActionContext CreateContext()
    {
        return new ActionContext(unit);
    }

    protected override void OnEnter()
    {
        // Pula seleção de alvos e executa direto
        Execute();
    }

    protected override void OnUpdate() { }

    protected override void Resolve()
    {
        // Ao esperar, marcamos que a unidade já agiu E já moveu para encerrar o turno
        unit.hasMovedThisTurn = true;
        unit.hasActedThisTurn = true;
        Debug.Log($"[WaitAction] {unit.DisplayName} escolheu esperar. Encerrando turno.");
    }

    protected override void OnCancel() { }
}
