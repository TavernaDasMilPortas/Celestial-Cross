using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;

namespace CelestialCross.UI
{
    public class SplitScreenUIManager : MonoBehaviour
    {
        [Header("Backgrounds")]
        public RectTransform leftBackground;
        public RectTransform rightBackground;

        [Header("Modals")]
        public UnitModalUI leftModal;
        public UnitModalUI rightModal;
        
        [Header("Targeting Buttons")]
        public Transform targetButtonsContainer;
        public TargetButtonUI targetButtonPrefab;

        [Header("Animation Settings")]
        public float animDuration = 0.3f;
        public Ease animEase = Ease.OutBack;
        public Vector2 offscreenLeft = new Vector2(-1200, 0);
        public Vector2 offscreenRight = new Vector2(1200, 0);

        private List<TargetButtonUI> spawnedButtons = new List<TargetButtonUI>();
        private List<Unit> currentTargets;

        private Vector2 leftOriginalPos;
        private Vector2 rightOriginalPos;

        private Vector2 leftOriginalAnchorMin;
        private Vector2 leftOriginalAnchorMax;
        private Vector2 rightOriginalAnchorMin;
        private Vector2 rightOriginalAnchorMax;

        private void Awake()
        {
            if (leftBackground != null) 
            {
                leftOriginalPos = leftBackground.anchoredPosition;
                leftOriginalAnchorMin = leftBackground.anchorMin;
                leftOriginalAnchorMax = leftBackground.anchorMax;
            }
            if (rightBackground != null) 
            {
                rightOriginalPos = rightBackground.anchoredPosition;
                rightOriginalAnchorMin = rightBackground.anchorMin;
                rightOriginalAnchorMax = rightBackground.anchorMax;
            }
        }

        private void Start()
        {
            // Initial state: hidden
            HideInstant();
        }

        public void HideInstant()
        {
            leftBackground.anchoredPosition = new Vector2(offscreenLeft.x, leftOriginalPos.y);
            rightBackground.anchoredPosition = new Vector2(offscreenRight.x, rightOriginalPos.y);
            gameObject.SetActive(false);
        }

        public void Hide()
        {
            leftBackground.DOAnchorPos(new Vector2(offscreenLeft.x, leftOriginalPos.y), animDuration).SetEase(Ease.InBack);
            rightBackground.DOAnchorPos(new Vector2(offscreenRight.x, rightOriginalPos.y), animDuration).SetEase(Ease.InBack).OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
        }

        public void HideAll()
        {
            gameObject.SetActive(false);
        }

        public void ShowSplit(Unit attacker, List<Unit> targets)
        {
            if (attacker == null || targets == null || targets.Count == 0) return;
            gameObject.SetActive(true);

            currentTargets = targets;
            bool isAttackerAlly = attacker.Team == Team.Player;

            if (isAttackerAlly)
            {
                leftModal.UpdatePanel(attacker);
                rightModal.UpdatePanel(targets[0]);
            }
            else
            {
                rightModal.UpdatePanel(attacker);
                leftModal.UpdatePanel(targets[0]);
            }

            SetupTargetButtons(targets, isAttackerAlly);

            leftBackground.gameObject.SetActive(true);
            rightBackground.gameObject.SetActive(true);
            
            leftModal.gameObject.SetActive(true);
            rightModal.gameObject.SetActive(true);
            
            leftModal.SetLightningActive(true);
            rightModal.SetLightningActive(true);

            RectTransform rootRect = GetComponent<RectTransform>();
            float screenWidth = rootRect != null ? rootRect.rect.width : 1920f;
            float halfWidth = screenWidth / 2f;

            if (isAttackerAlly)
            {
                // Ally is on left. Right panel comes from behind.
                rightBackground.SetAsFirstSibling();
                
                // Left panel volta ao normal
                leftBackground.anchorMin = leftOriginalAnchorMin;
                leftBackground.anchorMax = leftOriginalAnchorMax;
                leftBackground.DOAnchorPos(leftOriginalPos, animDuration).SetEase(animEase);

                // Right panel começa de trás
                rightBackground.anchorMin = rightOriginalAnchorMin;
                rightBackground.anchorMax = rightOriginalAnchorMax;
                rightBackground.anchoredPosition = new Vector2(-halfWidth, rightOriginalPos.y);
                rightBackground.DOAnchorPos(rightOriginalPos, animDuration).SetEase(animEase);
            }
            else
            {
                // Enemy is on right. Left panel comes from behind.
                leftBackground.SetAsFirstSibling();

                // Right panel volta ao normal
                rightBackground.anchorMin = rightOriginalAnchorMin;
                rightBackground.anchorMax = rightOriginalAnchorMax;
                rightBackground.DOAnchorPos(rightOriginalPos, animDuration).SetEase(animEase);

                // Left panel começa de trás
                leftBackground.anchorMin = leftOriginalAnchorMin;
                leftBackground.anchorMax = leftOriginalAnchorMax;
                leftBackground.anchoredPosition = new Vector2(halfWidth, leftOriginalPos.y);
                leftBackground.DOAnchorPos(leftOriginalPos, animDuration).SetEase(animEase);
            }
        }

