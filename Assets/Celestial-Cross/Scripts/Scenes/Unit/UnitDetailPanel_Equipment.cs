using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Data;

namespace CelestialCross.Scenes.Unit
{
    public class UnitDetailPanel_Equipment : MonoBehaviour
    {
        [Header("Slots")]
        public Button[] artifactSlotButtons = new Button[6];
        public Image[] artifactSlotImages = new Image[6];
        
        [Header("Set Skills")]
        public Transform skillsContainer;
        public GameObject skillIconPrefab;

        [Header("Modals")]
        public UnitArtifactSelectModal artifactSelectModal;
        public ArtifactMiniInfoModal miniInfoModal;

        private UnitData currentUnitData;
        private RuntimeUnitData currentRuntimeData;
        private ArtifactSetCatalog currentArtifactCatalog;
        private string currentUnitId;

        private void Awake()
        {
            for (int i = 0; i < artifactSlotButtons.Length; i++)
            {
                int slotIndex = i;
                if (artifactSlotButtons[i] != null)
                {
                    artifactSlotButtons[i].onClick.AddListener(() => OnSlotClicked(slotIndex));
                }
            }
        }

        public void Refresh(UnitData unitData, RuntimeUnitData runtimeData, ArtifactSetCatalog artifactCatalog)
        {
            var acc = AccountManager.Instance?.PlayerAccount;
            if (acc == null || runtimeData == null) return;

            currentUnitData = unitData;
            currentRuntimeData = runtimeData;
            currentArtifactCatalog = artifactCatalog;
            currentUnitId = runtimeData.UnitID;

            // Limpar
            for(int i = 0; i < 6; i++)
            {
                if (artifactSlotImages[i] != null) artifactSlotImages[i].gameObject.SetActive(false);
            }

            if (skillsContainer != null)
            {
                foreach(Transform child in skillsContainer)
                {
                    if (child.gameObject != skillIconPrefab) Destroy(child.gameObject);
                }
            }

            var loadout = acc.GetLoadoutForUnit(runtimeData.UnitID);
            if (loadout == null) return;

            CelestialCross.Artifacts.ArtifactType[] slotTypes = (CelestialCross.Artifacts.ArtifactType[])global::System.Enum.GetValues(typeof(CelestialCross.Artifacts.ArtifactType));

            for (int i = 0; i < 6; i++)
            {
                var sType = slotTypes[i];
                string equippedGuid = null;

                switch (sType)
                {
                    case CelestialCross.Artifacts.ArtifactType.Helmet: equippedGuid = loadout.HelmetID; break;
                    case CelestialCross.Artifacts.ArtifactType.Chestplate: equippedGuid = loadout.ChestplateID; break;
                    case CelestialCross.Artifacts.ArtifactType.Gloves: equippedGuid = loadout.GlovesID; break;
                    case CelestialCross.Artifacts.ArtifactType.Boots:  equippedGuid = loadout.BootsID; break;
                    case CelestialCross.Artifacts.ArtifactType.Necklace: equippedGuid = loadout.NecklaceID; break;
                    case CelestialCross.Artifacts.ArtifactType.Ring:   equippedGuid = loadout.RingID; break;
                }

                if (!string.IsNullOrEmpty(equippedGuid))
                {
                    var artifact = acc.GetArtifactByGuid(equippedGuid);
                    if (artifact != null && artifactCatalog != null)
                    {
                        var set = artifactCatalog.GetSetById(artifact.artifactSetId);
                        if (set != null && artifactSlotImages[i] != null)
                        {
                            artifactSlotImages[i].sprite = set.GetIconForSlot(artifact.slot);
                            artifactSlotImages[i].gameObject.SetActive(true);
                            artifactSlotImages[i].preserveAspect = true;
                        }
                    }
                }
            }
        }

        private void OnSlotClicked(int index)
        {
            if (artifactSelectModal != null && !string.IsNullOrEmpty(currentUnitId))
            {
                CelestialCross.Artifacts.ArtifactType[] slotTypes = (CelestialCross.Artifacts.ArtifactType[])global::System.Enum.GetValues(typeof(CelestialCross.Artifacts.ArtifactType));
                if (index >= 0 && index < slotTypes.Length)
                {
                    artifactSelectModal.Show(currentUnitId, slotTypes[index], () => {
                        Refresh(currentUnitData, currentRuntimeData, currentArtifactCatalog);
                        if (UnitSceneController.Instance != null && UnitSceneController.Instance.attributesDetailPanel != null)
                        {
                            var attr = UnitSceneController.Instance.attributesDetailPanel.GetComponent<UnitDetailPanel_Attributes>();
                            if (attr != null) attr.Refresh(currentUnitData, currentRuntimeData);
                        }
                    });
                }
            }
        }
    }
}
