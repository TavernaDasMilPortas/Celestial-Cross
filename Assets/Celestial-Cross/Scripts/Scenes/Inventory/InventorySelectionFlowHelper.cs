using UnityEngine;
using UnityEngine.UI;
using CelestialCross.System;

namespace CelestialCross.Scenes.Inventory
{
    public class InventorySelectionFlowHelper : MonoBehaviour
    {
        [Header("Scene References")]
        public InventorySceneController inventoryController;
        public ArtifactTabPanel artifactTab;
        public PetTabPanel petTab;
        public Button returnBackButton;

        private void Start()
        {
            if (!InventorySelectionContext.IsSelectionMode)
            {
                // Modo normal do inventário
                if (artifactTab != null)
                {
                    if (artifactTab.equipButton != null) artifactTab.equipButton.gameObject.SetActive(false);
                    if (artifactTab.unequipButton != null) artifactTab.unequipButton.gameObject.SetActive(false);
                }
                if (petTab != null)
                {
                    if (petTab.equipButton != null) petTab.equipButton.gameObject.SetActive(false);
                    if (petTab.unequipButton != null) petTab.unequipButton.gameObject.SetActive(false);
                }
                return;
            }

            if (AccountManager.Instance != null && AccountManager.Instance.PlayerAccount != null)
            {
                // Dá um pequeno delay de frame para garantir que o InventorySceneController já inicializou as abas e rodou suas animações.
                StartCoroutine(InitFlowDelayed());
            }
            else
            {
                AccountManager.OnAccountReady += OnAccountReady;
            }
        }

        private void OnAccountReady()
        {
            AccountManager.OnAccountReady -= OnAccountReady;
            StartCoroutine(InitFlowDelayed());
        }

        private global::System.Collections.IEnumerator InitFlowDelayed()
        {
            yield return new WaitForEndOfFrame(); // Espera o Controller finalizar InitializeTabs

            Debug.Log($"[InventorySelectionFlowHelper] Modo Seleção Ativo para Unit: {InventorySelectionContext.UnitId}");

            // Interceptar o botão voltar
            if (returnBackButton != null)
            {
                returnBackButton.onClick.RemoveAllListeners();
                returnBackButton.onClick.AddListener(CancelSelection);
            }

            if (InventorySelectionContext.IsPetSelection)
            {
                SetupPetSelection();
            }
            else
            {
                SetupArtifactSelection();
            }
        }

        private void SetupArtifactSelection()
        {
            if (inventoryController != null && artifactTab != null)
            {
                inventoryController.SelectTab(artifactTab);

                // Ocultar botões originais
                if (artifactTab.upgradeButton != null) artifactTab.upgradeButton.gameObject.SetActive(false);
                if (artifactTab.sellButton != null) artifactTab.sellButton.gameObject.SetActive(false);
                if (artifactTab.filterButton != null) artifactTab.filterButton.gameObject.SetActive(false);

                // Forçar filtro para o slot correto
                var forceFilter = new ArtifactFilterData();
                forceFilter.types.Add(InventorySelectionContext.TargetArtifactSlot);
                artifactTab.ForceFilter(forceFilter);
                
                if (artifactTab.equipButton != null)
                {
                    artifactTab.equipButton.gameObject.SetActive(true);
                    artifactTab.equipButton.onClick.RemoveAllListeners();
                    artifactTab.equipButton.onClick.AddListener(OnEquipArtifactClicked);
                    
                    var txt = artifactTab.equipButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    if (txt != null) txt.text = "Equipar / Trocar";
                }

                bool hasEquipped = false;
                var acc = AccountManager.Instance?.PlayerAccount;
                if (acc != null)
                {
                    var loadout = acc.GetLoadoutForUnit(InventorySelectionContext.UnitId);
                    if (loadout != null)
                    {
                        switch (InventorySelectionContext.TargetArtifactSlot)
                        {
                            case CelestialCross.Artifacts.ArtifactType.Helmet: hasEquipped = !string.IsNullOrEmpty(loadout.HelmetID); break;
                            case CelestialCross.Artifacts.ArtifactType.Chestplate: hasEquipped = !string.IsNullOrEmpty(loadout.ChestplateID); break;
                            case CelestialCross.Artifacts.ArtifactType.Gloves: hasEquipped = !string.IsNullOrEmpty(loadout.GlovesID); break;
                            case CelestialCross.Artifacts.ArtifactType.Boots: hasEquipped = !string.IsNullOrEmpty(loadout.BootsID); break;
                            case CelestialCross.Artifacts.ArtifactType.Necklace: hasEquipped = !string.IsNullOrEmpty(loadout.NecklaceID); break;
                            case CelestialCross.Artifacts.ArtifactType.Ring: hasEquipped = !string.IsNullOrEmpty(loadout.RingID); break;
                        }
                    }
                }

                if (artifactTab.unequipButton != null)
                {
                    artifactTab.unequipButton.gameObject.SetActive(hasEquipped);
                    artifactTab.unequipButton.onClick.RemoveAllListeners();
                    artifactTab.unequipButton.onClick.AddListener(OnUnequipArtifactClicked);
                }
            }
        }

