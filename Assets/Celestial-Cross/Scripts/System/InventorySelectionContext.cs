using CelestialCross.Artifacts;

namespace CelestialCross.System
{
    public static class InventorySelectionContext
    {
        public static bool IsSelectionMode { get; set; } = false;
        public static string UnitId { get; set; } = "";
        
        // Artifacts
        public static ArtifactType TargetArtifactSlot { get; set; }
        
        // Pets
        public static bool IsPetSelection { get; set; } = false;

        public static string ReturnSceneName { get; set; } = "UnitScene";

        public static void Clear()
        {
            IsSelectionMode = false;
            UnitId = "";
            IsPetSelection = false;
            ReturnSceneName = "UnitScene";
        }
    }
}
