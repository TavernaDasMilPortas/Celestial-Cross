using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Progression;

namespace CelestialCross.Scenes.Hub
{
    public class HubSceneController : MonoBehaviour
    {
        [Header("Flow")]
        [SerializeField] private string preparationSceneName = "PreparationScene";
        [SerializeField] private string inventorySceneName = "InventoryScene";
        [SerializeField] private string unitSceneName = "UnitScene";
        [SerializeField] private string shopSceneName = "ShopScene";

        [Header("Data")]
        [SerializeField] private List<HubCategorySO> categories = new List<HubCategorySO>();

        [Header("Top Bar")]
        [SerializeField] private TMP_Text moneyText;
        [SerializeField] private TMP_Text energyText;
        [SerializeField] private Button btnGoInventory;
        [SerializeField] private Button btnGoUnit;
        [SerializeField] private Button btnGoShop;

        [Header("Stack Panel")]
        [SerializeField] private GameObject stackPanel;
        [SerializeField] private TMP_Text stackTitleText;
        [SerializeField] private Transform stackContentContainer;
        [SerializeField] private Button btnBack;

        [Header("Cards & Bottom Sheet")]
        [SerializeField] private HubCardUI genericCardPrefab;
        [SerializeField] private BottomSheetController bottomSheet;

        private Stack<Action> navigationStack = new Stack<Action>();
        private string currentTitle = "";

        void Start()
        {
            if (AccountManager.Instance != null && AccountManager.Instance.PlayerAccount != null)
                RefreshAccountUI();
            else
                AccountManager.OnAccountReady += HandleAccountReady;

            if (btnGoInventory != null) btnGoInventory.onClick.AddListener(GoToInventoryScene);
            if (btnGoUnit != null) btnGoUnit.onClick.AddListener(GoToUnitScene);
            if (btnGoShop != null) btnGoShop.onClick.AddListener(GoToShopScene);
            if (btnBack != null) btnBack.onClick.AddListener(PopScreen);

            // Initial state
            PushScreen("Modo de Jogo", BuildCategoryCards);
        }

        private void HandleAccountReady()
        {
            AccountManager.OnAccountReady -= HandleAccountReady;
            RefreshAccountUI();
        }

        private void OnDestroy()
        {
            AccountManager.OnAccountReady -= HandleAccountReady;
        }

        public void RefreshAccountUI()
        {
            if (AccountManager.Instance == null || AccountManager.Instance.PlayerAccount == null) return;
            if (moneyText != null) moneyText.text = $"Dinheiro: {AccountManager.Instance.PlayerAccount.Money}";
            if (energyText != null) energyText.text = $"Energia: {AccountManager.Instance.PlayerAccount.Energy}";
        }

        #region Navigation Stack
        public void PushScreen(string title, Action buildAction)
        {
            navigationStack.Push(buildAction);
            currentTitle = title;
            ExecuteCurrentScreen();
        }

        public void PopScreen()
        {
            if (navigationStack.Count > 1)
            {
                navigationStack.Pop(); // Remove current
                ExecuteCurrentScreen();
            }
        }

        private void ExecuteCurrentScreen()
        {
            if (navigationStack.Count == 0) return;

            ClearContainer();
            if (bottomSheet != null) bottomSheet.Hide();

            if (stackTitleText != null) stackTitleText.text = currentTitle;
            if (btnBack != null) btnBack.gameObject.SetActive(navigationStack.Count > 1);

            Action buildAction = navigationStack.Peek();
            buildAction?.Invoke();

            if (CelestialCross.System.BetterUIFixer.Instance != null && stackContentContainer != null)
            {
                CelestialCross.System.BetterUIFixer.Instance.RefreshImages(stackContentContainer.gameObject);
            }
        }

        private void ClearContainer()
        {
            if (stackContentContainer == null) return;
            foreach (Transform child in stackContentContainer)
                Destroy(child.gameObject);
        }
        #endregion