        private void SetupPetSelection()
        {
            if (inventoryController != null && petTab != null)
            {
                inventoryController.SelectTab(petTab);

                // Ocultar originais
                if (petTab.releaseButton != null) petTab.releaseButton.gameObject.SetActive(false);
                if (petTab.filterButton != null) petTab.filterButton.gameObject.SetActive(false);

                if (petTab.equipButton != null)
                {
                    petTab.equipButton.gameObject.SetActive(true);
                    petTab.equipButton.onClick.RemoveAllListeners();
                    petTab.equipButton.onClick.AddListener(OnEquipPetClicked);

                    var txt = petTab.equipButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    if (txt != null) txt.text = "Equipar Pet";
                }

                bool hasEquipped = false;
                var acc = AccountManager.Instance?.PlayerAccount;
                if (acc != null)
                {
                    var loadout = acc.GetLoadoutForUnit(InventorySelectionContext.UnitId);
                    if (loadout != null && !string.IsNullOrEmpty(loadout.PetID))
                    {
                        hasEquipped = true;
                    }
                }

                if (petTab.unequipButton != null)
                {
                    petTab.unequipButton.gameObject.SetActive(hasEquipped);
                    petTab.unequipButton.onClick.RemoveAllListeners();
                    petTab.unequipButton.onClick.AddListener(OnUnequipPetClicked);
                }
            }
        }

        private void OnEquipArtifactClicked()
        {
            if (artifactTab != null && artifactTab.CurrentSelectedArtifact != null)
            {
                var acc = AccountManager.Instance?.PlayerAccount;
                if (acc != null)
                {
                    var loadout = acc.GetLoadoutForUnit(InventorySelectionContext.UnitId);
                    if (loadout != null)
                    {
                        acc.UnequipArtifactFromAll(artifactTab.CurrentSelectedArtifact.idGUID);
                        
                        switch (InventorySelectionContext.TargetArtifactSlot)
                        {
                            case CelestialCross.Artifacts.ArtifactType.Helmet: loadout.HelmetID = artifactTab.CurrentSelectedArtifact.idGUID; break;
                            case CelestialCross.Artifacts.ArtifactType.Chestplate: loadout.ChestplateID = artifactTab.CurrentSelectedArtifact.idGUID; break;
                            case CelestialCross.Artifacts.ArtifactType.Gloves: loadout.GlovesID = artifactTab.CurrentSelectedArtifact.idGUID; break;
                            case CelestialCross.Artifacts.ArtifactType.Boots: loadout.BootsID = artifactTab.CurrentSelectedArtifact.idGUID; break;
                            case CelestialCross.Artifacts.ArtifactType.Necklace: loadout.NecklaceID = artifactTab.CurrentSelectedArtifact.idGUID; break;
                            case CelestialCross.Artifacts.ArtifactType.Ring: loadout.RingID = artifactTab.CurrentSelectedArtifact.idGUID; break;
                        }

                        AccountManager.Instance.SaveAccount();
                        if (CelestialCross.Audio.AudioManager.Instance != null) 
                            CelestialCross.Audio.AudioManager.Instance.PlayUI(CelestialCross.Audio.SoundKey.ItemEquip01);
                    }
                }
            }
            ReturnToUnitScene();
        }

