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
    
    [Tooltip("Opcional: Padrão avançado de chefes (Gatilhos de Vida e trocas de perfil).")]
    [SerializeField] private AIPatternData patternData;

    public AIBehaviorProfile BehaviorProfile => behaviorProfile;
    public AIPatternData PatternData => patternData;

    public AIBrain Brain { get; private set; }

    public void SetBehaviorProfile(AIBehaviorProfile profile)
    {
        this.behaviorProfile = profile;
    }

    public void SetPatternData(AIPatternData pattern)
    {
        this.patternData = pattern;
        if (pattern != null && pattern.initialProfile != null)
        {
            this.behaviorProfile = pattern.initialProfile;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        Brain = GetComponent<AIBrain>();
    }
}
