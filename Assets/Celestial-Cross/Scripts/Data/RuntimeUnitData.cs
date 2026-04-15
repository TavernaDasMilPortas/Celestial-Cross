using System;

namespace CelestialCross.Data
{
    [Serializable]
    public class RuntimeUnitData
    {
        public string UnitID;
        public int StarLevel;
        public int Fragments;

        public RuntimeUnitData() { }

        public RuntimeUnitData(string unitId, int initialStars)
        {
            UnitID = unitId;
            StarLevel = initialStars;
            Fragments = 0;
        }
    }
}