        private void OnUnequipArtifactClicked()
        {
            var acc = AccountManager.Instance?.PlayerAccount;
            if (acc != null)
            {
                var loadout = acc.GetLoadoutForUnit(InventorySelectionContext.UnitId);
                if (loadout != null)
                {
                    string toUnequip = null;
                    switch (InventorySelectionContext.TargetArtifactSlot)
                    {
                        case CelestialCross.Artifacts.ArtifactType.Helmet: toUnequip = loadout.HelmetID; break;
                        case CelestialCross.Artifacts.ArtifactType.Chestplate: toUnequip = loadout.ChestplateID; break;
                        case CelestialCross.Artifacts.ArtifactType.Gloves: toUnequip = loadout.GlovesID; break;
                        case CelestialCross.Artifacts.ArtifactType.Boots: toUnequip = loadout.BootsID; break;
                        case CelestialCross.Artifacts.ArtifactType.Necklace: toUnequip = loadout.NecklaceID; break;
                        case CelestialCross.Artifacts.ArtifactType.Ring: toUnequip = loadout.RingID; break;
                    }
                    if (!string.IsNullOrEmpty(toUnequip))
                    {
                        acc.UnequipArtifactFromAll(toUnequip);
                        AccountManager.Instance.SaveAccount();
                        if (CelestialCross.Audio.AudioManager.Instance != null) 
                            CelestialCross.Audio.AudioManager.Instance.PlayUI(CelestialCross.Audio.SoundKey.ItemUnequip01);
                    }
                }
            }
            ReturnToUnitScene();
        }

        private void OnEquipPetClicked()
        {
            if (petTab != null && petTab.CurrentSelectedPet != null)
            {
                var acc = AccountManager.Instance?.PlayerAccount;
                if (acc != null)
                {
                    var loadout = acc.GetLoadoutForUnit(InventorySelectionContext.UnitId);
                    if (loadout != null)
                    {
                        acc.UnequipPetFromAll(petTab.CurrentSelectedPet.UUID);
                        loadout.PetID = petTab.CurrentSelectedPet.UUID;
                        AccountManager.Instance.SaveAccount();
                        if (CelestialCross.Audio.AudioManager.Instance != null) 
                            CelestialCross.Audio.AudioManager.Instance.PlayUI(CelestialCross.Audio.SoundKey.ItemEquip01);
                    }
                }
            }
            ReturnToUnitScene();
        }

        private void OnUnequipPetClicked()
        {
            var acc = AccountManager.Instance?.PlayerAccount;
            if (acc != null)
            {
                var loadout = acc.GetLoadoutForUnit(InventorySelectionContext.UnitId);
                if (loadout != null && !string.IsNullOrEmpty(loadout.PetID))
                {
                    acc.UnequipPetFromAll(loadout.PetID);
                    loadout.PetID = string.Empty;
                    AccountManager.Instance.SaveAccount();
                    if (CelestialCross.Audio.AudioManager.Instance != null) 
                        CelestialCross.Audio.AudioManager.Instance.PlayUI(CelestialCross.Audio.SoundKey.ItemUnequip01);
                }
            }
            ReturnToUnitScene();
        }

        private void CancelSelection()
        {
            ReturnToUnitScene();
        }

        private void ReturnToUnitScene()
        {
            string ret = InventorySelectionContext.ReturnSceneName;
            InventorySelectionContext.Clear();
            SceneTransitionManager.LoadScene(ret);
        }
    }
}
