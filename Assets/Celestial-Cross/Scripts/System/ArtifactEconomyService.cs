using UnityEngine;
using CelestialCross.Artifacts;

namespace CelestialCross.System
{
    public static class ArtifactEconomyService
    {
        // Define o aumento de custo por n�vel. Pode ser configurado/substitu�do por AnimationCurve
        public static int GetUpgradeCost(int currentLevel, int stars, ArtifactRarity rarity)
        {
            // Base simples: N�vel 0 para 1 custa 100 * estrelas, multiplicando a cada n�vel
            int baseCost = 100 * stars;
            int levelMultiplier = (currentLevel + 1) * 2;
            int rarityPremium = ((int)rarity + 1);

            return baseCost * levelMultiplier * rarityPremium;
        }

        public static int GetSellValue(ArtifactInstanceData artifact)
        {
            // Valor Base: 50 a 500 pela raridade + multiplicador de estrelas
            int baseValue = 50 * ((int)artifact.rarity + 1) * (int)artifact.stars;
            
            // Simula��o de Investimento: Como n�o salvamos o total gasto, podemos inferir matematicamente
            // ou iterar de lvl 0 at� currentLevel usando o GetUpgradeCost.
            int inferredInvestedMoney = 0;
            for (int i = 0; i < artifact.currentLevel; i++)
            {
                inferredInvestedMoney += GetUpgradeCost(i, (int)artifact.stars, artifact.rarity);
            }

            // O retorno � de 45% do investido + o valor base
            float retentionFactor = 0.45f;
            return baseValue + Mathf.FloorToInt(inferredInvestedMoney * retentionFactor);
        }

        public static bool TryUpgradeArtifact(Account account, ArtifactInstanceData artifact)
        {
            if (artifact.currentLevel >= 15)
            {
                Debug.Log("Artefato j� est� no n�vel m�ximo (+15)!");
                return false;
            }

            int cost = GetUpgradeCost(artifact.currentLevel, (int)artifact.stars, artifact.rarity);
            
            if (account.Money < cost)
            {
                Debug.Log($"Dinheiro insuficiente para upar. Custo: {cost}, Dispon�vel: {account.Money}");
                return false;
            }

            // L�gica financeira e de N�vel
            account.Money -= cost;
            artifact.currentLevel++;

            // Evoluir Main Stat
            // Usar o gerador base
            float increment = ArtifactGenerator.GetMainStatUpgradeIncrement(artifact.mainStat.statType, (int)artifact.stars);
            artifact.mainStat.value += increment;

            // L�gica de SubStats a cada N n�veis (ex: a cada 3 ou 4 n�veis)
            if (artifact.currentLevel % 3 == 0) // Exemplo: Tiers +3, +6, +9, +12, +15
            {
                // Ou Adiciona um novo stat ou upa um existente
                if (artifact.subStats.Count < 4) 
                {
                    StatType subType = ArtifactGenerator.GetRandomSubstatType(artifact.mainStat.statType, ConvertToStatModifiers(artifact.subStats));
                    float subValue = ArtifactGenerator.GenerateSubstatValue(subType, (int)artifact.stars);
                    artifact.subStats.Add(new StatModifierData(subType, subValue));
                }
                else
                {
                    // Upa um stat aleat�rio j� existente
                    int rIndex = Random.Range(0, artifact.subStats.Count);
                    var targetSub = artifact.subStats[rIndex];
                    float subIncrement = ArtifactGenerator.GetSubstatUpgradeIncrement(targetSub.statType, (int)artifact.stars);
                    targetSub.value += subIncrement;
                }
            }

            return true;
        }

        public static bool TrySellArtifact(Account account, ArtifactInstanceData artifact)
        {
            if (account == null || artifact == null) return false;
            if (!account.OwnedArtifacts.Contains(artifact)) return false;

            int sellPrice = GetSellValue(artifact);
            
            account.Money += sellPrice;
            account.OwnedArtifacts.Remove(artifact);
            
            // Garantir que a peça vendida seja desequipada do Loadout
            account.UnequipArtifactFromAll(artifact.idGUID);

            return true;
        }

        // Helper para o Generator
        private static global::System.Collections.Generic.List<StatModifier> ConvertToStatModifiers(global::System.Collections.Generic.List<StatModifierData> dataList)
        {
            var list = new global::System.Collections.Generic.List<StatModifier>();
            foreach (var d in dataList)
            {
                list.Add(new StatModifier { statType = d.statType, value = d.value });
            }
            return list;
        }
    }
}
