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
        public Button equipButton;
        public SkillBranchModal branchModal;

        [Header("Data")]
        public UnitCatalog unitCatalog;

        private string currentUnitId;
        private SkillSlotType currentSlot;
        private Action onSelectionComplete;
        private AbilityGraphSO selectedGraph;

        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Close);
            if (equipButton != null)
                equipButton.onClick.AddListener(OnEquipClicked);
        }

        public void Open(string unitId, SkillSlotType slot, Action onComplete, SkillBranchModal bModal = null)
        {
            currentUnitId = unitId;
            currentSlot = slot;
            onSelectionComplete = onComplete;
            if (bModal != null) branchModal = bModal;
            selectedGraph = null;
            
            modalRoot.SetActive(true);
            PopulateOptions();
        }

        public void Close()
        {
            modalRoot.SetActive(false);
            onSelectionComplete?.Invoke();
        }

        private void OnEquipClicked()
        {
            if (selectedGraph != null)
            {
                var account = AccountManager.Instance?.PlayerAccount;
                if (account != null)
                {
                    var loadout = account.GetLoadoutForUnit(currentUnitId);
                    if (currentSlot == SkillSlotType.Slot1) loadout.Slot1SkillId = selectedGraph.name;
                    else if (currentSlot == SkillSlotType.Slot2) loadout.Slot2SkillId = selectedGraph.name;
                    AccountManager.Instance.SaveAccount();
                }
            }
            Close();
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
                if (img != null)
                {
                    if (graph.abilityIcon != null)
                    {
                        img.sprite = graph.abilityIcon;
                        img.color = Color.white;
                    }
                    else
                    {
                        img.sprite = null;
                    }
                    
                    // Highlight selected
                    if (selectedGraph == graph)
                    {
                        img.color = new Color(0.5f, 0.8f, 0.5f, 1f); // Greenish highlight
                    }
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
                        selectedGraph = capturedGraph;
                        PopulateOptions(); // refresh visuals
                        
                        var tiers = capturedGraph.GetRamificationTiers();
                        if (tiers != null && tiers.Count > 0 && branchModal != null)
                        {
                            modalRoot.SetActive(false);
                            branchModal.Open(currentUnitId, capturedGraph.name, capturedGraph, currentSlot, () => {
                                modalRoot.SetActive(true);
                                PopulateOptions();
                            });
                        }
                    });
                }
            }
        }
    }
}
