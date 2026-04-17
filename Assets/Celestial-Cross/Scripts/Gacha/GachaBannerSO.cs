using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace CelestialCross.Gacha
{
    public enum GachaRewardType { Unit, Pet, Artifact }
    public enum GachaRarity { Base, Uncommon, Rare, Epic, Legendary, Supreme }

    [global::System.Serializable]
    public class GachaRewardEntry
    {
        [EnumToggleButtons]
        public GachaRewardType RewardType;

#if UNITY_EDITOR
        [ValueDropdown("GetAllowedRarities")]
#endif
        public GachaRarity Rarity;

#if UNITY_EDITOR
        private IEnumerable<GachaRarity> GetAllowedRarities()
        {
            var parent = UnityEditor.Selection.activeObject as GachaBannerSO;
            if (parent != null && parent.BasicProbabilities != null && parent.BasicProbabilities.Count > 0)
            {
                var list = new List<GachaRarity>();
                foreach (var p in parent.BasicProbabilities) 
                    list.Add(p.Rarity);
                return list;
            }
            return (IEnumerable<GachaRarity>)global::System.Enum.GetValues(typeof(GachaRarity));
        }
#endif
        
        [ShowIf("RewardType", GachaRewardType.Unit)]
        public UnitData UnitData; // Se for Unit
        
        [ShowIf("RewardType", GachaRewardType.Pet)]
        public CelestialCross.Data.Pets.PetSpeciesSO PetSpeciesData; // Se for Pet
        
        [ShowIf("RewardType", GachaRewardType.Artifact)]
        public CelestialCross.Artifacts.ArtifactSet ArtifactSet; // Se for Artefato
        
        [Tooltip("Peso relativo do item dentro da sua raridade (Padrão 1)")]
        public int Weight = 1;

        [Title("Status do Drop")]
        [Tooltip("Quantas estrelas este Personagem, Pet ou Artefato terá ao ser sorteado?")]
        [Range(1, 6)] public int ItemStars = 3;

        [Tooltip("Qual a raridade base gerada (Apenas para Artefatos)")]
        [ShowIf("RewardType", GachaRewardType.Artifact)]
        public CelestialCross.Artifacts.ArtifactRarity ArtifactRarity = CelestialCross.Artifacts.ArtifactRarity.Common;

        public string GetID()
        {
            if (RewardType == GachaRewardType.Unit && UnitData != null) return UnitData.UnitID;
            if (RewardType == GachaRewardType.Pet && PetSpeciesData != null) return PetSpeciesData.id;
            if (RewardType == GachaRewardType.Artifact && ArtifactSet != null) return ArtifactSet.id;
            return "";
        }

#if UNITY_EDITOR
        public string GetEntryLabel()
        {
            string labelString = RewardType.ToString();
            string subLabel = "None";

            if (RewardType == GachaRewardType.Unit && UnitData != null) subLabel = string.IsNullOrEmpty(UnitData.displayName) ? UnitData.name : UnitData.displayName;
            else if (RewardType == GachaRewardType.Pet && PetSpeciesData != null) subLabel = string.IsNullOrEmpty(PetSpeciesData.SpeciesName) ? PetSpeciesData.name : PetSpeciesData.SpeciesName;
            else if (RewardType == GachaRewardType.Artifact && ArtifactSet != null) subLabel = string.IsNullOrEmpty(ArtifactSet.setName) ? ArtifactSet.name : ArtifactSet.setName;

            return $"[{labelString}] {subLabel}";
        }
#endif
    }

    [global::System.Serializable]
    public class GachaRarityProbability
    {
        public GachaRarity Rarity;
        [Range(0f, 100f)] public float BaseChance;
    }

    [CreateAssetMenu(fileName = "NewGachaBanner", menuName = "Celestial Cross/Shop & Gacha/Gacha Banner Settings")]
    public class GachaBannerSO : ScriptableObject
    {
        [Header("Configurações do Banner")]
        public string BannerID;
        public string BannerName;
        public Sprite BannerSplashArt;
        public int CostPerPull = 1; // Quantos 'Mapas das Estrelas' custa 1 tiro
        
        [Header("Sistema de Garantia (Pity)")]
        [Tooltip("Quantos tiros antes da chance do supremo começar a escalar (Soft)")]
        public int SoftPityThreshold = 70;
        
        [Tooltip("Em qual tiro o personagem supremo é 100% garantido (Hard)")]
        public int HardPityThreshold = 90;
        
        [Tooltip("Um premio Uncommon ou superior é garantido a cada N tiros")]
        public int GuaranteedAboveBaseEvery = 10;
        
        [Header("Múltiplos Supremos e Escolha (Path)")]
        public bool HasEpitomizedPath = false;
        
        [Tooltip("Lista de opções de Supremo em Destaque. O jogador pode escolher um para focar no sistema Pity.")]
        [ListDrawerSettings(ListElementLabelName = "GetEntryLabel")]
        public List<GachaRewardEntry> SupremeChoices = new List<GachaRewardEntry>();

        [Header("Tabelas de Probabilidade e Pool")]
        public List<GachaRarityProbability> BasicProbabilities = new List<GachaRarityProbability>();
        
        [ListDrawerSettings(ListElementLabelName = "GetEntryLabel")]
        public List<GachaRewardEntry> TotalPool = new List<GachaRewardEntry>();
    }
}