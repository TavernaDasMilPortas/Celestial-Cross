using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace CelestialCross.Progression
{
    public class InviteUnlockModal : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject modalPanel;
        [SerializeField] private Transform optionsContainer;
        [SerializeField] private Button optionButtonPrefab;
        [SerializeField] private Button closeButton;

        private StoryNode _currentNode;

        private void Awake()
        {
            if (modalPanel != null) modalPanel.SetActive(false);
            if (closeButton != null) closeButton.onClick.AddListener(() => modalPanel.SetActive(false));
        }

        public void Show(StoryNode node)
        {
            if (node == null || node.Requirement == null || node.Requirement.InviteCostOptions.Count == 0) return;
            
            _currentNode = node;
            
            foreach (Transform child in optionsContainer) Destroy(child.gameObject);

            var account = AccountManager.Instance?.PlayerAccount;

            foreach (var cost in node.Requirement.InviteCostOptions)
            {
                Button btn = Instantiate(optionButtonPrefab, optionsContainer);
                TMP_Text label = btn.GetComponentInChildren<TMP_Text>();
                if (label != null) label.text = cost.DisplayName;

                int currentAmount = account != null ? account.GetItemCount(cost.InviteItemID) : 0;
                bool canAfford = currentAmount >= cost.Amount;

                btn.interactable = canAfford;

                btn.onClick.AddListener(() => {
                    if (account.RemoveItem(cost.InviteItemID, cost.Amount))
                    {
                        AccountManager.Instance.SaveAccount();
                        if (!account.UnlockedDiaryNodeIDs.Contains(node.NodeID))
                        {
                            account.UnlockedDiaryNodeIDs.Add(node.NodeID);
                            AccountManager.Instance.SaveAccount();
                        }
                        
                        modalPanel.SetActive(false);
                        
                        CelestialCross.System.ProgressionService.Instance.TryStartNode(node);
                    }
                });
            }

            modalPanel.SetActive(true);
        }
    }
}
