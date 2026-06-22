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
                
                // Left panel shrinks from full screen
                DOTween.To(() => leftBackground.anchorMax, x => leftBackground.anchorMax = x, new Vector2(leftOriginalAnchorMax.x, leftBackground.anchorMax.y), animDuration);
                leftBackground.DOAnchorPos(leftOriginalPos, animDuration).SetEase(animEase);

                // Right panel starts from the left (hidden behind LeftBackground) and slides to its place
                rightBackground.anchorMin = new Vector2(rightOriginalAnchorMin.x, rightBackground.anchorMin.y);
                rightBackground.anchorMax = new Vector2(rightOriginalAnchorMax.x, rightBackground.anchorMax.y);
                rightBackground.anchoredPosition = new Vector2(-halfWidth, rightOriginalPos.y);
                rightBackground.DOAnchorPos(rightOriginalPos, animDuration).SetEase(animEase);
            }
            else
            {
                // Enemy is on right. Left panel comes from behind.
                leftBackground.SetAsFirstSibling();

                // Right panel shrinks from full screen
                DOTween.To(() => rightBackground.anchorMin, x => rightBackground.anchorMin = x, new Vector2(rightOriginalAnchorMin.x, rightBackground.anchorMin.y), animDuration);
                rightBackground.DOAnchorPos(rightOriginalPos, animDuration).SetEase(animEase);

                // Left panel starts from the right (hidden behind RightBackground) and slides to its place
                leftBackground.anchorMin = new Vector2(leftOriginalAnchorMin.x, leftBackground.anchorMin.y);
                leftBackground.anchorMax = new Vector2(leftOriginalAnchorMax.x, leftBackground.anchorMax.y);
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
                leftBackground.anchoredPosition = new Vector2(offscreenLeft.x, leftOriginalPos.y);
                
                // Anima anchorMax.x de 0.5 para 1 (tela cheia)
                DOTween.To(() => leftBackground.anchorMax, x => leftBackground.anchorMax = x, new Vector2(1f, leftBackground.anchorMax.y), animDuration);
                leftBackground.DOAnchorPos(new Vector2(0, leftOriginalPos.y), animDuration).SetEase(animEase);
            }
            else
            {
                rightModal.gameObject.SetActive(true);
                rightModal.SetLightningActive(false);
                rightModal.UpdatePanel(activeUnit);
                
                leftModal.gameObject.SetActive(false);
                leftBackground.gameObject.SetActive(false);
                
                rightBackground.gameObject.SetActive(true);
                rightBackground.anchoredPosition = new Vector2(offscreenRight.x, rightOriginalPos.y);
                
                // Anima anchorMin.x de 0.5 para 0 (tela cheia para a esquerda)
                DOTween.To(() => rightBackground.anchorMin, x => rightBackground.anchorMin = x, new Vector2(0f, rightBackground.anchorMin.y), animDuration);
                rightBackground.DOAnchorPos(new Vector2(0, rightOriginalPos.y), animDuration).SetEase(animEase);
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
