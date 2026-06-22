using UnityEngine;
using CelestialCross.System;
using CelestialCross.Artifacts;

namespace CelestialCross.Scenes.Unit
{
    public class UnitSelectionFlowHelper : MonoBehaviour
    {
        public static UnitSelectionFlowHelper Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void GoToInventoryForArtifact(string unitId, ArtifactType slot)
        {
            InventorySelectionContext.IsSelectionMode = true;
            InventorySelectionContext.UnitId = unitId;
            InventorySelectionContext.TargetArtifactSlot = slot;
            InventorySelectionContext.IsPetSelection = false;
            InventorySelectionContext.ReturnSceneName = "UnitScene";

            SceneTransitionManager.LoadScene("InventoryScene");
        }

        public void GoToInventoryForPet(string unitId)
        {
            InventorySelectionContext.IsSelectionMode = true;
            InventorySelectionContext.UnitId = unitId;
            InventorySelectionContext.IsPetSelection = true;
            InventorySelectionContext.ReturnSceneName = "UnitScene";

            SceneTransitionManager.LoadScene("InventoryScene");
        }
    }
}
