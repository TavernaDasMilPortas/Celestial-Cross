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
            if (unitData == null || unitData.ConstellationLevel >= MAX_CONSTELLATION) return false;
            
            var account = AccountManager.Instance?.PlayerAccount;
            if (account == null) return false;

            string insigniaID = GetInsigniaItemID(unitData.UnitID);
            
            // Consome 1 insígnia
            if (account.RemoveItem(insigniaID, 1))
            {
                unitData.ConstellationLevel++;
                return true;
            }
            
            return false;
        }
        
        // Retorna os grafos passivos desbloqueados para o nível atual
        public static List<Celestial_Cross.Scripts.Abilities.Graph.AbilityGraphSO> GetUnlockedPassives(UnitData soData, int constellationLevel)
        {
            var result = new List<Celestial_Cross.Scripts.Abilities.Graph.AbilityGraphSO>();
            if (soData == null || soData.constellationConfig == null) return result;

            int count = Mathf.Min(constellationLevel, soData.constellationConfig.stars.Count);
            for (int i = 0; i < count; i++)
            {
                var graph = soData.constellationConfig.stars[i].passiveGraph;
                if (graph != null)
                    result.Add(graph);
            }
            return result;
        }
    }
}
