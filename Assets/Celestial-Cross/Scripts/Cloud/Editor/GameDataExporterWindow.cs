using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using CelestialCross.Cloud.Data;
using CelestialCross.Data.Pets;
using CelestialCross.Artifacts;
using CelestialCross.Gacha;

namespace CelestialCross.Cloud.Editor
{
    public class GameDataExporterWindow : EditorWindow
    {
        [MenuItem("Celestial Cross/Cloud/Exportar Game Data")]
        public static void ShowWindow()
        {
            GetWindow<GameDataExporterWindow>("Game Data Exporter");
        }

        private void OnGUI()
        {
            GUILayout.Label("Exportador de Dados para o Backend (Azure)", EditorStyles.boldLabel);
            
            GUILayout.Space(10);
            GUILayout.Label("Este processo coleta Pets, Artefatos e Banners e os salva\nem formato JSON na pasta do Backend.");
            
            GUILayout.Space(20);
            if (GUILayout.Button("Exportar Game Data Agora", GUILayout.Height(40)))
            {
                ExportGameData();
            }
        }

        private void ExportGameData()
        {
            var masterData = new MasterGameData();

            // 1. Export Pet Species
            var petGuids = AssetDatabase.FindAssets("t:PetSpeciesSO");
            foreach (var guid in petGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var petSO = AssetDatabase.LoadAssetAtPath<PetSpeciesSO>(path);
                if (petSO != null)
                {
                    var p = new PetSpeciesConfigData
                    {
                        id = petSO.id,
                        name = petSO.SpeciesName,
                        statRanges = new PetStatRangesData
                        {
                            health = new FloatRangeData { min = petSO.MinBaseHealth, max = petSO.MaxBaseHealth },
                            attack = new FloatRangeData { min = petSO.MinBaseAttack, max = petSO.MaxBaseAttack },
                            defense = new FloatRangeData { min = petSO.MinBaseDefense, max = petSO.MaxBaseDefense },
                            speed = new FloatRangeData { min = petSO.MinBaseSpeed, max = petSO.MaxBaseSpeed },
                            criticalChance = new FloatRangeData { min = petSO.MinBaseCriticalChance, max = petSO.MaxBaseCriticalChance },
                            criticalDamage = new FloatRangeData { min = petSO.MinBaseCriticalDamage, max = petSO.MaxBaseCriticalDamage },
                            effectAccuracy = new FloatRangeData { min = petSO.MinBaseEffectAccuracy, max = petSO.MaxBaseEffectAccuracy },
                            effectResistance = new FloatRangeData { min = petSO.MinBaseEffectResistance, max = petSO.MaxBaseEffectResistance }
                        }
                    };
                    masterData.petSpecies.Add(p);
                }
            }

            // 2. Export Star Multipliers
            masterData.starMultipliers.Add(new KVP_StringFloat { key = "1", value = 1.00f });
            masterData.starMultipliers.Add(new KVP_StringFloat { key = "2", value = 1.20f });
            masterData.starMultipliers.Add(new KVP_StringFloat { key = "3", value = 1.40f });
            masterData.starMultipliers.Add(new KVP_StringFloat { key = "4", value = 1.60f });
            masterData.starMultipliers.Add(new KVP_StringFloat { key = "5", value = 1.80f });
            masterData.starMultipliers.Add(new KVP_StringFloat { key = "6", value = 2.00f });

            // 3. Export Artifact Tuning
            var tuning = Resources.Load<ArtifactGenerationTuning>("ArtifactGenerationTuning");
            if (tuning != null)
            {
                // Initial Substats
                masterData.artifactTuning.initialSubstatCounts.Add(new KVP_StringIntRange { key = ArtifactRarity.Common.ToString(), value = new IntRangeData { min = tuning.commonInitialSubstats.min, max = tuning.commonInitialSubstats.max } });
                masterData.artifactTuning.initialSubstatCounts.Add(new KVP_StringIntRange { key = ArtifactRarity.Uncommon.ToString(), value = new IntRangeData { min = tuning.uncommonInitialSubstats.min, max = tuning.uncommonInitialSubstats.max } });
                masterData.artifactTuning.initialSubstatCounts.Add(new KVP_StringIntRange { key = ArtifactRarity.Rare.ToString(), value = new IntRangeData { min = tuning.rareInitialSubstats.min, max = tuning.rareInitialSubstats.max } });
                masterData.artifactTuning.initialSubstatCounts.Add(new KVP_StringIntRange { key = ArtifactRarity.Epic.ToString(), value = new IntRangeData { min = tuning.epicInitialSubstats.min, max = tuning.epicInitialSubstats.max } });
                masterData.artifactTuning.initialSubstatCounts.Add(new KVP_StringIntRange { key = ArtifactRarity.Legendary.ToString(), value = new IntRangeData { min = tuning.legendaryInitialSubstats.min, max = tuning.legendaryInitialSubstats.max } });

                // Allowed Main Stats
                foreach (var res in tuning.slotRestrictions)
                {
                    masterData.artifactTuning.allowedMainStatsPerSlot.Add(new KVP_StringStringList
                    {
                        key = res.slot.ToString(),
                        value = res.allowedMainStats.Select(s => s.ToString()).ToList()
                    });
                }

                // Stat Ranges
                foreach (var ranges in tuning.statRanges)
                {
                    var stData = new StatTypeData { statType = ranges.statType.ToString() };
                    for (int i = 0; i < 6; i++)
                    {
                        stData.mainBaseByStars.Add(new FloatRangeData { min = ranges.mainBaseByStars[i].min, max = ranges.mainBaseByStars[i].max });
                        stData.mainUpgradeByStars.Add(new FloatRangeData { min = ranges.mainUpgradeByStars[i].min, max = ranges.mainUpgradeByStars[i].max });
                        stData.subInitialByStars.Add(new FloatRangeData { min = ranges.subInitialByStars[i].min, max = ranges.subInitialByStars[i].max });
                        stData.subUpgradeByStars.Add(new FloatRangeData { min = ranges.subUpgradeByStars[i].min, max = ranges.subUpgradeByStars[i].max });
                    }
                    masterData.artifactTuning.statRanges.Add(stData);
                }
            }

            // 4. Export Banners
            var bannerGuids = AssetDatabase.FindAssets("t:GachaBannerSO");
            foreach (var guid in bannerGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var banner = AssetDatabase.LoadAssetAtPath<GachaBannerSO>(path);
                if (banner != null)
                {
                    var bData = new GachaBannerConfigData
                    {
                        bannerId = banner.BannerID,
                        costPerPull = banner.CostPerPull,
                        softPityThreshold = banner.SoftPityThreshold,
                        hardPityThreshold = banner.HardPityThreshold,
                        guaranteedAboveBaseEvery = banner.GuaranteedAboveBaseEvery,
                        hasEpitomizedPath = banner.HasEpitomizedPath
                    };

                    foreach (var bp in banner.BasicProbabilities)
                    {
                        bData.basicProbabilities.Add(new KVP_StringFloat { key = bp.Rarity.ToString(), value = bp.BaseChance });
                    }

                    foreach (var sc in banner.SupremeChoices)
                    {
                        bData.supremeChoices.Add(new GachaRewardEntryData
                        {
                            rewardType = sc.RewardType.ToString(),
                            rarity = sc.Rarity.ToString(),
                            itemId = sc.GetID(),
                            weight = sc.Weight,
                            minItemStars = (int)sc.MinItemStars,
                            maxItemStars = (int)sc.MaxItemStars,
                            minArtifactRarity = (int)sc.MinArtifactRarity,
                            maxArtifactRarity = (int)sc.MaxArtifactRarity
                        });
                    }

                    foreach (var tp in banner.TotalPool)
                    {
                        bData.totalPool.Add(new GachaRewardEntryData
                        {
                            rewardType = tp.RewardType.ToString(),
                            rarity = tp.Rarity.ToString(),
                            itemId = tp.GetID(),
                            weight = tp.Weight,
                            minItemStars = (int)tp.MinItemStars,
                            maxItemStars = (int)tp.MaxItemStars,
                            minArtifactRarity = (int)tp.MinArtifactRarity,
                            maxArtifactRarity = (int)tp.MaxArtifactRarity
                        });
                    }

                    masterData.banners.Add(bData);
                }
            }

            // Write to file
            string json = JsonUtility.ToJson(masterData, true);
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string targetPath = Path.Combine(projectRoot, "Backend", "gamedata.json");
            
            // Ensure Backend dir exists
            if(!Directory.Exists(Path.Combine(projectRoot, "Backend")))
            {
                Directory.CreateDirectory(Path.Combine(projectRoot, "Backend"));
            }

            File.WriteAllText(targetPath, json);
            Debug.Log($"[GameDataExporter] Dados exportados com sucesso para: {targetPath}");
            EditorUtility.DisplayDialog("Sucesso", "Game Data exportado para o Backend/gamedata.json com sucesso!", "OK");
        }
    }
}
