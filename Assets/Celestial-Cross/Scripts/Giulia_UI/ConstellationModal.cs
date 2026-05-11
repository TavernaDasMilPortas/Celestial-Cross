using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Data;
using CelestialCross.System;
using System.Collections.Generic;

namespace CelestialCross.Giulia_UI
{
    public class ConstellationModal : MonoBehaviour
    {
        [Header("UI Refs")]
        public GameObject root;
        public TextMeshProUGUI unitNameText;
        public TextMeshProUGUI insigniaCountText;
        public Button upgradeButton;
        public Button closeButton;

        [Header("Nodes & Visuals")]
        public RectTransform nodesContainer;
        public Image[] starIcons = new Image[6];
        public Image[] connectionLines = new Image[5]; // Conectam 0-1, 1-2, 2-3, 3-4, 4-5

        [Header("Info Panel")]
        public GameObject infoPanel;
        public TextMeshProUGUI skillNameText;
        public TextMeshProUGUI skillDescText;

        private RuntimeUnitData currentUnit;
        private UnitData currentSO;

        void Awake()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (upgradeButton != null) upgradeButton.onClick.AddListener(OnUpgradeClicked);
            if (infoPanel != null) infoPanel.SetActive(false);
            if (root != null) root.SetActive(false);
        }

        public void Open(string unitID)
        {
            Debug.Log($"[ConstellationModal] Abrindo modal para: {unitID}");
            var acc = AccountManager.Instance?.PlayerAccount;
            if (acc == null) { Debug.LogError("[ConstellationModal] AccountManager.PlayerAccount está nulo!"); return; }

            currentUnit = acc.GetOwnedUnitRuntimeData(unitID);
            
            // Busca o catálogo de forma mais robusta
            var inv = GetComponentInParent<InventoryUI>();
            if (inv == null) inv = Object.FindObjectOfType<InventoryUI>();

            if (inv != null && inv.unitCatalog != null)
                currentSO = inv.unitCatalog.GetUnitData(unitID);
            else
                Debug.LogError("[ConstellationModal] InventoryUI ou UnitCatalog não encontrado na cena!");

            if (currentUnit == null) 
            { 
                Debug.LogError($"[ConstellationModal] Unidade {unitID} não encontrada nos dados da conta!"); 
                return; 
            }
            if (currentSO == null) 
            { 
                Debug.LogError($"[ConstellationModal] UnitData para {unitID} não encontrado no catálogo! Verifique se a unidade está registrada no SO do Catálogo."); 
                return; 
            }

            unitNameText.text = currentSO.displayName;
            
            UpdateVisualLayout();
            RefreshUI();
            
            if (root != null) root.SetActive(true);
        }

        private void UpdateVisualLayout()
        {
            if (currentSO.constellationConfig == null) return;

            var stars = currentSO.constellationConfig.stars;
            if (stars == null || stars.Count < 6) return;

            // Posicionar Estrelas
            for (int i = 0; i < starIcons.Length && i < stars.Count; i++)
            {
                starIcons[i].rectTransform.anchoredPosition = stars[i].position;
                
                int idx = i;
                var btn = starIcons[i].GetComponent<Button>();
                if (btn == null) btn = starIcons[i].gameObject.AddComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => ShowSkillInfo(idx));
            }

            // Posicionar e Rotacionar Linhas (usando connectionIndices se houver, ou fallback sequencial)
            int[] indices = currentSO.constellationConfig.connectionIndices;
            bool useCustomConnections = indices != null && indices.Length > 1 && indices.Length % 2 == 0;

            for (int i = 0; i < connectionLines.Length; i++)
            {
                int idx1, idx2;
                if (useCustomConnections && (i * 2 + 1) < indices.Length)
                {
                    idx1 = indices[i * 2];
                    idx2 = indices[i * 2 + 1];
                }
                else
                {
                    // Fallback sequencial
                    idx1 = i;
                    idx2 = i + 1;
                }

                if (idx1 < stars.Count && idx2 < stars.Count)
                {
                    Vector2 p1 = stars[idx1].position;
                    Vector2 p2 = stars[idx2].position;
                    
                    RectTransform lineRT = connectionLines[i].rectTransform;
                    lineRT.anchoredPosition = (p1 + p2) / 2f;
                    float distance = Vector2.Distance(p1, p2);
                    lineRT.sizeDelta = new Vector2(distance, 5f);
                    float angle = Mathf.Atan2(p2.y - p1.y, p2.x - p1.x) * Mathf.Rad2Deg;
                    lineRT.localRotation = Quaternion.Euler(0, 0, angle);
                }
            }
        }

        public void RefreshUI()
        {
            int level = currentUnit.ConstellationLevel;
            
            for (int i = 0; i < starIcons.Length; i++)
            {
                starIcons[i].color = (i < level) ? Color.yellow : new Color(0.4f, 0.4f, 0.4f, 0.8f);
            }

            for (int i = 0; i < connectionLines.Length; i++)
            {
                // A linha i acende se a estrela de destino (i+1) estiver ativa
                connectionLines[i].color = (i + 1 < level) ? new Color(1f, 0.9f, 0f, 0.8f) : new Color(0.2f, 0.2f, 0.2f, 0.4f);
            }

            string insigniaID = ConstellationService.GetInsigniaItemID(currentUnit.UnitID);
            int count = AccountManager.Instance.PlayerAccount.GetItemCount(insigniaID);
            insigniaCountText.text = $"Insígnias: {count}";
            
            upgradeButton.interactable = (count > 0 && level < 6);
        }

        private void ShowSkillInfo(int index)
        {
            if (currentSO.constellationConfig != null && currentSO.constellationConfig.stars.Count > index)
            {
                var star = currentSO.constellationConfig.stars[index];
                skillNameText.text = string.IsNullOrEmpty(star.starName) ? $"Estrela {index + 1}" : star.starName;
                
                if (!string.IsNullOrEmpty(star.customDescription))
                {
                    skillDescText.text = star.customDescription;
                }
                else if (star.passiveGraph != null)
                {
                    skillNameText.text = star.passiveGraph.abilityName; // Opcional: usar o nome do grafo
                    skillDescText.text = star.passiveGraph.abilityDescription;
                }
                else
                {
                    skillDescText.text = "Nenhuma habilidade configurada para este nível.";
                }
                infoPanel.SetActive(true);
            }
        }

        private void OnUpgradeClicked()
        {
            if (ConstellationService.TryUpgradeConstellation(currentUnit))
            {
                AccountManager.Instance.SaveAccount();
                RefreshUI();
            }
        }

        public void Close()
        {
            if (root != null) root.SetActive(false);
            if (infoPanel != null) infoPanel.SetActive(false);
        }
    }
}