        #region Builders
        private void BuildCategoryCards()
        {
            if (categories == null) return;
            var account = AccountManager.Instance?.PlayerAccount;

            foreach (var cat in categories)
            {
                if (cat == null) continue;

                HubCardUI card = Instantiate(genericCardPrefab, stackContentContainer);
                card.gameObject.SetActive(true);

                var (completed, total) = cat.GetProgress(account);
                card.SetupAsCategory(cat, completed, total);

                card.buttonComponent.onClick.AddListener(() => 
                {
                    currentTitle = cat.CategoryName;
                    PushScreen(cat.CategoryName, () => BuildChapterCards(cat));
                });
            }
        }

        private void BuildChapterCards(HubCategorySO category)
        {
            if (category == null || category.Chapters == null) return;
            var account = AccountManager.Instance?.PlayerAccount;
            var completedSet = new HashSet<string>(account?.CompletedNodeIDs ?? new List<string>());
            var ownedUnits = account?.OwnedUnitIDs ?? new List<string>();

            foreach (var chapter in category.Chapters)
            {
                if (chapter == null) continue;

                HubCardUI card = Instantiate(genericCardPrefab, stackContentContainer);
                card.gameObject.SetActive(true);

                int total = chapter.Nodes != null ? chapter.Nodes.Count : 0;
                int completed = 0;
                if (chapter.Nodes != null)
                {
                    foreach (var n in chapter.Nodes)
                    {
                        if (n != null && completedSet.Contains(n.NodeID)) completed++;
                    }
                }

                bool isLocked = false;
                if (CelestialCross.System.ProgressionService.Instance != null)
                    isLocked = !CelestialCross.System.ProgressionService.Instance.IsChapterUnlocked(chapter);

                card.SetupAsChapter(chapter, completed, total, isLocked);

                card.buttonComponent.onClick.AddListener(() => 
                {
                    currentTitle = chapter.ChapterTitle;
                    PushScreen(chapter.ChapterTitle, () => BuildNodeCards(chapter));
                });
            }
        }

        private void BuildNodeCards(ChapterData chapter)
        {
            if (chapter == null || chapter.Nodes == null) return;

            foreach (var node in chapter.Nodes)
            {
                if (node == null) continue;

                HubCardUI card = Instantiate(genericCardPrefab, stackContentContainer);
                card.gameObject.SetActive(true);

                bool isCompleted = false;
                int remainingAttempts = -1;
                bool isLocked = false;

                if (CelestialCross.System.ProgressionService.Instance != null)
                {
                    isCompleted = CelestialCross.System.ProgressionService.Instance.IsNodeCompleted(node.NodeID);
                    int completions = CelestialCross.System.ProgressionService.Instance.GetCompletionCount(node);
                    if (node.MaxCompletions != -1)
                        remainingAttempts = Mathf.Max(0, node.MaxCompletions - completions);

                    // A node is locked if it has a requirement and the previous node is not completed
                    if (node.Requirement != null && node.Requirement.RequiresPreviousNode && !string.IsNullOrEmpty(node.Requirement.PreviousNodeID))
                    {
                        isLocked = !CelestialCross.System.ProgressionService.Instance.IsNodeCompleted(node.Requirement.PreviousNodeID);
                    }
                }

                card.SetupAsNode(node, isCompleted, isLocked, remainingAttempts);

                card.buttonComponent.onClick.AddListener(() => 
                {
                    if (bottomSheet != null)
                        bottomSheet.Show(node);
                    else
                        CelestialCross.System.ProgressionService.Instance?.TryStartNode(node);
                });
            }
        }
        #endregion

        #region Scene Navigation
        public void GoToShopScene()
        {
            if (!string.IsNullOrEmpty(shopSceneName)) SceneManager.LoadScene(shopSceneName);
        }

        public void GoToInventoryScene()
        {
            if (!string.IsNullOrEmpty(inventorySceneName)) SceneManager.LoadScene(inventorySceneName);
        }

        public void GoToUnitScene()
        {
            if (!string.IsNullOrEmpty(unitSceneName)) SceneManager.LoadScene(unitSceneName);
        }
        #endregion
    }
}