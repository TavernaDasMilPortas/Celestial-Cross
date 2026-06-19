using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using CelestialCross.System;
using Celestial_Cross.Scripts.Abilities.SkillTree;
using Celestial_Cross.Scripts.Abilities.Graph;
using CelestialCross.Data;

namespace CelestialCross.UI.Skills
{
    public class SkillTabUI : MonoBehaviour
    {
        [Header("Slot Buttons")]
        public Button basicSkillButton;
        public TextMeshProUGUI basicSkillText;
        public Button movementSkillButton;
        public TextMeshProUGUI movementSkillText;
        public Button slot1SkillButton;
        public TextMeshProUGUI slot1SkillText;
        public Button slot2SkillButton;
        public TextMeshProUGUI slot2SkillText;

        [Header("Modals")]
        public SkillSelectionModal selectionModal;
        public SkillBranchModal branchModal;

        [Header("Data")]
        public UnitCatalog unitCatalog;

        private string currentUnitId;

        private void Start()
        {
            if (basicSkillButton != null) basicSkillButton.onClick.AddListener(() => OnSlotClicked(SkillSlotType.Basic));
            if (movementSkillButton != null) movementSkillButton.onClick.AddListener(() => OnSlotClicked(SkillSlotType.Movement));
            if (slot1SkillButton != null) slot1SkillButton.onClick.AddListener(() => OnSlotClicked(SkillSlotType.Slot1));
            if (slot2SkillButton != null) slot2SkillButton.onClick.AddListener(() => OnSlotClicked(SkillSlotType.Slot2));
        }

        public void Setup(string unitId)
        {
            currentUnitId = unitId;
            Refresh();
        }

        public void Refresh()
        {
            if (string.IsNullOrEmpty(currentUnitId)) return;

            var account = AccountManager.Instance?.PlayerAccount;
            if (account == null) return;

            var loadout = account.GetLoadoutForUnit(currentUnitId);

            // Buscar o SkillTreeConfigSO via UnitData
            SkillTreeConfigSO treeConfig = GetTreeConfig();

            if (loadout != null && treeConfig != null && !loadout.hasInitializedDefaultSkills)
            {
                loadout.InitializeDefaults(treeConfig);
                AccountManager.Instance?.SaveAccount();
            }

            AbilityGraphSO basicGraph = treeConfig != null ? treeConfig.basicAttack : null;
            AbilityGraphSO moveGraph = treeConfig != null ? treeConfig.movementSkill : null;

            AbilityGraphSO slot1Graph = null;
            if (loadout != null && !string.IsNullOrEmpty(loadout.Slot1SkillId) && treeConfig != null)
            {
                #pragma warning disable 612, 618
                var pool = (treeConfig.slot1Skills != null && treeConfig.slot1Skills.Count > 0) ? treeConfig.slot1Skills : treeConfig.combatSkills;
                if (pool != null) slot1Graph = pool.Find(g => g != null && g.name == loadout.Slot1SkillId);
                #pragma warning restore 612, 618
            }

            AbilityGraphSO slot2Graph = null;
            if (loadout != null && !string.IsNullOrEmpty(loadout.Slot2SkillId) && treeConfig != null)
            {
                #pragma warning disable 612, 618
                var pool = (treeConfig.slot2Skills != null && treeConfig.slot2Skills.Count > 0) ? treeConfig.slot2Skills : treeConfig.combatSkills;
                if (pool != null) slot2Graph = pool.Find(g => g != null && g.name == loadout.Slot2SkillId);
                #pragma warning restore 612, 618
            }

            UpdateSlotUI(basicSkillButton, basicSkillText, "Ataque Básico", basicGraph);
            UpdateSlotUI(movementSkillButton, movementSkillText, "Movimentação", moveGraph);
            UpdateSlotUI(slot1SkillButton, slot1SkillText, "Slot 1", slot1Graph);
            UpdateSlotUI(slot2SkillButton, slot2SkillText, "Slot 2", slot2Graph);
        }

