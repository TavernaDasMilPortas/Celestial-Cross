using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace CelestialCross.UI.Skills
{
    public class PassiveListModal : MonoBehaviour
    {
        public GameObject modalRoot;
        public RectTransform passivesContainer;
        public RectTransform conditionsContainer;
        public RectTransform buffsContainer;
        
        public GameObject listItemPrefab;
        public Button closeButton;

        private void Start()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Close);
        }

        public void Open(global::Unit unit)
        {
            if (unit == null) return;
            modalRoot.SetActive(true);
            Populate(unit);
        }

        public void Close()
        {
            modalRoot.SetActive(false);
        }

        private void Populate(global::Unit unit)
        {
            foreach (Transform child in passivesContainer) Destroy(child.gameObject);
            foreach (Transform child in conditionsContainer) Destroy(child.gameObject);
            foreach (Transform child in buffsContainer) Destroy(child.gameObject);

            // Populate Passives
            if (unit.Data != null)
            {
                // Constellation, pets, etc. For demonstration, we just get from UnitData
                AddListItem(passivesContainer, "Passivas", "Efeitos base da unidade e constelação");
            }

            // Populate Conditions & Buffs from PassiveManager or StatusManager
            // Example:
            if (unit.PassiveManager != null)
            {
                var activePassives = unit.PassiveManager.GetActiveConditionNames();
                if (activePassives != null)
                {
                    foreach (var p in activePassives)
                    {
                        AddListItem(conditionsContainer, p, "Condição Ativa");
                    }
                }
            }
            
            // Add Buffs/Debuffs (stat modifiers)
            AddListItem(buffsContainer, "Buffs", "Exemplo de status modificado");
        }

        private void AddListItem(RectTransform container, string title, string desc)
        {
            if (listItemPrefab == null) return;
            var go = Instantiate(listItemPrefab, container);
            var text = go.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = $"<b>{title}</b>\n<size=80%>{desc}</size>";
            }
        }
    }
}
