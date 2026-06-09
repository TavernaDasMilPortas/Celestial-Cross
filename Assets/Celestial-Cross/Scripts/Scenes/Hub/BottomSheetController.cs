using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Progression;
using CelestialCross.System;

namespace CelestialCross.Scenes.Hub
{
    public class BottomSheetController : MonoBehaviour
    {
        [Header("Animation")]
        public RectTransform panelTransform;
        public float slideSpeed = 10f;
        private Vector2 hiddenPos = new Vector2(0, -2000);
        private Vector2 visiblePos = new Vector2(0, 0);
        private bool isShowing = false;

        [Header("UI References")]
        public Image nodeIcon;
        public TMP_Text titleText;
        public TMP_Text energyText;
        public TMP_Text attemptsText;
        public TMP_Text resetText;
        public Transform rewardsContainer;
        public Transform itemCostsContainer;
        public GameObject itemCostPrefab; // A simple text+icon prefab
        public GameObject rewardIconPrefab; // Need a generic reward icon prefab

        public Button btnStart;
        public Button btnClose;

        private StoryNode currentNode;

        private void Awake()
        {
            if (panelTransform == null) panelTransform = GetComponent<RectTransform>();
            panelTransform.anchoredPosition = hiddenPos;
            
            if (btnClose != null) btnClose.onClick.AddListener(Hide);
            if (btnStart != null) btnStart.onClick.AddListener(OnStartClicked);
        }

        private void Update()
        {
            Vector2 target = isShowing ? visiblePos : hiddenPos;
            panelTransform.anchoredPosition = Vector2.Lerp(panelTransform.anchoredPosition, target, Time.deltaTime * slideSpeed);
        }

        public void Show(StoryNode node)
        {
            currentNode = node;
            
            if (titleText != null) titleText.text = node.Title;
            
            if (nodeIcon != null)
            {
                nodeIcon.sprite = node.NodeIcon;
                nodeIcon.gameObject.SetActive(nodeIcon.sprite != null);
            }

            // Entry Cost
            if (node.EntryCost != null)
            {
                if (energyText != null) energyText.text = node.EntryCost.EnergyCost > 0 ? $"Energia: {node.EntryCost.EnergyCost}" : "Energia: 0";
                
                // Item costs
                foreach (Transform child in itemCostsContainer) Destroy(child.gameObject);
                if (node.EntryCost.ItemCosts != null)
                {
                    foreach (var cost in node.EntryCost.ItemCosts)
                    {
                        var go = Instantiate(itemCostPrefab, itemCostsContainer);
                        go.SetActive(true);
                        var txt = go.GetComponentInChildren<TMP_Text>();
                        if (txt != null) txt.text = $"{cost.Amount}x {cost.DisplayName}";
                    }
                }
            }

            // Attempts
            int completions = ProgressionService.Instance != null ? ProgressionService.Instance.GetCompletionCount(node) : 0;
            if (attemptsText != null)
            {
                if (node.MaxCompletions == -1)
                    attemptsText.text = "Tentativas: Ilimitadas";
                else
                    attemptsText.text = $"Tentativas Restantes: {Mathf.Max(0, node.MaxCompletions - completions)}";
            }

            // Reset Policy
            if (resetText != null)
            {
                if (node.ResetPolicy.ResetType == CompletionResetType.Never)
                    resetText.text = "Sem reset automático";
                else
                    resetText.text = $"Reseta: {node.ResetPolicy.ResetType}";
            }

            // Rewards
            foreach (Transform child in rewardsContainer) Destroy(child.gameObject);
            var activeRewards = ProgressionService.Instance != null ? ProgressionService.Instance.GetRewardsForNode(node) : new global::System.Collections.Generic.List<CelestialCross.Data.Rewards.RewardDefinition>();
            foreach (var reward in activeRewards)
            {
                var go = Instantiate(rewardIconPrefab, rewardsContainer);
                go.SetActive(true);
                // Here we would configure the reward UI icon
            }

            isShowing = true;

            if (CelestialCross.System.BetterUIFixer.Instance != null)
            {
                CelestialCross.System.BetterUIFixer.Instance.RefreshImages(gameObject);
            }
        }

        public void Hide()
        {
            isShowing = false;
        }

        private void OnStartClicked()
        {
            if (currentNode == null) return;
            
            if (ProgressionService.Instance != null)
            {
                bool started = ProgressionService.Instance.TryStartNode(currentNode);
                if (started)
                {
                    Hide();
                }
            }
        }
    }
}
