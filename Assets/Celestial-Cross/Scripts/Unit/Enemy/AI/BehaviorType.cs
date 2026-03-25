/// <summary>
/// Define o estilo geral de comportamento de um inimigo controlado por IA.
/// </summary>
public enum BehaviorType
{
    Aggressive,     // prioriza alvos com MAIS vida (assassino)
    Opportunist,    // prioriza alvos com MENOS vida (finalizar)
    Defensive,      // prioriza se mover para longe / proteger aliados
    Balanced,       // pondera ataque e posição igualmente
    Support         // prioriza buffs/cura em aliados (futuro)
}
