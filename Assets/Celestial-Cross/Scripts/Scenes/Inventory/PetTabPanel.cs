using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Data.Pets;
using System.Collections.Generic;

namespace CelestialCross.Scenes.Inventory
{
    public class PetTabPanel : InventoryTabPanel
    {
        [Header("Pet Detail UI")]
        public Image petSpriteImage;
        public Image petIconImage;
        public TextMeshProUGUI petNameText;
        public Transform starsContainer;
        public GameObject starPrefab;
        public TextMeshProUGUI statsText; // Mostra vida, ataque, etc.
        
        [Header("Skill UI")]
        public Image skillIconImage;
        public TextMeshProUGUI skillDescriptionText;
        public Button upgradeSkillButton; // O botão "em breve"
        
        [Header("Actions")]
        public Button releaseButton;
        public Button filterButton;

        [Header("Skill Modal")]
        public PetSkillModal skillModal;

        private RuntimePetData currentSelectedPet;
        private List<string> activePetFilter = null;
        private Celestial_Cross.Scripts.Abilities.Graph.AbilityGraphSO currentPetSkillGraph;

        private global::System.Collections.Generic.Dictionary<string, Image> petSlotImages = new global::System.Collections.Generic.Dictionary<string, Image>();
        private Color defaultSlotColor = new Color(0.2f, 0.2f, 0.28f, 1f);
        private Color selectedSlotColor = new Color(0.4f, 0.5f, 0.8f, 1f);

        protected override void Awake()
        {
            base.Awake();
            if (upgradeSkillButton != null)
            {
                upgradeSkillButton.interactable = false;
                // O tooltip "Em breve" será gerado pelo UIBuilder no prefab ou componente de tooltip.
            }

            if (releaseButton != null)
            {
                releaseButton.onClick.AddListener(OnReleaseClicked);
            }

            if (filterButton != null)
            {
                filterButton.onClick.AddListener(OnFilterClicked);
            }

            if (skillIconImage != null)
            {
                skillIconImage.raycastTarget = true;
                var btn = skillIconImage.gameObject.GetComponent<Button>();
                if (btn == null) btn = skillIconImage.gameObject.AddComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(OnSkillIconClicked);
                Debug.Log("[PetTabPanel] skillIconImage configurado com Button para cliques.");
            }
        }

        private void Start()
        {
            if (InventorySceneController.Instance != null && InventorySceneController.Instance.petFilterModal != null)
            {
                InventorySceneController.Instance.petFilterModal.OnFilterApplied += HandlePetFilter;
            }
        }

        private void HandlePetFilter(List<string> selectedSpeciesIDs)
        {
            activePetFilter = (selectedSpeciesIDs != null && selectedSpeciesIDs.Count > 0) ? selectedSpeciesIDs : null;
            Refresh();
        }

        protected override void OnShow()
        {
            base.OnShow();
            Refresh();
        }

        public override void Refresh()
        {
            if (AccountManager.Instance == null || AccountManager.Instance.PlayerAccount == null) return;
            if (InventorySceneController.Instance == null || InventorySceneController.Instance.petCatalog == null) return;
            
            var ownedPets = AccountManager.Instance.PlayerAccount.OwnedRuntimePets;
            if (ownedPets == null) return;

            foreach (Transform child in gridContent.transform)
            {
                Destroy(child.gameObject);
            }

            petSlotImages.Clear();

            var prefab = InventorySceneController.Instance.slotPrefab;
            if (prefab == null) 
            {
                Debug.LogWarning("[PetTabPanel] slotPrefab ausente no InventorySceneController. Rode o UI Builder atualizado.");
                return;
            }

            RuntimePetData firstPet = null;
            PetSpeciesSO firstSpecies = null;

            foreach (var pet in ownedPets)
            {
                if (activePetFilter != null && !activePetFilter.Contains(pet.SpeciesID)) continue;

                var species = InventorySceneController.Instance.petCatalog.GetPetSpecies(pet.SpeciesID);
                if (species == null) continue;

                var slotObj = Instantiate(prefab, gridContent.transform);
                slotObj.SetActive(true);

                var slotImg = slotObj.GetComponent<Image>();
                if (slotImg != null)
                {
                    petSlotImages[pet.UUID] = slotImg;
                    slotImg.color = defaultSlotColor;
                }

                var iconImg = slotObj.transform.Find("Icon")?.GetComponent<Image>();
                if (iconImg != null)
                {
                    iconImg.sprite = species.Icon;
                    iconImg.gameObject.SetActive(true);
                }

                var txt = slotObj.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
                if (txt != null) txt.text = species.SpeciesName;

                var btn = slotObj.GetComponent<Button>();
                if (btn != null) btn.onClick.AddListener(() => SelectPet(pet, species));

                if (firstPet == null)
                {
                    firstPet = pet;
                    firstSpecies = species;
                }
            }

            // Auto-selecionar o primeiro pet se houver algum
            if (firstPet != null)
            {
                if (currentSelectedPet == null || !petSlotImages.ContainsKey(currentSelectedPet.UUID))
                {
                    SelectPet(firstPet, firstSpecies);
                }
                else
                {
                    // Re-selecionar para atualizar highlight de cor
                    var currentSpecies = InventorySceneController.Instance.petCatalog.GetPetSpecies(currentSelectedPet.SpeciesID);
                    if (currentSpecies != null)
                        SelectPet(currentSelectedPet, currentSpecies);
                }
            }
            else
            {
                // Limpa visual se vazio
                currentSelectedPet = null;
                if (petSpriteImage != null) petSpriteImage.sprite = null;
                if (petIconImage != null) petIconImage.sprite = null;
                if (petNameText != null) petNameText.text = "Selecione um Pet";
                UpdateStars(0);
                if (statsText != null) statsText.text = "HP: —\nATK: —\nDEF: —\nSPD: —";
                if (skillIconImage != null) skillIconImage.gameObject.SetActive(false);
                if (skillDescriptionText != null) skillDescriptionText.text = "Sem Habilidade";
            }
        }

