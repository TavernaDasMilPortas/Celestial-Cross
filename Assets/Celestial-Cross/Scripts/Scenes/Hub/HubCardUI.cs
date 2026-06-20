using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using CelestialCross.Progression;
using CelestialCross.System;

namespace CelestialCross.Scenes.Hub
{
    public class HubCardUI : MonoBehaviour
    {
        public enum CardState { Locked, Purchasable, Unlocked }

        [Header("Components")]
        public Image backgroundImage; // Pode ser null
        public Image iconImage;
        public TMP_Text titleText;
        public TMP_Text subtitleText;
        public Image progressBar;
        public GameObject lockOverlay;
        public Image statusIcon;
        public TMP_Text energyText;
        public Button buttonComponent;

        [Header("Giant Lock Animation (Optional)")]
        public Image bigLockImage; // Imagem central grande do cadeado
        public Sprite bigLockedSprite;
        public Sprite bigUnlockedSprite;
        
        [Header("Animation Settings")]
        public float unlockAnimationDuration = 1.5f; // Controle de tempo do dissolve/resintonização

        [Header("Icons")]
        public Sprite lockedStatusIcon;
        public Sprite completedStatusIcon;
        public Sprite availableStatusIcon;
        public Sprite combatNodeIcon;
        public Sprite dialogueNodeIcon;
        public Sprite unlockStatusIcon; // Novo ícone para a transição de desbloqueio

        [Header("Shaders & FX")]
        public Material corruptedMaterialTemplate; // Crie um material, configure os sliders e jogue aqui!
        public Sprite corruptedOverlaySprite; // Sprite opcional para servir de base para o overlay corrompido
        private Material corruptedMaterialInstance;
        private Image corruptedOverlayImage;

        private CardState currentState;
        private string unlockRequirementMessage = "";
        private StoryNode currentNode;

        public Action OnNodeClicked;

        private void Awake()
        {
            // Garantir que não está zero se o Unity não atualizou o prefab antigo
            if (unlockAnimationDuration <= 0f) unlockAnimationDuration = 1.5f;

            if (buttonComponent != null)
            {
                buttonComponent.onClick.AddListener(HandleClick);
            }
        }

        private void Start()
        {
            // Animação dramática de entrada do Card (Persona Style)
            // Aparece girando, escalando e dando um "kick" pro lugar
            transform.localScale = new Vector3(0.1f, 1.2f, 1f);
            transform.localRotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(-15f, 15f));
            
            Sequence entranceSeq = DOTween.Sequence();
            entranceSeq.Append(transform.DOScale(new Vector3(1.1f, 0.9f, 1f), 0.2f).SetEase(Ease.OutCirc));
            entranceSeq.Join(transform.DOLocalRotate(Vector3.zero, 0.2f).SetEase(Ease.OutBack));
            entranceSeq.Append(transform.DOScale(Vector3.one, 0.15f).SetEase(Ease.InOutBounce));
        }

        private void OnDestroy()
        {
            if (corruptedMaterialInstance != null)
            {
                Destroy(corruptedMaterialInstance);
            }
        }

        private void EnsureCorruptedOverlay()
        {
            if (corruptedOverlayImage == null)
            {
                GameObject overlayObj = new GameObject("CorruptedOverlay", typeof(RectTransform), typeof(Image));
                
                // Colocar como filho do card principal, logo atrás do cadeado ou do bigLockImage
                overlayObj.transform.SetParent(this.transform, false);
                if (bigLockImage != null)
                {
                    overlayObj.transform.SetSiblingIndex(bigLockImage.transform.GetSiblingIndex());
                }
                else if (lockOverlay != null)
                {
                    overlayObj.transform.SetSiblingIndex(lockOverlay.transform.GetSiblingIndex());
                }
                else
                {
                    overlayObj.transform.SetAsLastSibling();
                }
                
                RectTransform rt = overlayObj.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                corruptedOverlayImage = overlayObj.GetComponent<Image>();
                corruptedOverlayImage.color = Color.white; // Base branca para o shader

                if (corruptedOverlaySprite != null)
                {
                    corruptedOverlayImage.sprite = corruptedOverlaySprite;
                }

                if (corruptedMaterialTemplate != null)
                {
                    // Copia o material que você configurou para animá-lo de forma independente neste card
                    corruptedMaterialInstance = new Material(corruptedMaterialTemplate);
                    corruptedMaterialInstance.SetFloat("_DissolveAmount", 0f); // Garante que comece em 0
                    corruptedOverlayImage.material = corruptedMaterialInstance;
                }
                else
                {
                    // Fallback se você esquecer de arrastar o material
                    Shader fallbackShader = Shader.Find("UI/CorruptedFilter");
                    if (fallbackShader != null)
                    {
                        corruptedMaterialInstance = new Material(fallbackShader);
                        corruptedMaterialInstance.SetFloat("_DissolveAmount", 0f); // Garante que comece em 0
                        corruptedOverlayImage.material = corruptedMaterialInstance;
                    }
                }
            }
        }

