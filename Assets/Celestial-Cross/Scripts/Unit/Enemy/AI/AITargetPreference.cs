/// <summary>
/// Critério de seleção de alvo para a IA.
/// </summary>
public enum AITargetPreference
{
    HighestHealth,   // assassino: quer o mais forte
    LowestHealth,    // oportunista: quer finalizar
    Closest,         // pragmático: o mais perto
    Farthest,        // defensivo: ataque à distância
    HighestAttack,   // estratégico: neutralizar ameaças
    Random           // imprevisível
}
