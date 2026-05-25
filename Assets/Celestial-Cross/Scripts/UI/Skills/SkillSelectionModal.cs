using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using CelestialCross.System;
using Celestial_Cross.Scripts.Abilities.SkillTree;
using Celestial_Cross.Scripts.Abilities.Graph;
using System;

namespace CelestialCross.UI.Skills
{
    public class SkillSelectionModal : MonoBehaviour
    {
        public GameObject modalRoot;
        public RectTransform optionsContainer;
        public GameObject optionPrefab;
        public Button closeButton;

        [Header("Data")]
        public UnitCatalog unitCatalog;

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

            var account = AccountManager.Instance?.PlayerAccount;
            if (account == null) return;
            var loadout = account.GetLoadoutForUnit(currentUnitId);

            // Buscar o SkillTreeConfigSO via UnitData
            if (unitCatalog == null)
            {
                unitCatalog = FindObjectOfType<UnitCatalog>();
                #if UNITY_EDITOR
                if (unitCatalog == null)
                {
                    string[] guids = UnityEditor.AssetDatabase.FindAssets("t:UnitCatalog");
                    if (guids.Length > 0)
                    {
                        unitCatalog = UnityEditor.AssetDatabase.LoadAssetAtPath<UnitCatalog>(UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]));
                    }
                }
                #endif
            }

            SkillTreeConfigSO treeConfig = null;
            if (unitCatalog != null)
            {
                var unitData = unitCatalog.GetUnitData(currentUnitId);
                if (unitData != null)
                    treeConfig = unitData.skillTreeConfig;
            }

            if (treeConfig == null)
            {
                Debug.LogWarning($"[SkillSelectionModal] SkillTreeConfig não encontrado para unidade '{currentUnitId}'.");
                return;
            }

            // Selecionar o pool adequado com base no slot
            List<AbilityGraphSO> pool = new List<AbilityGraphSO>();
            #pragma warning disable 612, 618
            if (currentSlot == SkillSlotType.Slot1)
            {
                pool = (treeConfig.slot1Skills != null && treeConfig.slot1Skills.Count > 0)
                    ? treeConfig.slot1Skills
                    : treeConfig.combatSkills;
            }
            else if (currentSlot == SkillSlotType.Slot2)
            {
                pool = (treeConfig.slot2Skills != null && treeConfig.slot2Skills.Count > 0)
                    ? treeConfig.slot2Skills
                    : treeConfig.combatSkills;
            }
            #pragma warning restore 612, 618

            if (pool == null) return;

            foreach (var graph in pool)
            {
                if (graph == null) continue;

                // Evitar equipar a mesma habilidade nos dois slots
                string otherSkillId = (currentSlot == SkillSlotType.Slot1) ? loadout.Slot2SkillId : loadout.Slot1SkillId;
                if (graph.name == otherSkillId) continue;

                var go = Instantiate(optionPrefab, optionsContainer);
                go.SetActive(true); // Garante que a opção fique ativa e visível
                
                var img = go.GetComponent<Image>();
                if (img != null && graph.abilityIcon != null)
                {
                    img.sprite = graph.abilityIcon;
                    img.color = Color.white;
                }

                var text = go.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = ""; // Nome não é necessário na tree / seletor
                }

                var btn = go.GetComponent<Button>();
                if (btn != null)
                {
                    var capturedGraph = graph;
                    btn.onClick.AddListener(() => {
                        string skillId = capturedGraph.name; // Usamos o nome do asset como ID
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
