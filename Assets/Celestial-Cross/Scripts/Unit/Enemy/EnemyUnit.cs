using UnityEngine;

/// <summary>
/// Subclasse de Unit para inimigos controlados por IA.
/// Usa o mesmo pipeline de ações (UnitData + IExecutableDefinitionData) que o player.
/// O AIBrain é adicionado automaticamente via RequireComponent.
/// </summary>
[RequireComponent(typeof(AIBrain))]
public class EnemyUnit : Unit
{
    [Header("AI")]
    [SerializeField] private AIBehaviorProfile behaviorProfile;

    public AIBehaviorProfile BehaviorProfile => behaviorProfile;

    public AIBrain Brain { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        Brain = GetComponent<AIBrain>();
    }
}
