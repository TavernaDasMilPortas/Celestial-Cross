using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using CelestialCross.System;
using Celestial_Cross.Scripts.Abilities.SkillTree;
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

            var unitData = account.GetOwnedUnitRuntimeData(currentUnitId);
            var loadout = account.GetLoadoutForUnit(currentUnitId);

            // We need to fetch the SkillTreeConfigSO from the unit's base data.
            // Since we don't have direct access here easily without UnitCatalog, we assume UnitData holds it.
            // But we need the UnitCatalog from somewhere. We can just emit an event or require a reference.
            
            // For now, let's just show "Equipped" or "Not Equipped" using loadout.
            UpdateSlotUI(SkillSlotType.Basic, basicSkillText, "Ataque Básico");
            UpdateSlotUI(SkillSlotType.Movement, movementSkillText, "Movimentação");
            
            string slot1Name = string.IsNullOrEmpty(loadout?.Slot1SkillId) ? "Vazio" : loadout.Slot1SkillId;
            UpdateSlotUI(SkillSlotType.Slot1, slot1SkillText, slot1Name);
            
            string slot2Name = string.IsNullOrEmpty(loadout?.Slot2SkillId) ? "Vazio" : loadout.Slot2SkillId;
            UpdateSlotUI(SkillSlotType.Slot2, slot2SkillText, slot2Name);
        }

        private void UpdateSlotUI(SkillSlotType type, TextMeshProUGUI textComp, string skillName)
        {
            if (textComp != null)
            {
                textComp.text = $"<b>{type}</b>\n<color=#ffb>{skillName}</color>";
            }
        }

        private void OnSlotClicked(SkillSlotType slot)
        {
            if (string.IsNullOrEmpty(currentUnitId)) return;
            var account = AccountManager.Instance?.PlayerAccount;
            if (account == null) return;

            // Open Selection Modal for Slot1/Slot2, or open Branch Modal directly for Basic/Movement
            if (slot == SkillSlotType.Slot1 || slot == SkillSlotType.Slot2)
            {
                if (selectionModal != null)
                {
                    selectionModal.Open(currentUnitId, slot, () => {
                        // After selection, we might open branch modal
                        Refresh();
                    });
                }
            }
            else
            {
                // Basic / Movement can only have branches, they are fixed
                if (branchModal != null)
                {
                    branchModal.Open(currentUnitId, slot.ToString(), null, () => {
                        Refresh();
                    });
                }
            }
        }
    }
}
