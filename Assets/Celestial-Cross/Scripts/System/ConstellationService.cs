using System.Collections.Generic;
using CelestialCross.Data;
using UnityEngine;

namespace CelestialCross.System
{
    public static class ConstellationService
    {
        public const int MAX_CONSTELLATION = 6;
        
        public static string GetInsigniaItemID(string unitID) => $"insignia_{unitID}";
        
        // Chamado quando jogador recebe unidade duplicata (fictício para integrar com Inventory/Gacha depois)
        public static void HandleDuplicateUnit(string unitID)
        {
            // TODO: Integrar com Account/Inventory real
            Debug.Log($"[ConstellationService] Gerando insígnia para duplicata de {unitID}");
        }
        
        // Tenta subir constelação
        public static bool TryUpgradeConstellation(RuntimeUnitData unitData)
        {
            if (unitData.ConstellationLevel >= MAX_CONSTELLATION) return false;
            
            // TODO: Checar e consumir item do inventário real aqui
            unitData.ConstellationLevel++;
            return true;
        }
        
        // Retorna os grafos passivos desbloqueados para o nível atual
        public static List<Celestial_Cross.Scripts.Abilities.Graph.AbilityGraphSO> GetUnlockedPassives(UnitData soData, int constellationLevel)
        {
            var result = new List<Celestial_Cross.Scripts.Abilities.Graph.AbilityGraphSO>();
            if (soData == null) return result;

            for (int i = 0; i < constellationLevel && i < soData.constellationPassives.Count; i++)
            {
                if (soData.constellationPassives[i] != null)
                    result.Add(soData.constellationPassives[i]);
            }
            return result;
        }
    }
}