        public void ShowFullScreenTurn(Unit activeUnit)
        {
            if (activeUnit == null) return;
            gameObject.SetActive(true);

            bool isAlly = activeUnit.Team == Team.Player;
            ClearButtons();

            if (isAlly)
            {
                leftModal.gameObject.SetActive(true);
                leftModal.SetLightningActive(false);
                leftModal.UpdatePanel(activeUnit);
                
                rightModal.gameObject.SetActive(false);
                rightBackground.gameObject.SetActive(false);
                
                leftBackground.gameObject.SetActive(true);
                
                // Mantém âncoras originais (sem achatar)
                leftBackground.anchorMin = leftOriginalAnchorMin;
                leftBackground.anchorMax = leftOriginalAnchorMax;
                
                leftBackground.anchoredPosition = new Vector2(offscreenLeft.x, leftOriginalPos.y);
                
                // Sabendo que a posição original encosta a lateral do modal no centro da tela,
                // deslocar exatamente a metade da largura do PRÓPRIO rect empurra seu centro para o meio da tela.
                float leftShift = leftBackground.rect.width / 2f;
                leftBackground.DOAnchorPos(new Vector2(leftOriginalPos.x + leftShift, leftOriginalPos.y), animDuration).SetEase(animEase);
            }
            else
            {
                rightModal.gameObject.SetActive(true);
                rightModal.SetLightningActive(false);
                rightModal.UpdatePanel(activeUnit);
                
                leftModal.gameObject.SetActive(false);
                leftBackground.gameObject.SetActive(false);
                
                rightBackground.gameObject.SetActive(true);
                
                // Mantém âncoras originais (sem achatar)
                rightBackground.anchorMin = rightOriginalAnchorMin;
                rightBackground.anchorMax = rightOriginalAnchorMax;
                
                rightBackground.anchoredPosition = new Vector2(offscreenRight.x, rightOriginalPos.y);
                
                // Desloca exatamente a metade da largura para a esquerda
                float rightShift = rightBackground.rect.width / 2f;
                rightBackground.DOAnchorPos(new Vector2(rightOriginalPos.x - rightShift, rightOriginalPos.y), animDuration).SetEase(animEase);
            }
        }

        public void ShowInspect(Unit inspectedUnit)
        {
            if (inspectedUnit == null) return;
            gameObject.SetActive(true);

            bool isAlly = inspectedUnit.Team == Team.Player;
            
            if (isAlly)
            {
                leftModal.gameObject.SetActive(true);
                leftModal.SetLightningActive(false);
                leftModal.UpdatePanel(inspectedUnit);
                
                rightModal.gameObject.SetActive(false);
                rightBackground.gameObject.SetActive(false);
                
                leftBackground.gameObject.SetActive(true);
                leftBackground.anchoredPosition = new Vector2(offscreenLeft.x, leftOriginalPos.y);
                leftBackground.DOAnchorPos(leftOriginalPos, animDuration).SetEase(animEase);
            }
            else
            {
                rightModal.gameObject.SetActive(true);
                rightModal.SetLightningActive(false);
                rightModal.UpdatePanel(inspectedUnit);
                
                leftModal.gameObject.SetActive(false);
                leftBackground.gameObject.SetActive(false);
                
                rightBackground.gameObject.SetActive(true);
                rightBackground.anchoredPosition = new Vector2(offscreenRight.x, rightOriginalPos.y);
                rightBackground.DOAnchorPos(rightOriginalPos, animDuration).SetEase(animEase);
            }
            
            ClearButtons();
        }

        private void SetupTargetButtons(List<Unit> targets, bool isAttackerAlly)
        {
            ClearButtons();

            if (targets.Count <= 1) return; // Nao precisa de botoes para 1 alvo

            // Posiciona o container embaixo do modal correspondente
            targetButtonsContainer.SetParent(isAttackerAlly ? rightBackground : leftBackground, false);

            for (int i = 0; i < targets.Count; i++)
            {
                TargetButtonUI btn = Instantiate(targetButtonPrefab, targetButtonsContainer);
                btn.Setup(i, OnTargetButtonClicked);
                spawnedButtons.Add(btn);
            }

            spawnedButtons[0].SetSelected(true);
        }

        private void OnTargetButtonClicked(int index)
        {
            if (currentTargets == null || index < 0 || index >= currentTargets.Count) return;

            // Atualiza visual dos botoes
            for (int i = 0; i < spawnedButtons.Count; i++)
            {
                spawnedButtons[i].SetSelected(i == index);
            }

            // Atualiza o modal correspondente
            bool isAttackerAlly = currentTargets[0].Team != Team.Player; // O alvo do ataque é inverso ao atacante. Se alvo não é player, atacante é aliado
            if (isAttackerAlly)
            {
                rightModal.UpdatePanel(currentTargets[index]);
                rightModal.transform.DOPunchScale(new Vector3(0.05f, 0.05f, 0f), 0.2f);
            }
            else
            {
                leftModal.UpdatePanel(currentTargets[index]);
                leftModal.transform.DOPunchScale(new Vector3(0.05f, 0.05f, 0f), 0.2f);
            }
        }

        private void ClearButtons()
        {
            foreach (var btn in spawnedButtons)
            {
                if (btn != null) Destroy(btn.gameObject);
            }
            spawnedButtons.Clear();
        }
    }
}