        private string ResolveSlotSkillName(string skillId, SkillTreeConfigSO treeConfig)
        {
            if (string.IsNullOrEmpty(skillId)) return "Vazio";
            if (treeConfig == null) return skillId;

            #pragma warning disable 612, 618
            var list1 = treeConfig.slot1Skills;
            if (list1 != null)
            {
                foreach (var graph in list1)
                    if (graph != null && graph.name == skillId)
                        return string.IsNullOrEmpty(graph.abilityName) ? graph.name : graph.abilityName;
            }

            var list2 = treeConfig.slot2Skills;
            if (list2 != null)
            {
                foreach (var graph in list2)
                    if (graph != null && graph.name == skillId)
                        return string.IsNullOrEmpty(graph.abilityName) ? graph.name : graph.abilityName;
            }

            var legacy = treeConfig.combatSkills;
            if (legacy != null)
            {
                foreach (var graph in legacy)
                    if (graph != null && graph.name == skillId)
                        return string.IsNullOrEmpty(graph.abilityName) ? graph.name : graph.abilityName;
            }
            #pragma warning restore 612, 618

            return skillId;
        }

        private void UpdateSlotUI(Button button, TextMeshProUGUI textComp, string slotLabel, AbilityGraphSO graph)
        {
            if (textComp != null)
            {
                textComp.text = $"<b>{slotLabel}</b>";
            }

            if (button != null)
            {
                var img = button.GetComponent<Image>();
                if (img != null)
                {
                    if (graph != null && graph.abilityIcon != null)
                    {
                        img.sprite = graph.abilityIcon;
                        img.color = Color.white;
                    }
                    else
                    {
                        img.sprite = null;
                        img.color = new Color(0.2f, 0.2f, 0.3f, 1f);
                    }
                }
            }
        }

        private void OnSlotClicked(SkillSlotType slot)
        {
            if (string.IsNullOrEmpty(currentUnitId)) return;
            var account = AccountManager.Instance?.PlayerAccount;
            if (account == null) return;

            SkillTreeConfigSO treeConfig = GetTreeConfig();

            if (slot == SkillSlotType.Slot1 || slot == SkillSlotType.Slot2)
            {
                var loadout = account.GetLoadoutForUnit(currentUnitId);
                string skillId = (slot == SkillSlotType.Slot1) ? loadout?.Slot1SkillId : loadout?.Slot2SkillId;

                if (string.IsNullOrEmpty(skillId))
                {
                    // Slot is empty, open selection modal directly
                    if (selectionModal != null)
                    {
                        selectionModal.Open(currentUnitId, slot, () => {
                            Refresh();
                        });
                    }
                }
                else
                {
                    // Slot has a skill, open branch modal
                    AbilityGraphSO graph = null;
                    if (treeConfig != null)
                    {
                        #pragma warning disable 612, 618
                        var pool = (slot == SkillSlotType.Slot1)
                            ? (treeConfig.slot1Skills != null && treeConfig.slot1Skills.Count > 0 ? treeConfig.slot1Skills : treeConfig.combatSkills)
                            : (treeConfig.slot2Skills != null && treeConfig.slot2Skills.Count > 0 ? treeConfig.slot2Skills : treeConfig.combatSkills);
                        
                        if (pool != null)
                        {
                            graph = pool.Find(g => g != null && g.name == skillId);
                        }
                        #pragma warning restore 612, 618
                    }

                    if (graph != null && branchModal != null)
                    {
                        branchModal.Open(currentUnitId, graph.name, graph, slot, () => {
                            Refresh();
                        }, () => {
                            // On Change clicked in branch modal: open selection modal
                            if (selectionModal != null)
                            {
                                selectionModal.Open(currentUnitId, slot, () => {
                                    Refresh();
                                });
                            }
                        });
                    }
                    else
                    {
                        // Fallback if graph is not found
                        if (selectionModal != null)
                        {
                            selectionModal.Open(currentUnitId, slot, () => {
                                Refresh();
                            });
                        }
                    }
                }
            }
            else
            {
                // Basic / Movement can only have branches, they are fixed
                AbilityGraphSO graph = null;
                if (treeConfig != null)
                {
                    graph = (slot == SkillSlotType.Basic) ? treeConfig.basicAttack : treeConfig.movementSkill;
                }

                if (branchModal != null && graph != null)
                {
                    branchModal.Open(currentUnitId, graph.name, graph, slot, () => {
                        Refresh();
                    });
                }
            }
        }

        private SkillTreeConfigSO GetTreeConfig()
        {
            if (unitCatalog == null)
            {
                if (CelestialCross.System.GlobalCatalogs.Instance != null)
                {
                    unitCatalog = CelestialCross.System.GlobalCatalogs.Instance.unitCatalog;
                }
                
                if (unitCatalog == null)
                    unitCatalog = FindObjectOfType<UnitCatalog>();
            }
            if (unitCatalog == null) return null;
            var unitData = unitCatalog.GetUnitData(currentUnitId);
            return unitData?.skillTreeConfig;
        }
    }
}
