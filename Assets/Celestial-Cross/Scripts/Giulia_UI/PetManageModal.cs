using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using CelestialCross.System;
using CelestialCross.Data.Pets;

namespace CelestialCross.Giulia_UI
{
    public class PetManageModal : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI detailsText;
        [SerializeField] private Button releaseButton;
        [SerializeField] private TextMeshProUGUI releaseYieldText;
        [SerializeField] private Button closeButton;

        private RuntimePetData currentPet;
        private Action onStateChanged;

        private void Awake()
        {
            if (releaseButton != null) releaseButton.onClick.AddListener(OnReleaseClicked);
            if (closeButton != null) closeButton.onClick.AddListener(Close);
        }

        public void Show(RuntimePetData pet, Action onStateChangedCallback)
        {
            currentPet = pet;
            onStateChanged = onStateChangedCallback;
            RefreshUI();
            
            // Force modal on top
            transform.SetAsLastSibling();
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        private void RefreshUI()
        {
            if (currentPet == null) { Close(); return; }
            
            var inventoryUI = FindObjectOfType<InventoryUI>();
            string petName = currentPet.DisplayName;
            CelestialCross.Data.Pets.PetSpeciesSO speciesData = null;
            if (inventoryUI != null && inventoryUI.petCatalog != null)
            {
                speciesData = inventoryUI.petCatalog.GetPetSpecies(currentPet.SpeciesID);
                if (speciesData != null) petName = speciesData.SpeciesName;
            }

            titleText.text = $"Pet: {petName}";

            string baseInfo = $"Estrelas: {currentPet.RarityStars}\nLevel: {currentPet.CurrentLevel}\n\n";
            string statsInfo = $"<b>Status:</b>\n" +
                               $"- HP: {currentPet.Health}\n" +
                               $"- ATK: {currentPet.Attack}\n" +
                               $"- DEF: {currentPet.Defense}\n" +
                               $"- SPD: {currentPet.Speed}\n" +
                               $"- CRIT: {currentPet.CriticalChance}%\n" +
                               $"- ACC: {currentPet.EffectAccuracy}%\n\n";

            string activeSkill = speciesData != null && speciesData.PassiveSkills != null && speciesData.PassiveSkills.Count > 0 
                ? $"<color=#ffffaa>{speciesData.PassiveSkills[0].abilityName}</color>\n" 
                : "";

            detailsText.text = baseInfo + statsInfo + activeSkill;

            var acc = AccountManager.Instance.PlayerAccount;
            bool isEquipped = acc.IsPetEquipped(currentPet.UUID);

            if (PetReleaseManager.Instance != null && PetReleaseManager.Instance.ReleaseConfig != null)
            {
                int stars = currentPet.RarityStars;
                int stardustAmount = PetReleaseManager.Instance.ReleaseConfig.StardustPerStar[Mathf.Min(stars, PetReleaseManager.Instance.ReleaseConfig.StardustPerStar.Length - 1)];
                int petSoulsAmount = PetReleaseManager.Instance.ReleaseConfig.PetSoulsPerStar[Mathf.Min(stars, PetReleaseManager.Instance.ReleaseConfig.PetSoulsPerStar.Length - 1)];
                releaseYieldText.text = isEquipped ? "EQUIPADO\n(Remova para libertar)" : $"LIBERTAR\n(+{stardustAmount} Poeira\n+{petSoulsAmount} Fragmentos)";
            }
            else
            {
                int stars = currentPet.RarityStars; if (stars < 0 || stars > 5) stars = 1;
                releaseYieldText.text = isEquipped ? "EQUIPADO" : $"LIBERTAR\n(+{stars * 10} Poeira\n+{stars * 1} Fragmentos)";
            }
            
            releaseButton.interactable = !isEquipped;
        }

        private void OnReleaseClicked()
        {
            if (currentPet == null) return;
            
            PetReleaseManager.Instance.ReleasePet(currentPet.UUID);
            Debug.Log($"Pet {currentPet.UUID} released successfully.");
            onStateChanged?.Invoke();
            Close();
        }
    }
}


