using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CelestialCross.Dialogue.Manager
{
    public class DialogueUI : MonoBehaviour
    {
        [Header("Main Panels")]
        [SerializeField] private GameObject mainDialoguePanel;
        [SerializeField] private GameObject choicesPanel;

        [Header("Text Components")]
        [SerializeField] private TMP_Text speakerNameText;
        [SerializeField] private TMP_Text dialogueText;

        [Header("Visuals")]
        [SerializeField] private Image characterPortrait;
        [SerializeField] private GameObject continueIndicator;

        [Header("Choices")]
        [SerializeField] private Transform choicesContainer;
        [SerializeField] private Button choiceButtonPrefab;

        [Header("Settings")]
        [SerializeField] private float typingSpeed = 0.05f;

        private Coroutine _typingCoroutine;
        private string _fullText;
        public bool IsTyping { get; private set; }

        public void ShowSpeech(string speaker, string text, Sprite portrait)
        {
            if (mainDialoguePanel == null) return;
            
            mainDialoguePanel.SetActive(true);
            if (choicesPanel != null) choicesPanel.SetActive(false);
            
            if (speakerNameText != null)
            {
                speakerNameText.text = speaker;
                speakerNameText.gameObject.SetActive(!string.IsNullOrEmpty(speaker));
            }
            
            if (characterPortrait != null)
            {
                characterPortrait.sprite = portrait;
                characterPortrait.enabled = (portrait != null);
            }

            if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
            _typingCoroutine = StartCoroutine(TypeText(text));
        }

        public void ShowChoices(List<string> choices, Action<int> onChoiceSelected)
        {
            mainDialoguePanel.SetActive(true); // Opcional: manter fundo da fala visível
            choicesPanel.SetActive(true);
            
            // Limpar botões antigos
            foreach (Transform child in choicesContainer) Destroy(child.gameObject);

            for (int i = 0; i < choices.Count; i++)
            {
                int index = i;
                Button btn = Instantiate(choiceButtonPrefab, choicesContainer);
                btn.GetComponentInChildren<TMP_Text>().text = choices[i];
                btn.onClick.AddListener(() => {
                    choicesPanel.SetActive(false);
                    onChoiceSelected?.Invoke(index);
                });
            }
        }

        public void SkipTypewriter()
        {
            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
                _typingCoroutine = null;
            }
            dialogueText.text = _fullText;
            IsTyping = false;
            if (continueIndicator != null) continueIndicator.SetActive(true);
        }

        private IEnumerator TypeText(string text)
        {
            _fullText = text;
            dialogueText.text = "";
            IsTyping = true;
            if (continueIndicator != null) continueIndicator.SetActive(false);

            foreach (char c in text.ToCharArray())
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(typingSpeed);
            }

            IsTyping = false;
            if (continueIndicator != null) continueIndicator.SetActive(true);
            _typingCoroutine = null;
        }

        public void Hide()
        {
            mainDialoguePanel.SetActive(false);
            choicesPanel.SetActive(false);
        }
    }
}
