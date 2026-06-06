using System;

namespace CelestialCross.Data.Energy
{
    [Serializable]
    public class EnergyData
    {
        public int CurrentEnergy;
        public string LastRegenTimestampUTC;
        public string LastServerTimestampUTC;
    }
}
