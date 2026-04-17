/// <summary>
/// Critério de seleção de alvo para a IA.
/// </summary>
public enum AITargetPreference
{
    LowestHealth,    // Focar em unidades com a menor vida
    HighestHealth,   // assassino: quer o mais forte
    Closest,         // pragmático: o mais perto
    Farthest,        // defensivo: ataque à distância
    HighestAttack,   // estratégico: neutralizar ameaças
    Random,          // imprevisível
    PrioritizeRole,  // Focar em alvos de uma role específica
    PrioritizeClass  // Focar em alvos de uma class específica
}
