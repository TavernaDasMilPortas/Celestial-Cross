using System;

namespace CelestialCross.Storage
{
    [Serializable]
    public class StorageMetadata
    {
        public string LastSaveTime;
        public int Money;
        public int Energy;
        
        // Pode ser expandido para mostrar mais dados no modal de conflito
    }
}
