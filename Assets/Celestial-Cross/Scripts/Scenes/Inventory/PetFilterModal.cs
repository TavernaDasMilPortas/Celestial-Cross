using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using CelestialCross.Data.Pets;

namespace CelestialCross.Scenes.Inventory
{
    public class PetFilterModal : MonoBehaviour
    {
        public global::System.Action<List<string>> OnFilterApplied;
        [Header("UI References")]
        public Transform speciesGridContainer;
        public GameObject speciesIconPrefab; // Prefab contendo Image e Button e um outline para seleção
        public Button applyFilterButton;
        public Button closeButton;

        [Header("Data")]
        public PetCatalog petCatalog;

        private List<string> selectedSpeciesIDs = new List<string>();

        private void Awake()
        {
            if (applyFilterButton != null) applyFilterButton.onClick.AddListener(ApplyFilter);
            if (closeButton != null) closeButton.onClick.AddListener(Hide);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            UIModalJuicer.AnimateModalShow(GetComponent<RectTransform>());
            PopulateGrid();
        }

        public void Hide()
        {
            UIModalJuicer.AnimateModalHide(GetComponent<RectTransform>(), () => gameObject.SetActive(false));
        }

        private void PopulateGrid()
        {
            // Limpa o grid
            foreach (Transform child in speciesGridContainer)
            {
                Destroy(child.gameObject);
            }

            var allSpecies = petCatalog.GetAllPetSpecies(); // Requer acesso aos dados
            
            foreach (var species in allSpecies)
            {
                var iconObj = Instantiate(speciesIconPrefab, speciesGridContainer);
                var img = iconObj.GetComponent<Image>();
                var btn = iconObj.GetComponent<Button>();
                
                img.sprite = species.Icon;
                
                bool isOwned = CheckIfOwned(species.id); // Implementar lógica de verificação
                
                if (!isOwned)
                {
                    btn.interactable = false;
                    img.color = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Acinzentado e transparente
                }
                else
                {
                    btn.interactable = true;
                    img.color = Color.white;
                    btn.onClick.AddListener(() => ToggleSpeciesSelection(species.id, iconObj));
                    
                    // Restaurar estado visual se já estiver selecionado
                    UpdateVisualSelection(iconObj, selectedSpeciesIDs.Contains(species.id));
                }
            }
        }

        private bool CheckIfOwned(string speciesID)
        {
            var acc = global::AccountManager.Instance?.PlayerAccount;
            if (acc == null || acc.OwnedRuntimePets == null) return false;
            foreach(var p in acc.OwnedRuntimePets) {
                if (p.SpeciesID == speciesID) return true;
            }
            return false;
        }

        private void ToggleSpeciesSelection(string id, GameObject iconObj)
        {
            if (selectedSpeciesIDs.Contains(id))
            {
                selectedSpeciesIDs.Remove(id);
                UpdateVisualSelection(iconObj, false);
            }
            else
            {
                selectedSpeciesIDs.Add(id);
                UpdateVisualSelection(iconObj, true);
            }
        }

        private void UpdateVisualSelection(GameObject iconObj, bool isSelected)
        {
            // Ativar ou desativar o outline/highlight (supondo que é o primeiro filho)
            if (iconObj.transform.childCount > 0)
            {
                iconObj.transform.GetChild(0).gameObject.SetActive(isSelected);
            }
        }

        private void ApplyFilter()
        {
            OnFilterApplied?.Invoke(new List<string>(selectedSpeciesIDs));
            Hide();
        }
    }
}
