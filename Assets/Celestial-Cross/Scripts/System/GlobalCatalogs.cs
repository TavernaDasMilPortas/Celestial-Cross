using UnityEngine;

namespace CelestialCross.System
{
    public class GlobalCatalogs : MonoBehaviour
    {
        public static GlobalCatalogs Instance { get; private set; }

        [Header("Catalogs")]
        public UnitCatalog unitCatalog;
        public PetCatalog petCatalog;
        public ArtifactSetCatalog artifactSetCatalog;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
