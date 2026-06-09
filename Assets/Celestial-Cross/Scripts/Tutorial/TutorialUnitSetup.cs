using UnityEngine;
using System;

namespace CelestialCross.Tutorial
{
    [Serializable]
    public class TutorialUnitSetup
    {
        public UnitData UnitData;
        public Team Team;
        public Vector2Int GridPosition;
        public int OverrideHP; // 0 = usa o padrão
    }
}
