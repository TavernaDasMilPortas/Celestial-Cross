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

    [Header("Typewriter")]
    [SerializeField] private float typingSpeed = 40f;

    private Coroutine _typingCoroutine;
    private string _fullText;

    /// <summary>Retorna true enquanto o texto ainda está sendo "digitado".</summary>
    public bool IsTyping { get; private set; }

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

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (characterImage != null)
            characterImage.enabled = false;

        if (continueIndicator != null)
            continueIndicator.SetActive(false);
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
