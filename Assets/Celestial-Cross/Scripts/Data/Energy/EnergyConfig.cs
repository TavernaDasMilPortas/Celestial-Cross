using UnityEngine;

namespace CelestialCross.Data.Energy
{
    [CreateAssetMenu(menuName = "Celestial Cross/Config/Energy Config")]
    public class EnergyConfig : ScriptableObject
    {
        public int MaxEnergy = 100;
        public float RegenIntervalSeconds = 300f; // 5 minutos
    }
}