        public void SelectPet(RuntimePetData petData, PetSpeciesSO speciesSO)
        {
            currentSelectedPet = petData;

            // Feedback visual de seleção no grid
            foreach (var kvp in petSlotImages)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.color = (kvp.Key == petData.UUID) ? selectedSlotColor : defaultSlotColor;
                }
            }
            
            if (petSpriteImage != null) petSpriteImage.sprite = speciesSO.sprite;
            if (petIconImage != null) petIconImage.sprite = speciesSO.Icon;
            if (petNameText != null) petNameText.text = speciesSO.SpeciesName;

            UpdateStars(petData.RarityStars);
            UpdateStats(petData);
            UpdateSkill(speciesSO);
        }

        private void UpdateStars(int stars)
        {
            if (starsContainer == null || starPrefab == null) return;
            
            foreach (Transform child in starsContainer)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < stars; i++)
            {
                Instantiate(starPrefab, starsContainer);
            }
        }

        private void UpdateStats(RuntimePetData data)
        {
            if (statsText == null) return;
            statsText.text = $"HP: {data.Health}\nATK: {data.Attack}\nDEF: {data.Defense}\nSPD: {data.Speed}";
        }

        private void UpdateSkill(PetSpeciesSO speciesSO)
        {
            currentPetSkillGraph = (speciesSO.AbilityGraphs != null && speciesSO.AbilityGraphs.Count > 0) ? speciesSO.AbilityGraphs[0] : null;

            if (currentPetSkillGraph != null)
            {
                if (skillIconImage != null)
                {
                    skillIconImage.sprite = currentPetSkillGraph.abilityIcon;
                    skillIconImage.gameObject.SetActive(true);
                }
                if (skillDescriptionText != null)
                {
                    skillDescriptionText.text = $"<b>{currentPetSkillGraph.abilityName}</b>\n{currentPetSkillGraph.abilityDescription}";
                }
            }
            else
            {
                if (skillIconImage != null) skillIconImage.gameObject.SetActive(false);
                if (skillDescriptionText != null) skillDescriptionText.text = "Sem Habilidade";
            }
        }

        private void OnReleaseClicked()
        {
            if (currentSelectedPet == null) return;
            
            if (CelestialCross.System.PetReleaseManager.Instance != null)
            {
                CelestialCross.System.PetReleaseManager.Instance.ReleasePet(currentSelectedPet.UUID);
                currentSelectedPet = null;
                Refresh();
            }
            else
            {
                Debug.LogError("[PetTabPanel] PetReleaseManager não encontrado na cena.");
            }
        }

        private void OnSkillIconClicked()
        {
            Debug.Log($"[PetTabPanel] OnSkillIconClicked. Habilidade: {(currentPetSkillGraph != null ? currentPetSkillGraph.abilityName : "nulo")}, Modal: {(skillModal != null ? "atribuído" : "nulo")}");
            if (currentPetSkillGraph != null && skillModal != null)
            {
                skillModal.Show(currentPetSkillGraph.abilityName, currentPetSkillGraph.abilityIcon, currentPetSkillGraph.abilityDescription);
            }
        }

        private void OnFilterClicked()
        {
            if (InventorySceneController.Instance != null && InventorySceneController.Instance.petFilterModal != null)
            {
                InventorySceneController.Instance.petFilterModal.Show();
            }
        }
    }
}
