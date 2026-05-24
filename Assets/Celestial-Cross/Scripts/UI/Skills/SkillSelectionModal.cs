using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using CelestialCross.System;
using Celestial_Cross.Scripts.Abilities.SkillTree;
using System;

namespace CelestialCross.UI.Skills
{
    public class SkillSelectionModal : MonoBehaviour
    {
        public GameObject modalRoot;
        public RectTransform optionsContainer;
        public GameObject optionPrefab;
        public Button closeButton;

        private string currentUnitId;
        private SkillSlotType currentSlot;
        private Action onSelectionComplete;

        private void Start()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Close);
        }

        public void Open(string unitId, SkillSlotType slot, Action onComplete)
        {
            currentUnitId = unitId;
            currentSlot = slot;
            onSelectionComplete = onComplete;
            
            modalRoot.SetActive(true);
            PopulateOptions();
        }

        public void Close()
        {
            modalRoot.SetActive(false);
            onSelectionComplete?.Invoke();
        }

        private void PopulateOptions()
        {
            foreach (Transform child in optionsContainer)
            {
                Destroy(child.gameObject);
            }

            // In a real scenario, we fetch the unit's SkillTreeConfigSO from the UnitCatalog.
            // For now, we will create placeholders to let the user select.
            // When UnitCatalog is integrated, we read unitCatalog.GetUnitData(currentUnitId).skillTreeConfig

            var account = AccountManager.Instance?.PlayerAccount;
            if (account == null) return;
            var loadout = account.GetLoadoutForUnit(currentUnitId);
            
            // Temporary placeholder list for demonstration
            List<string> availableSkillIds = new List<string>() { "Skill_A", "Skill_B", "Skill_C" };

            foreach (var skillId in availableSkillIds)
            {
                var go = Instantiate(optionPrefab, optionsContainer);
                var text = go.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null) text.text = skillId;

                var btn = go.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.AddListener(() => {
                        if (currentSlot == SkillSlotType.Slot1) loadout.Slot1SkillId = skillId;
                        else if (currentSlot == SkillSlotType.Slot2) loadout.Slot2SkillId = skillId;

                        AccountManager.Instance.SaveAccount();
                        Close();
                    });
                }
            }
        }
    }
}
