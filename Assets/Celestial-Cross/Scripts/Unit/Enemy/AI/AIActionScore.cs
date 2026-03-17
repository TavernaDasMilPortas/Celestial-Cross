using UnityEngine;

/// <summary>
/// Resultado da avaliação de uma ação pela IA.
/// Encapsula qual ação, qual alvo/tile e a pontuação calculada.
/// </summary>
public struct AIActionScore
{
    public int actionIndex;         // índice na lista Unit.actions
    public Unit target;             // alvo principal (null se MoveAction)
    public Vector2Int moveTarget;   // tile destino (usado para MoveAction)
    public float score;             // pontuação final

    public AIActionScore(int actionIndex, float score)
    {
        this.actionIndex = actionIndex;
        this.score = score;
        this.target = null;
        this.moveTarget = Vector2Int.zero;
    }
}
