using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace CelestialCross.Giulia_UI
{
    public class ItemsInventoryUI : MonoBehaviour
    {
        [Header("Painel Superior (Detalhes)")]
        public Image selectedItemIcon;
        public TMP_Text selectedItemNameText;
        public TMP_Text selectedItemQuantityText;
        public TMP_Text selectedItemDescriptionText;

        [Header("Painel Inferior (Grade/Filtros)")]
        public Transform gridContainer;
        public GameObject itemSlotPrefab;

        [Header("Filtros (Abas Internas)")]
        public Button btnFilterAll;
        public Button btnFilterPetSouls;
        public Button btnFilterInsignias;
        // Adicione mais botões (Consumíveis, Materiais, etc) no futuro

        // Filtro Atual
        public enum ItemFilter { All, PetSouls, Insignias }
        private ItemFilter currentFilter = ItemFilter.All;

        // Item que foi clicado
        private ItemQuantity currentSelectedItem;

        private void Start()
        {
            SetupTabButtons();
            RefreshGrid();
        }

        private void SetupTabButtons()
        {
            if(btnFilterAll != null)
            {
                btnFilterAll.onClick.AddListener(() => SetFilter(ItemFilter.All));
            }

            if(btnFilterPetSouls != null)
            {
                btnFilterPetSouls.onClick.AddListener(() => SetFilter(ItemFilter.PetSouls));
            }

            if(btnFilterInsignias != null)
            {
                btnFilterInsignias.onClick.AddListener(() => SetFilter(ItemFilter.Insignias));
            }
        }

        public void SetFilter(ItemFilter newFilter)
        {
            currentFilter = newFilter;
            RefreshGrid();
        }

        public void RefreshGrid()
        {
            if (AccountManager.Instance == null || AccountManager.Instance.PlayerAccount == null)
                return;

            List<ItemQuantity> allItems = AccountManager.Instance.PlayerAccount.OwnedItems;

            // Limpa o grid atual
            foreach (Transform child in gridContainer)
            {
                Destroy(child.gameObject);
            }

            // Preenche baseado no filtro selecionado
            foreach (var item in allItems)
            {
                if (PassesFilter(item.ItemID, currentFilter))
                {
                    CreateItemSlot(item);
                }
            }

            // Auto-selecionar o primeiro se houver
            if (gridContainer.childCount > 0)
            {
                var firstButton = gridContainer.GetChild(0).GetComponent<Button>();
                if (firstButton != null)
                    firstButton.onClick.Invoke();
            }
            else
            {
                ClearSelection();
            }
        }

        private bool PassesFilter(string itemId, ItemFilter filter)
        {
            if (filter == ItemFilter.All)
                return true;

            if (filter == ItemFilter.PetSouls)
                return itemId.StartsWith("soul_"); // A lógica que criamos no PetReleaseManager

            if (filter == ItemFilter.Insignias)
                return itemId.StartsWith("insignia_");

            return true;
        }

        private void CreateItemSlot(ItemQuantity item)
        {
            if (itemSlotPrefab == null) return;
            GameObject slotGo = Instantiate(itemSlotPrefab, gridContainer);
            slotGo.SetActive(true);
            Image bg = slotGo.GetComponent<Image>();
            if (bg != null && bg.color == Color.white) bg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            
            string itemNameText = item.ItemID;
            if (item.ItemID.StartsWith("soul_")) {
                string speciesName = item.ItemID.Replace("soul_", "");
                if (speciesName.Length > 1) speciesName = char.ToUpper(speciesName[0]) + speciesName.Substring(1);
                itemNameText = "Fragmento de\n" + speciesName;
            } else if (item.ItemID.StartsWith("insignia_")) {
                string unitName = item.ItemID.Replace("insignia_", "");
                if (unitName.Length > 1) unitName = char.ToUpper(unitName[0]) + unitName.Substring(1);
                itemNameText = "Insígnia de\n" + unitName;
            }
            
            TMP_Text quantityText = slotGo.GetComponentInChildren<TMP_Text>();
            if (quantityText != null) {
                quantityText.color = Color.white;
                quantityText.alignment = TextAlignmentOptions.Center;
                quantityText.text = itemNameText + "\n<color=#ffff00>x" + item.Quantity + "</color>";
            }
            
            Button btn = slotGo.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(() => { OnItemSelected(item); });
        }

        private void OnItemSelected(ItemQuantity item)
        {
            currentSelectedItem = item;

            if (selectedItemNameText != null)
            {
                string displayName = item.ItemID;
                if (item.ItemID.StartsWith("soul_"))
                {
                    string speciesName = item.ItemID.Replace("soul_", "");
                    displayName = char.ToUpper(speciesName[0]) + speciesName.Substring(1) + " (Pet Soul)";
                }
                else if (item.ItemID.StartsWith("insignia_"))
                {
                    string unitName = item.ItemID.Replace("insignia_", "");
                    displayName = char.ToUpper(unitName[0]) + unitName.Substring(1) + " (Insígnia Estelar)";
                }

                selectedItemNameText.text = displayName;
            }

            if (selectedItemQuantityText != null)
                selectedItemQuantityText.text = $"Possui: {item.Quantity}";

            if (selectedItemDescriptionText != null)
            {
                if (item.ItemID.StartsWith("soul_"))
                    selectedItemDescriptionText.text = "Material obtido ao soltar esta espécie de pet. Pode ser usado no futuro para evoluir suas habilidades exclusivas.";
                else if (item.ItemID.StartsWith("insignia_"))
                    selectedItemDescriptionText.text = "Material obtido ao adquirir uma cópia repetida deste personagem. Pode ser usado na aba do personagem para aprimorar sua Constelação e desbloquear habilidades passivas.";
                else
                    selectedItemDescriptionText.text = "Nenhuma descrição disponível.";
            }

            // Ícone: A ser preenchido quando houver catálogos
            // if (selectedItemIcon != null) { ... }
        }

        private void ClearSelection()
        {
            currentSelectedItem = null;
            if (selectedItemNameText != null) selectedItemNameText.text = "Selecione um item";
            if (selectedItemQuantityText != null) selectedItemQuantityText.text = "";
            if (selectedItemDescriptionText != null) selectedItemDescriptionText.text = "";
            if (selectedItemIcon != null) selectedItemIcon.sprite = null;
        }
    }
}