        private void HandleClick()
        {
            if (currentState == CardState.Locked)
            {
                // Feedback visual Extravagante Persona 5
                transform.DOComplete();
                transform.DOShakePosition(0.4f, strength: 10f, vibrato: 20);

                if (corruptedMaterialInstance != null && corruptedOverlayImage != null)
                {
                    // Piscar vermelho em vez de alterar a intensidade de glitch
                    corruptedOverlayImage.color = Color.red;
                    corruptedOverlayImage.DOColor(Color.white, 0.5f);
                }

                // Chamar Mensageiro
                if (!string.IsNullOrEmpty(unlockRequirementMessage) && MessengerSystem.Instance != null)
                {
                    MessengerSystem.Instance.ShowMessage(unlockRequirementMessage, lockedStatusIcon);
                }
            }
            else
            {
                // Deixar o SceneController lidar com a progressão se Purchasable/Unlocked
                OnNodeClicked?.Invoke();
            }
        }

        public void PlayUnlockAnimation(Action onComplete = null)
        {
            if (currentState != CardState.Locked) return;

            // Feedback dramático: um "burst" branco e shake
            transform.DOComplete();
            transform.DOShakeScale(0.3f, 0.2f, 15);
            transform.DOShakeRotation(0.3f, 10f, 15);

            // Animação de desbloqueio ("corrupção liberada") do ícone de status (pequeno)
            if (statusIcon != null && unlockStatusIcon != null)
            {
                statusIcon.sprite = unlockStatusIcon;
                statusIcon.transform.DOScale(2f, 0.4f).SetEase(Ease.OutBack).SetLoops(2, LoopType.Yoyo);
            }

            // Animação Gigante Central (se o usuário arrastou as referências)
            if (bigLockImage != null && bigUnlockedSprite != null)
            {
                bigLockImage.gameObject.SetActive(true);
                bigLockImage.transform.DOComplete();
                
                // Chacoalha o cadeado central
                bigLockImage.transform.DOShakePosition(0.5f, 15f, 25).OnComplete(() =>
                {
                    // Muda pro destrancado
                    bigLockImage.sprite = bigUnlockedSprite;
                    
                    // Dá um pulso forte
                    bigLockImage.transform.DOScale(1.8f, 0.3f).SetEase(Ease.OutBack).SetLoops(2, LoopType.Yoyo).OnComplete(() => 
                    {
                        // Some dramáticamente
                        bigLockImage.DOFade(0f, 0.2f).SetEase(Ease.OutCubic);
                    });
                });
            }

            if (corruptedMaterialInstance != null)
            {
                corruptedMaterialInstance.SetFloat("_DissolveAmount", 0f); // Força para 0 antes da animação

                // Fade out dramático no shader usando o tempo configurável
                DOTween.To(() => corruptedMaterialInstance.GetFloat("_DissolveAmount"), 
                           x => corruptedMaterialInstance.SetFloat("_DissolveAmount", x), 
                           1f, unlockAnimationDuration).SetEase(Ease.InExpo).SetUpdate(true).OnComplete(() =>
                           {
                               if (corruptedOverlayImage != null) corruptedOverlayImage.gameObject.SetActive(false);
                               if (lockOverlay != null) lockOverlay.SetActive(false);
                               
                               // Pulo final das infos reveladas
                               if (titleText != null) 
                               {
                                   titleText.transform.localScale = Vector3.zero;
                                   titleText.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
                               }

                               onComplete?.Invoke();
                           });
            }
            else
            {
                if (corruptedOverlayImage != null) corruptedOverlayImage.gameObject.SetActive(false);
                if (lockOverlay != null) lockOverlay.SetActive(false);
                onComplete?.Invoke();
            }

            // Restaurar nomes instantaneamente mas revelar com a animação acima
            if (titleText != null) titleText.text = currentNode != null ? currentNode.Title : "???";
            currentState = CardState.Unlocked;
        }

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
            
