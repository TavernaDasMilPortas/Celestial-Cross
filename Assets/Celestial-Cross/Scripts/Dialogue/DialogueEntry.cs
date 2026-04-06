using UnityEngine;

/// <summary>
/// Representa uma única fala dentro de uma sequência de diálogo.
/// Cada entry pode alterar o sprite exibido (expressão/personagem).
/// </summary>
[System.Serializable]
public struct DialogueEntry
{
    [Tooltip("Nome do personagem que está falando. Exibido acima do texto.")]
    public string speakerName;

    [TextArea(2, 5)]
    [Tooltip("Texto da fala.")]
    public string dialogueText;

    [Tooltip("Sprite do personagem (retrato/expressão). Deixe vazio para esconder a imagem.")]
    public Sprite characterSprite;
}
