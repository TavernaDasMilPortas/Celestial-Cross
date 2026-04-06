using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Gerencia o fluxo de uma sequência de diálogos.
/// Detecta touch/clique para avançar ou completar o typewriter.
/// Dispara OnDialogueEnd ao finalizar toda a sequência.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    [Header("Dados")]
    [Tooltip("Sequência de diálogos a ser reproduzida ao iniciar a cena.")]
    [SerializeField] private DialogueSequence dialogueSequence;

    [Header("UI")]
    [SerializeField] private DialogueUI dialogueUI;

    [Header("Configuração")]
    [Tooltip("Se true, inicia o diálogo automaticamente no Start().")]
    [SerializeField] private bool autoStart = true;

    [Header("Eventos")]
    [Tooltip("Invocado quando toda a sequência de diálogos termina.")]
    public UnityEvent OnDialogueEnd;

    private int _currentIndex = -1;
    private bool _isActive;

    void Start()
    {
        if (autoStart && dialogueSequence != null)
            StartDialogue(dialogueSequence);
    }

    void Update()
    {
        if (!_isActive) return;

        // Detecta touch (mobile) ou clique (desktop)
        bool inputDetected = Input.GetMouseButtonDown(0);

        if (!inputDetected && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
                inputDetected = true;
        }

        if (inputDetected)
            Advance();
    }

    /// <summary>
    /// Inicia (ou reinicia) uma sequência de diálogo.
    /// Pode ser chamado de fora para trocar a sequência em runtime.
    /// </summary>
    public void StartDialogue(DialogueSequence sequence)
    {
        if (sequence == null || sequence.entries == null || sequence.entries.Count == 0)
        {
            Debug.LogWarning("[DialogueManager] Sequência de diálogo vazia ou nula.");
            return;
        }

        dialogueSequence = sequence;
        _currentIndex = -1;
        _isActive = true;

        ShowNext();
    }

    /// <summary>
    /// Avança para o próximo diálogo, ou completa o typewriter se ainda estiver digitando.
    /// </summary>
    public void Advance()
    {
        if (!_isActive) return;

        if (dialogueUI != null && dialogueUI.IsTyping)
        {
            dialogueUI.CompleteText();
            return;
        }

        ShowNext();
    }

    private void ShowNext()
    {
        _currentIndex++;

        if (dialogueSequence == null || _currentIndex >= dialogueSequence.entries.Count)
        {
            EndDialogue();
            return;
        }

        DialogueEntry entry = dialogueSequence.entries[_currentIndex];

        if (dialogueUI != null)
            dialogueUI.ShowEntry(entry);
    }

    private void EndDialogue()
    {
        _isActive = false;
        _currentIndex = -1;

        if (dialogueUI != null)
            dialogueUI.Hide();

        OnDialogueEnd?.Invoke();
    }
}
