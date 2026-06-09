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

    [Header("Flags")]
    [Tooltip("Flag que será ativada quando o jogador selecionar esta opção. Deixe vazio se não precisa.")]
    public string flagToSet;

    [Tooltip("Flags necessárias para esta opção estar disponível. TODAS devem existir.")]
    public string[] requiredFlags;

    [Tooltip("Flags que BLOQUEIAM esta opção. Se QUALQUER uma existir, a opção fica indisponível.")]
    public string[] blockedByFlags;
}

