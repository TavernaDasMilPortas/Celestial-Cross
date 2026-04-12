using UnityEngine;

/// <summary>
/// Representa uma opção de escolha dentro de um diálogo.
/// O jogador vê o choiceText como botão e, ao clicar, o personagem
/// responde com a sequência de responseEntries.
/// </summary>
[System.Serializable]
public struct DialogueChoice
{
    [Tooltip("Texto do botão que o jogador vê.")]
    public string choiceText;

    [Tooltip("Sequência de respostas ao selecionar esta opção. Pode ter várias entries para criar branches longos.")]
    public DialogueEntry[] responseEntries;
}
