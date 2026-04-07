using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace CelestialCross.Artifacts
{
    public static class ArtifactGenerator
    {
        private const string TuningResourceName = "ArtifactGenerationTuning";
        private static ArtifactGenerationTuning cachedTuning;

        private static ArtifactGenerationTuning GetTuningOrNull()
        {
            if (cachedTuning == null)
                cachedTuning = Resources.Load<ArtifactGenerationTuning>(TuningResourceName);

            if (cachedTuning == null || !cachedTuning.useTuning)
                return null;

            return cachedTuning;
        }

        // Define quantos substats nascem dependendo da Raridade
        public static int GetInitialSubstatCount(ArtifactRarity rarity)
        {
            var tuning = GetTuningOrNull();
            if (tuning != null)
                return tuning.GetInitialSubstatCount(rarity);

            switch (rarity)
            {
                case ArtifactRarity.Common: return Random.Range(0, 2);    // 0 ou 1
                case ArtifactRarity.Uncommon: return Random.Range(1, 3);  // 1 ou 2
                case ArtifactRarity.Rare: return Random.Range(2, 4);      // 2 ou 3
                case ArtifactRarity.Epic: return 3;                       // 3 sempre garantidos
                case ArtifactRarity.Legendary: return 4;                  // 4 sempre garantidos
                default: return 0;
            }
        }

        // Base Stat cresce fixiamente baseado exclusivamente na estrela
        public static float GetMainStatBaseValue(StatType statType, int stars)
        {
            var tuning = GetTuningOrNull();
            if (tuning != null)
            {
                var range = tuning.GetMainBaseRange(statType, stars);
                return Mathf.Round(Random.Range(range.min, range.max));
            }

            // Essa formula pode ser refinada. Usamos arbitrários pra teste.
            // Para efeitos de escalonamento: Estrelas ditam a força do status
            float valueMultiplier = stars * 1.5f; 
            float rawValue = (statType.ToString().Contains("Percent") || statType == StatType.CriticalRate) ? 5.0f + valueMultiplier : 100f * valueMultiplier;
            return Mathf.Round(rawValue); // Garante que seja um número inteiro
        }

        // Substat tem faixas para o roll RNG inicial e UPGRADES baseados nas estrelas
        public static float GenerateSubstatValue(StatType statType, int stars)
        {
            var tuning = GetTuningOrNull();
            if (tuning != null)
            {
                var range = tuning.GetSubInitialRange(statType, stars);
                return Mathf.Round(Random.Range(range.min, range.max));
            }

            float minVal = stars * 1.0f;
            float maxVal = stars * 2.0f;

            if (statType.ToString().Contains("Percent") || statType == StatType.CriticalRate || statType == StatType.CriticalDamage)
            {
                return Mathf.Round(Random.Range(minVal * 0.5f, maxVal * 0.5f)); // Sem números quebrados
            }
            
            return Mathf.Round(Random.Range(minVal * 10, maxVal * 10)); // Valores int/flat
        }

        // Main stat aumenta exatamente numa quantidade fixa por nível
        public static float GetMainStatUpgradeIncrement(StatType statType, int stars)
        {
            var tuning = GetTuningOrNull();
            if (tuning != null)
            {
                var range = tuning.GetMainUpgradeRange(statType, stars);
                float raw = Random.Range(range.min, range.max);
                return Mathf.Max(1f, Mathf.Round(raw));
            }

            // Cresce um valor FIXO sem RNG baseado na estrela.
            float inc = stars * 1.2f;
            float rawIncrement = (statType.ToString().Contains("Percent") || statType == StatType.CriticalRate) ? inc * 0.5f : inc * 20f;
            return Mathf.Max(1f, Mathf.Round(rawIncrement)); // Garante pelo menos +1 e número inteiro
        }

        // Substats aumentam com um range variavel de sorte no RNG
        public static float GetSubstatUpgradeIncrement(StatType statType, int stars)
        {
            var tuning = GetTuningOrNull();
            if (tuning != null)
            {
                var range = tuning.GetSubUpgradeRange(statType, stars);
                float raw = Random.Range(range.min, range.max);
                return Mathf.Max(1f, Mathf.Round(raw));
            }

            float minUpgrade = stars * 0.5f;
            float maxUpgrade = stars * 1.5f;
            
            if (statType.ToString().Contains("Percent") || statType == StatType.CriticalRate)
            {
                return Mathf.Max(1f, Mathf.Round(Random.Range(minUpgrade * 0.5f, maxUpgrade * 0.5f))); // Sem números quebrados
            }

            return Mathf.Max(1f, Mathf.Round(Random.Range(minUpgrade * 5f, maxUpgrade * 5f))); // Valores int/flat
        }

        public static StatType GetRandomSubstatType(StatType mainStatToExclude, List<StatModifier> currentSubstats)
        {
            // Pega todos os stat types possiveis
            var allTypes = System.Enum.GetValues(typeof(StatType)).Cast<StatType>().ToList();
            
            // Bloqueia duplicatas (não pode rolar um substat que é do mesmo tipo do Main e que já existe listado)
            allTypes.Remove(mainStatToExclude);
            foreach(var s in currentSubstats)
            {
                allTypes.Remove(s.statType);
            }

            if(allTypes.Count > 0)
                return allTypes[Random.Range(0, allTypes.Count)];
            
            return StatType.HealthFlat; // Failsafe
        }
    }
}
