using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Data.Pets;
using CelestialCross.Data;

namespace CelestialCross.Scenes.Unit
{
    public class UnitDetailPanel_Pet : MonoBehaviour
    {
        [Header("UI - Pet Equipado")]
        public GameObject petEquippedContainer;
        public GameObject petEmptyContainer;
        
        public Image petSpriteImage;
        public TextMeshProUGUI petNameText;
        public TextMeshProUGUI petStatsText;
        public Image skillIconImage;
        public TextMeshProUGUI skillDescText;
        
        [Header("Ações")]
        public Button selectPetButton;
        public UnitPetSelectModal petSelectModal;

        private UnitData currentUnitData;
        private RuntimeUnitData currentRuntimeData;
        private PetCatalog currentPetCatalog;
        private string currentUnitId;

        private void Awake()
        {
            if (selectPetButton != null)
                selectPetButton.onClick.AddListener(OnSelectPetClicked);
        }

        public void Refresh(UnitData unitData, RuntimeUnitData runtimeData, PetCatalog petCatalog)
        {
            if (unitData == null || runtimeData == null) return;
            
            currentUnitData = unitData;
            currentRuntimeData = runtimeData;
            currentPetCatalog = petCatalog;
            currentUnitId = runtimeData.UnitID;

            // Busca se há um pet equipado via loadout
            var account = AccountManager.Instance?.PlayerAccount;
            var loadout = account?.GetLoadoutForUnit(runtimeData.UnitID);
            if (account == null || loadout == null || string.IsNullOrEmpty(loadout.PetID))
            {
                DisplayEmpty();
                return;
            }

            var petInstance = account.GetPetByUUID(loadout.PetID);
            if (petInstance == null)
            {
                DisplayEmpty();
                return;
            }

            var petSpecies = petCatalog.GetPetSpecies(petInstance.SpeciesID);
            if (petSpecies == null)
            {
                DisplayEmpty();
                return;
            }

            DisplayPet(petInstance, petSpecies);
        }

        private void DisplayEmpty()
        {
            if (petEquippedContainer) petEquippedContainer.SetActive(false);
            if (petEmptyContainer) petEmptyContainer.SetActive(true);
        }

        private void DisplayPet(RuntimePetData petData, PetSpeciesSO speciesSO)
        {
            if (petEquippedContainer) petEquippedContainer.SetActive(true);
            if (petEmptyContainer) petEmptyContainer.SetActive(false);

            if (petSpriteImage != null) petSpriteImage.sprite = speciesSO.sprite;
            if (petNameText != null) petNameText.text = speciesSO.SpeciesName;
            if (petStatsText != null) petStatsText.text = $"HP: {petData.Health} | ATK: {petData.Attack}";
            
            // Skill info: Pega a primeira ativa se houver
            if (speciesSO.ActiveSkills != null && speciesSO.ActiveSkills.Count > 0 && speciesSO.ActiveSkills[0] != null)
            {
                if (skillIconImage != null)
                {
                    skillIconImage.gameObject.SetActive(true);
                    skillIconImage.sprite = speciesSO.ActiveSkills[0].abilityIcon;
                }
                if (skillDescText != null) skillDescText.text = speciesSO.ActiveSkills[0].abilityDescription;
            }
            else
            {
                if (skillIconImage != null) skillIconImage.gameObject.SetActive(false);
                if (skillDescText != null) skillDescText.text = "Nenhuma habilidade ativa.";
            }
        }

        private void OnSelectPetClicked()
        {
            if (petSelectModal != null && !string.IsNullOrEmpty(currentUnitId))
                petSelectModal.Show(currentUnitId, () => {
                    Refresh(currentUnitData, currentRuntimeData, currentPetCatalog);
                    if (UnitSceneController.Instance != null && UnitSceneController.Instance.attributesDetailPanel != null)
                    {
                        var attr = UnitSceneController.Instance.attributesDetailPanel.GetComponent<UnitDetailPanel_Attributes>();
                        if (attr != null) attr.Refresh(currentUnitData, currentRuntimeData);
                    }
                });
        }
    }
}
