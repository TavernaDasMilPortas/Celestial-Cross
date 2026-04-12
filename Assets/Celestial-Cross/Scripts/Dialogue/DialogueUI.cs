using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controla a exibição visual da caixa de diálogo:
/// painel, imagem do personagem (centralizada), nome e texto com efeito typewriter.
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [Header("Referências de UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Image characterImage;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject continueIndicator;

    [Header("Escolhas")]
    [Tooltip("Container (ex: VerticalLayoutGroup) onde os botões de escolha serão instanciados.")]
    [SerializeField] private Transform choicesContainer;
    [Tooltip("Prefab de botão com TMP_Text filho. Será instanciado para cada opção.")]
    [SerializeField] private Button choiceButtonPrefab;

    [Header("Typewriter")]
    [SerializeField] private float typingSpeed = 40f;

    private Coroutine _typingCoroutine;
    private string _fullText;
    private Action<int> _onChoiceSelected;

    /// <summary>Retorna true enquanto o texto ainda está sendo "digitado".</summary>
    public bool IsTyping { get; private set; }

    /// <summary>
    /// Garante que toda a UI de diálogo comece escondida ao iniciar a cena.
    /// </summary>
    void Awake()
    {
        Hide();
    }

    /// <summary>
    /// Exibe uma entrada de diálogo: atualiza imagem, nome e inicia o typewriter.
    /// </summary>
    public void ShowEntry(DialogueEntry entry)
    {
        // --- Painel ---
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        // --- Imagem do personagem (centralizada) ---
        if (characterImage != null)
        {
            if (entry.characterSprite != null)
            {
                characterImage.sprite = entry.characterSprite;
                characterImage.enabled = true;
                characterImage.SetNativeSize();
            }
            else
            {
                characterImage.enabled = false;
            }
        }

        // --- Nome do personagem ---
        if (speakerNameText != null)
        {
            speakerNameText.text = entry.speakerName;
            speakerNameText.gameObject.SetActive(!string.IsNullOrEmpty(entry.speakerName));
        }

        // --- Texto com typewriter ---
        _fullText = entry.dialogueText ?? string.Empty;

        if (_typingCoroutine != null)
            StopCoroutine(_typingCoroutine);

        if (continueIndicator != null)
            continueIndicator.SetActive(false);

        _typingCoroutine = StartCoroutine(TypeText(_fullText));
    }

    /// <summary>
    /// Completa o texto instantaneamente (skip do typewriter).
    /// </summary>
    public void CompleteText()
    {
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
        }

        IsTyping = false;

        if (dialogueText != null)
            dialogueText.text = _fullText;

        if (continueIndicator != null)
            continueIndicator.SetActive(true);
    }

    /// <summary>
    /// Esconde toda a UI de diálogo.
    /// </summary>
    public void Hide()
    {
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
        }

        IsTyping = false;
        HideChoices();

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (characterImage != null)
            characterImage.enabled = false;

        if (continueIndicator != null)
            continueIndicator.SetActive(false);
    }

    /// <summary>
    /// Instancia botões de escolha dentro do container.
    /// Cada botão dispara onSelect com o índice correspondente.
    /// </summary>
    public void ShowChoices(DialogueChoice[] choices, Action<int> onSelect)
    {
        if (choicesContainer == null || choiceButtonPrefab == null) return;

        _onChoiceSelected = onSelect;

        if (continueIndicator != null)
            continueIndicator.SetActive(false);

        for (int i = 0; i < choices.Length; i++)
        {
            Button btn = Instantiate(choiceButtonPrefab, choicesContainer);
            btn.gameObject.SetActive(true);

            TMP_Text label = btn.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = choices[i].choiceText;

            int idx = i; // captura para closure
            btn.onClick.AddListener(() => _onChoiceSelected?.Invoke(idx));
        }

        choicesContainer.gameObject.SetActive(true);
    }

    /// <summary>
    /// Destrói os botões de escolha e esconde o container.
    /// </summary>
    public void HideChoices()
    {
        if (choicesContainer == null) return;

        foreach (Transform child in choicesContainer)
            Destroy(child.gameObject);

        choicesContainer.gameObject.SetActive(false);
        _onChoiceSelected = null;
    }

    private IEnumerator TypeText(string text)
    {
        IsTyping = true;

        if (dialogueText != null)
            dialogueText.text = string.Empty;

        float delay = typingSpeed > 0f ? 1f / typingSpeed : 0f;

        for (int i = 0; i < text.Length; i++)
        {
            if (dialogueText != null)
                dialogueText.text = text.Substring(0, i + 1);

            if (delay > 0f)
                yield return new WaitForSeconds(delay);
        }

        IsTyping = false;
        _typingCoroutine = null;

        if (continueIndicator != null)
            continueIndicator.SetActive(true);
    }
}
