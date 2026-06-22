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
        [SerializeField] private TMP_Text energyRegenText;
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

            if (CelestialCross.System.EnergyService.Instance != null)
                CelestialCross.System.EnergyService.Instance.OnEnergyChanged += HandleEnergyChanged;

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
            if (CelestialCross.System.EnergyService.Instance != null)
                CelestialCross.System.EnergyService.Instance.OnEnergyChanged -= HandleEnergyChanged;
        }

        private void HandleEnergyChanged(int current, int max)
        {
            RefreshAccountUI();
        }

        public void RefreshAccountUI()
        {
            if (AccountManager.Instance == null || AccountManager.Instance.PlayerAccount == null) return;
            if (moneyText != null) moneyText.text = $"{AccountManager.Instance.PlayerAccount.Money}";
            if (energyText != null) energyText.text = $"{AccountManager.Instance.PlayerAccount.Energy}";
        }

        private void Update()
        {
            if (energyRegenText == null) return;

            if (CelestialCross.System.EnergyService.Instance != null && AccountManager.Instance != null && AccountManager.Instance.PlayerAccount != null)
            {
                int currentEnergy = CelestialCross.System.EnergyService.Instance.GetCurrentEnergy();
                int maxEnergy = CelestialCross.System.EnergyService.Instance.GetMaxEnergy();
                
                if (currentEnergy >= maxEnergy)
                {
                    energyRegenText.text = "Máximo";
                }
                else
                {
                    float timeUntilNext = CelestialCross.System.EnergyService.Instance.GetTimeUntilNextRegen();
                    if (timeUntilNext >= 0)
                    {
                        int minutes = Mathf.FloorToInt(timeUntilNext / 60f);
                        int seconds = Mathf.FloorToInt(timeUntilNext % 60f);
                        energyRegenText.text = $"{minutes:00}:{seconds:00}";
                    }
                    else
                    {
                        energyRegenText.text = "--:--";
                    }
                }
            }
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

                bool playUnlockAnim = false;
                if (!isLocked && completed < total)
                {
                    if (account != null && !account.PendingUnlockAnimations.Contains(chapter.name))
                    {
                        playUnlockAnim = true;
                        isLocked = true; // Força inicial visual de trancado para purificar
                    }
                }

                card.SetupAsChapter(chapter, completed, total, isLocked);

                // Lógica de animação de desbloqueio automático
                if (playUnlockAnim)
                {
                    card.PlayUnlockAnimation(() => 
                    {
                        account.PendingUnlockAnimations.Add(chapter.name);
                        AccountManager.Instance.SaveAccount();
                    });
                }
                else if (!isLocked && account != null && !account.PendingUnlockAnimations.Contains(chapter.name))
                {
                    account.PendingUnlockAnimations.Add(chapter.name); // Retroativo
                }

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
                bool canAffordItems = true;

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

                    canAffordItems = CelestialCross.System.ProgressionService.Instance.CanAffordItemCosts(node);
                }

                card.SetupAsNode(node, isCompleted, isLocked, remainingAttempts, canAffordItems);

                // Lógica de animação de desbloqueio automático
                var account = AccountManager.Instance?.PlayerAccount;
                bool playUnlockAnim = false;

                if (!isLocked && !isCompleted)
                {
                    if (account != null && !account.PendingUnlockAnimations.Contains(node.NodeID))
                    {
                        playUnlockAnim = true;
                        isLocked = true; // Força inicial visual de trancado para a purificação funcionar
                    }
                }

                card.SetupAsNode(node, isCompleted, isLocked, remainingAttempts, canAffordItems);

                if (playUnlockAnim)
                {
                    card.PlayUnlockAnimation(() => 
                    {
                        account.PendingUnlockAnimations.Add(node.NodeID);
                        AccountManager.Instance.SaveAccount();
                    });
                }
                else if (!isLocked && account != null && !account.PendingUnlockAnimations.Contains(node.NodeID))
                {
                    account.PendingUnlockAnimations.Add(node.NodeID); // Retroativo
                }

                card.OnNodeClicked = () => 
                {
                    if (GameFlowManager.Instance != null)
                    {
                        GameFlowManager.Instance.CurrentChapter = chapter;
                    }

                    // Se requer item e o jogador tem, podemos consumir/confirmar aqui ou no progression service
                    if (bottomSheet != null)
                        bottomSheet.Show(node);
                    else
                        CelestialCross.System.ProgressionService.Instance?.TryStartNode(node);
                };
            }
        }
        #endregion

        #region Scene Navigation
        public void GoToShopScene()
        {
            if (!string.IsNullOrEmpty(shopSceneName)) 
                System.SceneTransitionManager.LoadScene(shopSceneName);
        }

        public void GoToInventoryScene()
        {
            if (!string.IsNullOrEmpty(inventorySceneName)) 
                System.SceneTransitionManager.LoadScene(inventorySceneName);
        }

        public void GoToUnitScene()
        {
            if (!string.IsNullOrEmpty(unitSceneName)) 
                System.SceneTransitionManager.LoadScene(unitSceneName);
        }
        #endregion
    }
}