            currentState = CardState.Unlocked;
        }

        public void SetupAsChapter(ChapterData chapter, int completedCount, int totalCount, bool isLocked)
        {
            EnsureCorruptedOverlay();
            currentState = isLocked ? CardState.Locked : CardState.Unlocked;

            if (iconImage != null) iconImage.gameObject.SetActive(false); 
            
            if (titleText != null) titleText.text = isLocked ? "???" : chapter.ChapterTitle;
            
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

            if (corruptedOverlayImage != null) corruptedOverlayImage.gameObject.SetActive(isLocked);
            if (lockOverlay != null) lockOverlay.SetActive(isLocked);
            if (energyText != null) energyText.gameObject.SetActive(false);
            
            if (bigLockImage != null)
            {
                bigLockImage.gameObject.SetActive(isLocked);
                if (isLocked && bigLockedSprite != null) 
                {
                    bigLockImage.sprite = bigLockedSprite;
                    bigLockImage.color = Color.white; // Reseta o fade caso tenha ocorrido antes
                }
            }

            if (statusIcon != null)
            {
                statusIcon.gameObject.SetActive(true);
                statusIcon.sprite = isLocked ? lockedStatusIcon : (completedCount == totalCount ? completedStatusIcon : availableStatusIcon);
            }

            if (isLocked)
            {
                unlockRequirementMessage = $"Complete os requisitos para liberar o capítulo: {chapter.ChapterTitle}";
            }
        }

        public void SetupAsNode(StoryNode node, bool isCompleted, bool isLocked, int remainingAttempts, bool canAffordItems = true)
        {
            currentNode = node;
            EnsureCorruptedOverlay();

            if (isLocked) currentState = CardState.Locked;
            else if (!canAffordItems) currentState = CardState.Purchasable;
            else currentState = CardState.Unlocked;

            if (iconImage != null)
            {
                if (node.NodeIcon != null)
                    iconImage.sprite = node.NodeIcon;
                else
                    iconImage.sprite = node is CombatStoryNode ? combatNodeIcon : dialogueNodeIcon;
                
                iconImage.gameObject.SetActive(true);
            }

            if (titleText != null) titleText.text = currentState == CardState.Locked ? "???" : node.Title;
            
            if (subtitleText != null)
            {
                if (currentState == CardState.Locked)
                    subtitleText.text = "Bloqueado";
                else if (currentState == CardState.Purchasable)
                    subtitleText.text = "Requer Itens";
                else if (remainingAttempts == 0)
                    subtitleText.text = "Limite atingido";
                else if (remainingAttempts > 0)
                    subtitleText.text = $"Tentativas: {remainingAttempts}";
                else
                    subtitleText.text = isCompleted ? "Concluído" : "Disponível";
            }

            if (progressBar != null) progressBar.transform.parent.gameObject.SetActive(false);
            
            if (corruptedOverlayImage != null) corruptedOverlayImage.gameObject.SetActive(currentState == CardState.Locked);
            if (lockOverlay != null) lockOverlay.SetActive(currentState == CardState.Locked);

            if (bigLockImage != null)
            {
                bigLockImage.gameObject.SetActive(currentState == CardState.Locked);
                if (currentState == CardState.Locked && bigLockedSprite != null)
                {
                    bigLockImage.sprite = bigLockedSprite;
                    bigLockImage.color = Color.white; // Reseta o fade
                }
            }

            if (energyText != null)
            {
                int energyCost = node.EntryCost != null ? node.EntryCost.EnergyCost : 0;
                energyText.text = energyCost > 0 ? $"Energia: {energyCost}" : "Energia: Grátis";
                energyText.gameObject.SetActive(currentState != CardState.Locked);
            }

            if (statusIcon != null)
            {
                statusIcon.gameObject.SetActive(true);
                if (currentState == CardState.Locked) statusIcon.sprite = lockedStatusIcon;
                else if (currentState == CardState.Purchasable) statusIcon.sprite = lockedStatusIcon; // Mostrar cadeado se falta item, mas no "Locked" total
                else statusIcon.sprite = isCompleted ? completedStatusIcon : availableStatusIcon;
            }

            if (currentState == CardState.Locked)
            {
                if (node.Requirement != null && node.Requirement.RequiresPreviousNode)
                {
                    unlockRequirementMessage = $"Complete a fase anterior para desbloquear.\nFalta: {node.Requirement.PreviousNodeID}";
                }
                else
                {
                    unlockRequirementMessage = "Esta fase est bloqueada. Continue jogando para desbloquear.";
                }
            }
        }
    }
}
