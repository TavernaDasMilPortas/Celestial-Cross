using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject que armazena uma sequência completa de diálogos.
/// Crie pelo menu: Create → Celestial-Cross → Dialogue Sequence.
/// </summary>
[CreateAssetMenu(fileName = "NewDialogueSequence", menuName = "Celestial-Cross/Dialogue Sequence")]
public class DialogueSequence : ScriptableObject
{
    [Tooltip("Lista ordenada de falas que compõem esta sequência.")]
    public List<DialogueEntry> entries = new List<DialogueEntry>();
}
