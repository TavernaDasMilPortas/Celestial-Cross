using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Coloque este script em um sprite/UI Image do CHIBI.
/// Ao clicar, inicia a sequência de diálogo via DialogueManager.
/// Quando o diálogo termina (via OnDialogueEnd), o CHIBI reaparece.
/// Requer um Collider2D no mesmo GameObject para detecção de clique.
/// </summary>
public class ChibiInteraction : MonoBehaviour, IPointerClickHandler
{
    [Header("Referências")]
    [Tooltip("Arraste o DialogueManager da cena aqui.")]
    [SerializeField] private DialogueManager dialogueManager;

    [Tooltip("Sequência de diálogo que será reproduzida ao clicar no CHIBI.")]
    [SerializeField] private DialogueSequence dialogueSequence;

    /// <summary>
    /// Detecta clique via IPointerClickHandler (funciona com UI e Physics Raycaster).
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (dialogueManager == null || dialogueSequence == null) return;

        // Esconde o CHIBI enquanto o diálogo está ativo
        gameObject.SetActive(false);

        // Inicia o diálogo
        dialogueManager.StartDialogue(dialogueSequence);
    }

    /// <summary>
    /// Chamado pelo UnityEvent OnDialogueEnd do DialogueManager.
    /// Reativa o CHIBI para permitir repetir o diálogo.
    /// </summary>
    public void OnDialogueFinished()
    {
        gameObject.SetActive(true);
    }
}
