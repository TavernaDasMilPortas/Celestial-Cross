using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using CelestialCross.System;
using Celestial_Cross.Scripts.Abilities.SkillTree;
using Celestial_Cross.Scripts.Abilities.Graph;
using Celestial_Cross.Scripts.Abilities.Graph.Runtime;
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

        [Header("Slot Actions")]
        public Button changeButton;
        public Button unequipButton;

        [Header("Skill Info")]
        public TextMeshProUGUI skillNameText;
        public TextMeshProUGUI skillDescText;

        private string currentUnitId;
        private string currentSkillId;
        private AbilityGraphSO currentGraph;
        private SkillSlotType currentSlot;
        private Action onSelectionComplete;
        private Action onChangeRequested;

        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Close);
            if (changeButton != null)
                changeButton.onClick.AddListener(OnChangeClicked);
            if (unequipButton != null)
                unequipButton.onClick.AddListener(OnUnequipClicked);
        }

        public void Open(string unitId, string skillId, AbilityGraphSO graph, SkillSlotType slot, Action onComplete, Action onChangeRequest = null)
        {
            currentUnitId = unitId;
            currentSkillId = skillId;
            currentGraph = graph;
            currentSlot = slot;
            onSelectionComplete = onComplete;
            onChangeRequested = onChangeRequest;
            
            modalRoot.SetActive(true);
            PopulateTiers();
            UpdateSkillInfo();
            UpdateActionButtons();
        }

        public void Close()
        {
            modalRoot.SetActive(false);
            onSelectionComplete?.Invoke();
        }

        private void UpdateSkillInfo()
        {
            if (currentGraph != null)
            {
                if (skillNameText != null)
                    skillNameText.text = string.IsNullOrEmpty(currentGraph.abilityName) ? currentGraph.name : currentGraph.abilityName;
                if (skillDescText != null)
                    skillDescText.text = currentGraph.abilityDescription;
            }
        }

        private void UpdateActionButtons()
        {
            bool isSlotAbility = (currentSlot == SkillSlotType.Slot1 || currentSlot == SkillSlotType.Slot2);
            if (changeButton != null) changeButton.gameObject.SetActive(isSlotAbility);
            if (unequipButton != null) unequipButton.gameObject.SetActive(isSlotAbility);
        }

        private void OnChangeClicked()
        {
            modalRoot.SetActive(false);
            onChangeRequested?.Invoke();
        }

        private void OnUnequipClicked()
        {
            var account = AccountManager.Instance?.PlayerAccount;
            if (account == null) return;
            var loadout = account.GetLoadoutForUnit(currentUnitId);
            if (loadout == null) return;

            if (currentSlot == SkillSlotType.Slot1) loadout.Slot1SkillId = "";
            else if (currentSlot == SkillSlotType.Slot2) loadout.Slot2SkillId = "";

            AccountManager.Instance.SaveAccount();
            Close();
        }

        private void PopulateTiers()
        {
            foreach (Transform child in tiersContainer)
            {
                Destroy(child.gameObject);
            }

            if (currentGraph == null) return;

            var account = AccountManager.Instance?.PlayerAccount;
            if (account == null) return;
            var loadout = account.GetLoadoutForUnit(currentUnitId);

            var branchTiers = currentGraph.GetRamificationTiers();

            if (branchTiers == null || branchTiers.Count == 0)
            {
                var infoGo = new GameObject("NoBranchesInfo", typeof(RectTransform), typeof(TextMeshProUGUI));
                infoGo.transform.SetParent(tiersContainer, false);
                var infoText = infoGo.GetComponent<TextMeshProUGUI>();
                infoText.text = "Esta habilidade não possui ramificações de talentos.";
                infoText.alignment = TextAlignmentOptions.Center;
                infoText.color = new Color(0.7f, 0.7f, 0.8f, 1f);
                infoText.fontSize = 20f;
                return;
            }

            foreach (var tier in branchTiers)
            {
                var tierGo = Instantiate(tierPrefab, tiersContainer);
                tierGo.SetActive(true);
                var titleText = tierGo.transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
                if (titleText != null) titleText.text = $"Tier {tier.tierIndex}";

                var optionsContainer = tierGo.transform.Find("OptionsContainer");
                if (optionsContainer != null)
                {
                    foreach (var option in tier.options)
                    {
                        var optGo = Instantiate(optionPrefab, optionsContainer);
                        optGo.SetActive(true);
                        
                        var text = optGo.GetComponentInChildren<TextMeshProUGUI>();
                        if (text != null)
                        {
                            text.text = option.displayName;
                        }

                        // Se quiser atualizar a descrição ou ícone no futuro, adicione a lógica aqui.
                        // ex: var descText = optGo.transform.Find("DescText")?.GetComponent<TextMeshProUGUI>();

                        var btn = optGo.GetComponent<Button>();
                        if (btn != null)
                        {
                            var capturedOption = option;
                            var capturedTier = tier;

                            // Realce visual da seleção
                            var selection = loadout.branchSelections.Find(s => s.skillId == currentSkillId);
                            bool isSelected = false;
                            if (selection != null && selection.selectedBranchIds.Count > capturedTier.tierIndex)
                            {
                                isSelected = selection.selectedBranchIds[capturedTier.tierIndex] == capturedOption.flowId;
                            }

                            var img = optGo.GetComponent<Image>();
                            if (img != null)
                            {
                                img.color = isSelected ? new Color(0.12f, 0.58f, 0.33f, 1f) : new Color(0.2f, 0.2f, 0.3f, 1f);
                            }

                            if (text != null)
                            {
                                text.color = isSelected ? Color.white : new Color(0.7f, 0.7f, 0.8f, 1f);
                            }

                            btn.onClick.AddListener(() => {
                                var sel = loadout.branchSelections.Find(s => s.skillId == currentSkillId);
                                if (sel == null)
                                {
                                    sel = new SkillBranchSelection(currentSkillId);
                                    loadout.branchSelections.Add(sel);
                                }
                                
                                while (sel.selectedBranchIds.Count <= capturedTier.tierIndex)
                                {
                                    sel.selectedBranchIds.Add(string.Empty);
                                }
                                sel.selectedBranchIds[capturedTier.tierIndex] = capturedOption.flowId;

                                AccountManager.Instance.SaveAccount();
                                PopulateTiers(); // Atualiza visualmente
                            });
                        }
                    }
                }
            }
        }
    }
}
