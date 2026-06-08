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
        [Header("Atributos do Pet")]
        public TextMeshProUGUI hpText;
        public TextMeshProUGUI atkText;
        public TextMeshProUGUI defText;
        public TextMeshProUGUI spdText;
        public TextMeshProUGUI critChanceText;
        public TextMeshProUGUI critDmgText;
        public TextMeshProUGUI accText;
        public TextMeshProUGUI resText;

        [Header("Estrelas")]
        public Transform starsContainer;
        public GameObject starPrefab;

        [Header("Habilidade")]
        public Image skillIconImage;
        public Button skillIconButton;
        public TextMeshProUGUI skillDescText;
        public CelestialCross.Scenes.Inventory.PetSkillModal petSkillModal;
        
        [Header("Ações")]
        public Button petImageButton;
        public Button emptySlotButton;
        public UnitPetSelectModal petSelectModal;

        private UnitData currentUnitData;
        private RuntimeUnitData currentRuntimeData;
        private PetCatalog currentPetCatalog;
        private string currentUnitId;

        private void Awake()
        {
            if (petImageButton != null)
                petImageButton.onClick.AddListener(OnSelectPetClicked);
            
            if (emptySlotButton != null)
                emptySlotButton.onClick.AddListener(OnSelectPetClicked);

            if (skillIconButton != null)
                skillIconButton.onClick.AddListener(OnSkillIconClicked);
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

            if (hpText != null) hpText.text = $"HP: {petData.Health}";
            if (atkText != null) atkText.text = $"ATK: {petData.Attack}";
            if (defText != null) defText.text = $"DEF: {petData.Defense}";
            if (spdText != null) spdText.text = $"SPD: {petData.Speed}";
            if (critChanceText != null) critChanceText.text = $"CRIT: {petData.CriticalChance}%";
            if (critDmgText != null) critDmgText.text = $"C.DMG: {petData.CriticalDamage}%";
            if (accText != null) accText.text = $"ACC: {petData.EffectAccuracy}%";
            if (resText != null) resText.text = $"RES: {petData.EffectResistance}%";
            
            // Skill info: Pega do Graph primário se houver
            if (speciesSO.AbilityGraphs != null && speciesSO.AbilityGraphs.Count > 0 && speciesSO.AbilityGraphs[0] != null)
            {
                var graph = speciesSO.AbilityGraphs[0];
                if (skillIconImage != null)
                {
                    skillIconImage.gameObject.SetActive(true);
                    skillIconImage.sprite = graph.abilityIcon;
                }
                if (skillDescText != null) skillDescText.text = graph.abilityDescription;
            }
            else
            {
                if (skillIconImage != null) skillIconImage.gameObject.SetActive(false);
                if (skillDescText != null) skillDescText.text = "Nenhuma habilidade associada.";
            }

            if (starsContainer != null && starPrefab != null)
            {
                foreach (Transform child in starsContainer)
                {
                    Destroy(child.gameObject);
                }
                for (int i = 0; i < petData.RarityStars; i++)
                {
                    var star = Instantiate(starPrefab, starsContainer);
                    star.SetActive(true);
                }
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

        private void OnSkillIconClicked()
        {
            if (petSkillModal == null) return;
            var account = AccountManager.Instance?.PlayerAccount;
            var loadout = account?.GetLoadoutForUnit(currentUnitId);
            if (loadout == null || string.IsNullOrEmpty(loadout.PetID)) return;

            var petInstance = account.GetPetByUUID(loadout.PetID);
            if (petInstance == null) return;

            var petSpecies = currentPetCatalog?.GetPetSpecies(petInstance.SpeciesID);
            if (petSpecies != null && petSpecies.AbilityGraphs != null && petSpecies.AbilityGraphs.Count > 0)
            {
                var graph = petSpecies.AbilityGraphs[0];
                if (graph != null)
                {
                    petSkillModal.Show(graph.name, graph.abilityIcon, graph.abilityDescription);
                }
            }
        }
    }
}
