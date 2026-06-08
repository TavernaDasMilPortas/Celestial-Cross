using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Progression;

namespace CelestialCross.Scenes.Hub
{
    public class HubCardUI : MonoBehaviour
    {
        [Header("Components")]
        public Image iconImage;
        public TMP_Text titleText;
        public TMP_Text subtitleText;
        public Image progressBar;
        public GameObject lockOverlay;
        public Image statusIcon;
        public TMP_Text energyText;
        public Button buttonComponent;

        [Header("Icons")]
        public Sprite lockedStatusIcon;
        public Sprite completedStatusIcon;
        public Sprite availableStatusIcon;
        public Sprite combatNodeIcon;
        public Sprite dialogueNodeIcon;

        public void SetupAsCategory(HubCategorySO category, int completedCount, int totalCount)
        {
            if (iconImage != null)
            {
                iconImage.sprite = category.Icon;
                iconImage.gameObject.SetActive(category.Icon != null);
            }
            
            if (titleText != null) titleText.text = category.CategoryName;
            
            if (subtitleText != null)
            {
                if (totalCount > 0)
                    subtitleText.text = $"{completedCount}/{totalCount} Concluídos";
                else
                    subtitleText.text = "Sem conteúdo";
            }

            if (progressBar != null)
            {
                progressBar.fillAmount = totalCount > 0 ? (float)completedCount / totalCount : 0f;
                progressBar.transform.parent.gameObject.SetActive(true);
            }

            if (lockOverlay != null) lockOverlay.SetActive(false);
            if (energyText != null) energyText.gameObject.SetActive(false);
            if (statusIcon != null) statusIcon.gameObject.SetActive(false);
        }

        public void SetupAsChapter(ChapterData chapter, int completedCount, int totalCount, bool isLocked)
        {
            if (iconImage != null) iconImage.gameObject.SetActive(false); // Can add chapter icon later
            
            if (titleText != null) titleText.text = chapter.ChapterTitle;
            
            if (subtitleText != null)
            {
                if (isLocked)
                    subtitleText.text = "Bloqueado";
                else
                    subtitleText.text = $"{completedCount}/{totalCount} Nós Concluídos";
            }

            if (progressBar != null)
            {
                progressBar.fillAmount = totalCount > 0 ? (float)completedCount / totalCount : 0f;
                progressBar.transform.parent.gameObject.SetActive(!isLocked);
            }

            if (lockOverlay != null) lockOverlay.SetActive(isLocked);
            if (energyText != null) energyText.gameObject.SetActive(false);
            
            if (statusIcon != null)
            {
                statusIcon.gameObject.SetActive(true);
                statusIcon.sprite = isLocked ? lockedStatusIcon : (completedCount == totalCount ? completedStatusIcon : availableStatusIcon);
            }

            if (buttonComponent != null) buttonComponent.interactable = !isLocked;
        }

        public void SetupAsNode(StoryNode node, bool isCompleted, bool isLocked, int remainingAttempts)
        {
            if (iconImage != null)
            {
                if (node.NodeIcon != null)
                    iconImage.sprite = node.NodeIcon;
                else
                    iconImage.sprite = node is CombatStoryNode ? combatNodeIcon : dialogueNodeIcon;
                
                iconImage.gameObject.SetActive(true);
            }

            if (titleText != null) titleText.text = node.Title;
            
            if (subtitleText != null)
            {
                if (isLocked)
                    subtitleText.text = "Bloqueado";
                else if (remainingAttempts == 0)
                    subtitleText.text = "Limite atingido";
                else if (remainingAttempts > 0)
                    subtitleText.text = $"Tentativas: {remainingAttempts}";
                else
                    subtitleText.text = isCompleted ? "Concluído" : "Disponível";
            }

            if (progressBar != null) progressBar.transform.parent.gameObject.SetActive(false);
            
            if (lockOverlay != null) lockOverlay.SetActive(isLocked);

            if (energyText != null)
            {
                int energyCost = node.EntryCost != null ? node.EntryCost.EnergyCost : 0;
                energyText.text = energyCost > 0 ? $"Energia: {energyCost}" : "Energia: Grátis";
                energyText.gameObject.SetActive(true);
            }

            if (statusIcon != null)
            {
                statusIcon.gameObject.SetActive(true);
                statusIcon.sprite = isLocked ? lockedStatusIcon : (isCompleted ? completedStatusIcon : availableStatusIcon);
            }

            if (buttonComponent != null) buttonComponent.interactable = !isLocked && remainingAttempts != 0;
        }
    }
}
