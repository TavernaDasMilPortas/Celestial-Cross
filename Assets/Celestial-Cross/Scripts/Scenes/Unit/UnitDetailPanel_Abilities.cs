using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Data;
using CelestialCross.Data.Pets;
using Celestial_Cross.Scripts.Abilities;
using Celestial_Cross.Scripts.Abilities.Graph;
using Celestial_Cross.Scripts.Abilities.SkillTree;
using CelestialCross.UI.Skills;

namespace CelestialCross.Scenes.Unit
{
    public class UnitDetailPanel_Abilities : MonoBehaviour
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

        public void Refresh(UnitData unitData, RuntimeUnitData runtimeData, PetCatalog petCatalog)
        {
            if (unitData == null || runtimeData == null) return;
            currentUnitId = runtimeData.UnitID;

            var account = AccountManager.Instance?.PlayerAccount;
            if (account == null) return;

            var loadout = account.GetLoadoutForUnit(currentUnitId);

            SkillTreeConfigSO treeConfig = unitData.skillTreeConfig;

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

            SkillTreeConfigSO treeConfig = null;
            if (unitCatalog == null) unitCatalog = FindObjectOfType<UnitCatalog>();
            if (unitCatalog != null)
            {
                var unitData = unitCatalog.GetUnitData(currentUnitId);
                if (unitData != null) treeConfig = unitData.skillTreeConfig;
            }

            if (slot == SkillSlotType.Slot1 || slot == SkillSlotType.Slot2)
            {
                var loadout = account.GetLoadoutForUnit(currentUnitId);
                string skillId = (slot == SkillSlotType.Slot1) ? loadout?.Slot1SkillId : loadout?.Slot2SkillId;

                if (string.IsNullOrEmpty(skillId))
                {
                    if (selectionModal != null)
                    {
                        selectionModal.Open(currentUnitId, slot, () => {
                            if (unitCatalog != null)
                                Refresh(unitCatalog.GetUnitData(currentUnitId), account.GetOwnedUnitRuntimeData(currentUnitId), null);
                        }, branchModal);
                    }
                }
                else
                {
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
                            if (unitCatalog != null)
                                Refresh(unitCatalog.GetUnitData(currentUnitId), account.GetOwnedUnitRuntimeData(currentUnitId), null);
                        }, () => {
                            if (selectionModal != null)
                            {
                                selectionModal.Open(currentUnitId, slot, () => {
                                    if (unitCatalog != null)
                                        Refresh(unitCatalog.GetUnitData(currentUnitId), account.GetOwnedUnitRuntimeData(currentUnitId), null);
                                }, branchModal);
                            }
                        });
                    }
                    else
                    {
                        if (selectionModal != null)
                        {
                            selectionModal.Open(currentUnitId, slot, () => {
                                if (unitCatalog != null)
                                    Refresh(unitCatalog.GetUnitData(currentUnitId), account.GetOwnedUnitRuntimeData(currentUnitId), null);
                            }, branchModal);
                        }
                    }
                }
            }
            else
            {
                AbilityGraphSO graph = null;
                if (treeConfig != null)
                {
                    graph = (slot == SkillSlotType.Basic) ? treeConfig.basicAttack : treeConfig.movementSkill;
                }

                if (branchModal != null && graph != null)
                {
                    branchModal.Open(currentUnitId, graph.name, graph, slot, () => {
                        if (unitCatalog != null)
                            Refresh(unitCatalog.GetUnitData(currentUnitId), account.GetOwnedUnitRuntimeData(currentUnitId), null);
                    });
                }
            }
        }
    }
}
