using System;

namespace CelestialCross.Data
{
    [Serializable]
    public class RuntimeUnitData
    {
        public string UnitID;
        public int StarLevel;
        public int Fragments;
        public int Level = 1;
        public int CurrentXP = 0;
        public int ConstellationLevel = 0;

        public RuntimeUnitData() { }

        public RuntimeUnitData(string unitId, int initialStars)
        {
            UnitID = unitId;
            StarLevel = initialStars;
            Fragments = 0;
        }
    }
}