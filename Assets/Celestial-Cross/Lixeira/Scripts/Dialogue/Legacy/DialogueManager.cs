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
    private bool _waitingForChoice;
    private DialogueEntry _pendingEntry;

    // Branch: sequência de entries gerada por uma escolha
    private DialogueEntry[] _branchEntries;
    private int _branchIndex;

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
        _branchEntries = null;
        _branchIndex = 0;

        ShowNext();
    }

    /// <summary>
    /// Avança para o próximo diálogo, ou completa o typewriter se ainda estiver digitando.
    /// </summary>
    public void Advance()
    {
        if (!_isActive) return;
        if (_waitingForChoice) return; // bloqueia input enquanto escolhas estão visíveis

        if (dialogueUI != null && dialogueUI.IsTyping)
        {
            dialogueUI.CompleteText();
            return;
        }

        // Se a entry atual tem choices, mostra as opções ao invés de avançar
        if (_pendingEntry.HasChoices)
        {
            _waitingForChoice = true;
            dialogueUI.ShowChoices(_pendingEntry.choices, OnChoiceSelected);
            return;
        }

        // Se estamos dentro de um branch, avança nele
        if (_branchEntries != null)
        {
            _branchIndex++;

            if (_branchIndex < _branchEntries.Length)
            {
                ShowBranchEntry();
                return;
            }

            // Branch terminou, volta para a sequência principal
            _branchEntries = null;
            _branchIndex = 0;
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
        _pendingEntry = entry;
        SetEntryFlag(entry);

        if (dialogueUI != null)
            dialogueUI.ShowEntry(entry);
    }

    private void ShowBranchEntry()
    {
        _pendingEntry = _branchEntries[_branchIndex];
        SetEntryFlag(_pendingEntry);

        if (dialogueUI != null)
            dialogueUI.ShowEntry(_pendingEntry);
    }

    private void SetEntryFlag(DialogueEntry entry)
    {
        if (!string.IsNullOrEmpty(entry.flagToSet) && DialogueFlagManager.Instance != null)
            DialogueFlagManager.Instance.SetFlag(entry.flagToSet);
    }

    /// <summary>
    /// Chamado pela UI quando o jogador seleciona uma opção de escolha.
    /// </summary>
    private void OnChoiceSelected(int choiceIndex)
    {
        if (!_waitingForChoice) return;
        _waitingForChoice = false;

        DialogueChoice choice = _pendingEntry.choices[choiceIndex];

        // Seta a flag da escolha, se definida
        if (!string.IsNullOrEmpty(choice.flagToSet) && DialogueFlagManager.Instance != null)
            DialogueFlagManager.Instance.SetFlag(choice.flagToSet);

        if (dialogueUI != null)
            dialogueUI.HideChoices();

        if (choice.responseEntries != null && choice.responseEntries.Length > 0)
        {
            _branchEntries = choice.responseEntries;
            _branchIndex = 0;
            ShowBranchEntry();
        }
        else
        {
            // Choice sem respostas, avança direto
            ShowNext();
        }
    }

    private void EndDialogue()
    {
        _isActive = false;
        _currentIndex = -1;
        _waitingForChoice = false;
        _branchEntries = null;
        _branchIndex = 0;

        if (dialogueUI != null)
            dialogueUI.Hide();

        OnDialogueEnd?.Invoke();
    }
}
