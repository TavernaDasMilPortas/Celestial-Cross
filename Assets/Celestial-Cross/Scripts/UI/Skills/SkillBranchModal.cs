using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using CelestialCross.System;
using Celestial_Cross.Scripts.Abilities.SkillTree;
using System;

namespace CelestialCross.UI.Skills
{
    public class SkillBranchModal : MonoBehaviour
    {
        public GameObject modalRoot;
        public RectTransform tiersContainer;
        public GameObject tierPrefab;
        public GameObject optionPrefab;
        public Button closeButton;

        private string currentUnitId;
        private string currentSkillId;
        private Action onSelectionComplete;

        private void Start()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Close);
        }

        public void Open(string unitId, string skillId, SkillBranchTree tree, Action onComplete)
        {
            currentUnitId = unitId;
            currentSkillId = skillId;
            onSelectionComplete = onComplete;
            
            modalRoot.SetActive(true);
            PopulateTiers(tree);
        }

        public void Close()
        {
            modalRoot.SetActive(false);
            onSelectionComplete?.Invoke();
        }

        private void PopulateTiers(SkillBranchTree tree)
        {
            foreach (Transform child in tiersContainer)
            {
                Destroy(child.gameObject);
            }

            if (tree == null || tree.tiers == null) return;

            var account = AccountManager.Instance?.PlayerAccount;
            if (account == null) return;
            var loadout = account.GetLoadoutForUnit(currentUnitId);

            foreach (var tier in tree.tiers)
            {
                var tierGo = Instantiate(tierPrefab, tiersContainer);
                var titleText = tierGo.transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
                if (titleText != null) titleText.text = $"Tier {tier.tierIndex}";

                var optionsContainer = tierGo.transform.Find("OptionsContainer");
                if (optionsContainer != null)
                {
                    foreach (var option in tier.options)
                    {
                        var optGo = Instantiate(optionPrefab, optionsContainer);
                        var text = optGo.GetComponentInChildren<TextMeshProUGUI>();
                        if (text != null) text.text = option.branchId;

                        var btn = optGo.GetComponent<Button>();
                        if (btn != null)
                        {
                            btn.onClick.AddListener(() => {
                                // Add or update the selection
                                var selection = loadout.branchSelections.Find(s => s.skillId == currentSkillId);
                                if (selection == null)
                                {
                                    selection = new SkillBranchSelection(currentSkillId);
                                    loadout.branchSelections.Add(selection);
                                }
                                
                                // Ensure there's space for this tier
                                while (selection.selectedBranchIds.Count <= tier.tierIndex)
                                {
                                    selection.selectedBranchIds.Add(string.Empty);
                                }
                                selection.selectedBranchIds[tier.tierIndex] = option.branchId;

                                AccountManager.Instance.SaveAccount();
                                PopulateTiers(tree); // Refresh visually
                            });
                        }
                    }
                }
            }
        }
    }
}
