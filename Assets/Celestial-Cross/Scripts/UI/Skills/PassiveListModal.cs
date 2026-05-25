using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace CelestialCross.UI.Skills
{
    public class PassiveListModal : MonoBehaviour
    {
        public GameObject modalRoot;
        
        [Header("Seção 1: Condições Temporárias")]
        public RectTransform conditionsGrid;
        public GameObject conditionIconPrefab;
        public PassiveDetailModal detailModal;
        
        [Header("Seção 2: Efeitos Ativos (Status)")]
        public RectTransform positiveModifiersContainer;
        public RectTransform negativeModifiersContainer;
        public GameObject modifierItemPrefab;
        
        [Header("Seção 3: Todas as Passivas Nativas")]
        public RectTransform allPassivesContainer;
        public GameObject passiveItemPrefab;
        
        public Button closeButton;

        private void Start()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Close);
        }

        public void Open(global::Unit unit)
        {
            if (unit == null) return;
            if (modalRoot != null) modalRoot.SetActive(true);
            Populate(unit);
        }

        public void Close()
        {
            if (modalRoot != null) modalRoot.SetActive(false);
            if (detailModal != null) detailModal.Close();
        }

        private void Populate(global::Unit unit)
        {
            // Limpar containers de forma segura
            ClearContainer(conditionsGrid);
            ClearContainer(positiveModifiersContainer);
            ClearContainer(negativeModifiersContainer);
            ClearContainer(allPassivesContainer);

            if (unit == null || unit.PassiveManager == null) return;

            // --- SEÇÃO 1: Condições Temporárias (Grid de Ícones) ---
            var allConditions = unit.PassiveManager.GetActiveConditionsInfo();
            if (allConditions != null && conditionsGrid != null && conditionIconPrefab != null)
            {
                foreach (var c in allConditions)
                {
                    if (c.isPersistent) continue; // Pular permanentes/passivas estáticas nesta seção

                    var go = Instantiate(conditionIconPrefab, conditionsGrid);
                    go.SetActive(true);

                    // Configurar imagem do botão (ícone da condição)
                    var btn = go.GetComponent<Button>();
                    var img = go.GetComponent<Image>();
                    if (img != null)
                    {
                        img.sprite = c.icon;
                    }

                    // Exibir turnos restantes no texto (ex: "3t")
                    var turnTxt = go.GetComponentInChildren<TextMeshProUGUI>();
                    if (turnTxt != null)
                    {
                        turnTxt.text = $"{c.remainingTurns}t";
                    }

                    // Ação de clique para abrir o submodal de detalhes
                    if (btn != null)
                    {
                        var capturedCond = c;
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(() => {
                            if (detailModal != null)
                            {
                                string durStr = $"{capturedCond.remainingTurns} Turnos Restantes" + (capturedCond.stacks > 1 ? $" (x{capturedCond.stacks} Acúmulos)" : "");
                                detailModal.Open(capturedCond.icon, capturedCond.name, durStr, capturedCond.description);
                            }
                        });
                    }
                }
            }

            // --- SEÇÃO 2: Modificadores de Status Ativos (Positivos e Negativos) ---
            var activeModifiers = unit.PassiveManager.GetActiveStatModifiers();
            if (activeModifiers != null && modifierItemPrefab != null)
            {
                foreach (var mod in activeModifiers)
                {
                    var container = mod.isPositive ? positiveModifiersContainer : negativeModifiersContainer;
                    if (container != null)
                    {
                        var go = Instantiate(modifierItemPrefab, container);
                        go.SetActive(true);

                        // Icone do Modificador
                        var img = go.transform.Find("Icon")?.GetComponent<Image>();
                        if (img != null)
                        {
                            img.sprite = mod.icon;
                            img.gameObject.SetActive(mod.icon != null);
                        }

                        // Texto do Modificador
                        var txt = go.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
                        if (txt != null)
                        {
                            string colorTag = mod.isPositive ? "#4f4" : "#f44";
                            txt.text = $"<b><color={colorTag}>{mod.statText}</color></b> (<size=90%>{mod.remaining}</size>)";
                        }
                    }
                }
            }

            // --- SEÇÃO 3: Todas as Passivas Nativas ---
            var staticPassives = unit.PassiveManager.GetStaticPassives();
            if (staticPassives != null && allPassivesContainer != null && passiveItemPrefab != null)
            {
                foreach (var sp in staticPassives)
                {
                    var go = Instantiate(passiveItemPrefab, allPassivesContainer);
                    go.SetActive(true);

                    // Icone da Passiva
                    var img = go.transform.Find("Icon")?.GetComponent<Image>();
                    if (img != null)
                    {
                        img.sprite = sp.icon;
                        img.gameObject.SetActive(sp.icon != null);
                    }

                    // Texto e Descrição da Passiva
                    var txt = go.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
                    if (txt != null)
                    {
                        string sourceStr = $"<color=#8bf>[{sp.source}]</color>";
                        string desc = string.IsNullOrEmpty(sp.description) ? "Sem descrição disponível." : sp.description;
                        txt.text = $"<b>{sp.name}</b> {sourceStr}\n<size=85%>{desc}</size>";
                    }
                }
            }
        }

        private void ClearContainer(RectTransform container)
        {
            if (container == null) return;
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
