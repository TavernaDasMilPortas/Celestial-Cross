using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace CelestialCross.Scenes.Unit
{
    public class UnitDetailPanel_Attributes : MonoBehaviour, IPointerClickHandler
    {
        public TextMeshProUGUI hpText;
        public TextMeshProUGUI atkText;
        public TextMeshProUGUI defText;
        public TextMeshProUGUI spdText;
        
        public TextMeshProUGUI critRateText;
        public TextMeshProUGUI critDmgText;
        public TextMeshProUGUI effectAccText;
        public TextMeshProUGUI effectResText;

        public ArtifactSetCatalog artifactSetCatalog;

        private bool showDetailedStats = false;
        private UnitData lastUnitData;
        private CelestialCross.Data.RuntimeUnitData lastRuntimeData;

        public void OnPointerClick(PointerEventData eventData)
        {
            showDetailedStats = !showDetailedStats;
            Refresh(lastUnitData, lastRuntimeData);
        }

        public void Refresh(UnitData unitData, CelestialCross.Data.RuntimeUnitData runtimeData)
        {
            if (unitData == null || runtimeData == null) return;
            
            lastUnitData = unitData;
            lastRuntimeData = runtimeData;

            var baseStats = unitData.GetStatsAtLevel(runtimeData.Level, 100);
            
            float hF = 0, hP = 0, aF = 0, aP = 0, dF = 0, dP = 0, spdF = 0, crF = 0, eaf = 0, cdF = 0, erf = 0;

            var account = global::AccountManager.Instance?.PlayerAccount;
            CelestialCross.Data.Pets.RuntimePetData equippedPet = null;
            if (account != null)
            {
                var loadout = account.GetLoadoutForUnit(runtimeData.UnitID);
                if (loadout != null)
                {
                    if (!string.IsNullOrEmpty(loadout.PetID))
                    {
                        equippedPet = account.GetPetByUUID(loadout.PetID);
                    }

                    var artifactIDs = loadout.GetEquippedArtifactIDs();
                    var setCounts = new Dictionary<string, int>();

                    foreach (var guid in artifactIDs)
                    {
                        var arti = account.GetArtifactByGuid(guid);
                        if (arti != null)
                        {
                            ProcessStatData(arti.mainStat, ref hF, ref hP, ref aF, ref aP, ref dF, ref dP, ref spdF, ref crF, ref eaf, ref cdF, ref erf);
                            if (arti.subStats != null)
                            {
                                foreach (var sub in arti.subStats)
                                    ProcessStatData(sub, ref hF, ref hP, ref aF, ref aP, ref dF, ref dP, ref spdF, ref crF, ref eaf, ref cdF, ref erf);
                            }

                            if (!string.IsNullOrEmpty(arti.artifactSetId))
                            {
                                if (!setCounts.ContainsKey(arti.artifactSetId)) setCounts[arti.artifactSetId] = 0;
                                setCounts[arti.artifactSetId]++;
                            }
                        }
                    }

                    if (artifactSetCatalog != null)
                    {
                        foreach (var kvp in setCounts)
                        {
                            var set = artifactSetCatalog.GetSetById(kvp.Key);
                            if (set == null) continue;

                            foreach (var bonus in set.setBonuses)
                            {
                                if (kvp.Value >= bonus.piecesRequired && bonus.statBonuses != null)
                                {
                                    foreach (var statMod in bonus.statBonuses)
                                    {
                                        ProcessStatData(statMod, ref hF, ref hP, ref aF, ref aP, ref dF, ref dP, ref spdF, ref crF, ref eaf, ref cdF, ref erf);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            int finalHealth = (int)Mathf.Round(baseStats.health * (1f + (hP / 100f)) + hF);
            int finalAttack = (int)Mathf.Round(baseStats.attack * (1f + (aP / 100f)) + aF);
            int finalDefense = (int)Mathf.Round(baseStats.defense * (1f + (dP / 100f)) + dF);
            int finalSpeed = (int)Mathf.Round(baseStats.speed + spdF);
            int finalCrit = Mathf.Clamp((int)Mathf.Round(baseStats.criticalChance + crF), 0, 100);
            int finalCritDmg = Mathf.Max(50, (int)Mathf.Round(baseStats.criticalDamage + cdF)); // Base 50%
            int finalAcc = Mathf.Clamp((int)Mathf.Round(baseStats.effectAccuracy + eaf), 0, 100);
            int finalRes = Mathf.Clamp((int)Mathf.Round(baseStats.effectResistance + erf), 0, 100);

            if (equippedPet != null)
            {
                finalHealth += equippedPet.Health;
                finalAttack += equippedPet.Attack;
                finalDefense += equippedPet.Defense;
                finalSpeed += equippedPet.Speed;
                finalCrit = Mathf.Clamp(finalCrit + equippedPet.CriticalChance, 0, 100);
                finalCritDmg = Mathf.Max(50, finalCritDmg + equippedPet.CriticalDamage);
                finalAcc = Mathf.Clamp(finalAcc + equippedPet.EffectAccuracy, 0, 100);
                finalRes = Mathf.Clamp(finalRes + equippedPet.EffectResistance, 0, 100);
            }

            int roundedBaseHealth = Mathf.RoundToInt(baseStats.health);
            int roundedBaseAttack = Mathf.RoundToInt(baseStats.attack);
            int roundedBaseDefense = Mathf.RoundToInt(baseStats.defense);
            int roundedBaseSpeed = Mathf.RoundToInt(baseStats.speed);
            int roundedBaseCrit = Mathf.RoundToInt(baseStats.criticalChance);
            int roundedBaseCritDmg = Mathf.RoundToInt(baseStats.criticalDamage);
            int roundedBaseAcc = Mathf.RoundToInt(baseStats.effectAccuracy);
            int roundedBaseRes = Mathf.RoundToInt(baseStats.effectResistance);

            if (showDetailedStats)
            {
                int hBonus = finalHealth - roundedBaseHealth;
                int aBonus = finalAttack - roundedBaseAttack;
                int dBonus = finalDefense - roundedBaseDefense;
                int sBonus = finalSpeed - roundedBaseSpeed;
                int crBonus = finalCrit - roundedBaseCrit;
                int cdBonus = finalCritDmg - roundedBaseCritDmg;
                int accBonus = finalAcc - roundedBaseAcc;
                int resBonus = finalRes - roundedBaseRes;

                if (hpText != null) hpText.text = hBonus > 0 ? $"HP: {roundedBaseHealth} <color=#ffff00>+{hBonus}</color>" : $"HP: {roundedBaseHealth}";
                if (atkText != null) atkText.text = aBonus > 0 ? $"ATK: {roundedBaseAttack} <color=#ffff00>+{aBonus}</color>" : $"ATK: {roundedBaseAttack}";
                if (defText != null) defText.text = dBonus > 0 ? $"DEF: {roundedBaseDefense} <color=#ffff00>+{dBonus}</color>" : $"DEF: {roundedBaseDefense}";
                if (spdText != null) spdText.text = sBonus > 0 ? $"SPD: {roundedBaseSpeed} <color=#ffff00>+{sBonus}</color>" : $"SPD: {roundedBaseSpeed}";

                if (critRateText != null) critRateText.text = crBonus > 0 ? $"C.RATE: {roundedBaseCrit}% <color=#ffff00>+{crBonus}%</color>" : $"C.RATE: {roundedBaseCrit}%";
                if (critDmgText != null) critDmgText.text = cdBonus > 0 ? $"C.DMG: {roundedBaseCritDmg}% <color=#ffff00>+{cdBonus}%</color>" : $"C.DMG: {roundedBaseCritDmg}%";
                if (effectAccText != null) effectAccText.text = accBonus > 0 ? $"ACC: {roundedBaseAcc}% <color=#ffff00>+{accBonus}%</color>" : $"ACC: {roundedBaseAcc}%";
                if (effectResText != null) effectResText.text = resBonus > 0 ? $"RES: {roundedBaseRes}% <color=#ffff00>+{resBonus}%</color>" : $"RES: {roundedBaseRes}%";
            }
            else
            {
                if (hpText != null) hpText.text = finalHealth > roundedBaseHealth ? $"HP: <color=#00ff00>{finalHealth}</color>" : $"HP: {finalHealth}";
                if (atkText != null) atkText.text = finalAttack > roundedBaseAttack ? $"ATK: <color=#00ff00>{finalAttack}</color>" : $"ATK: {finalAttack}";
                if (defText != null) defText.text = finalDefense > roundedBaseDefense ? $"DEF: <color=#00ff00>{finalDefense}</color>" : $"DEF: {finalDefense}";
                if (spdText != null) spdText.text = finalSpeed > roundedBaseSpeed ? $"SPD: <color=#00ff00>{finalSpeed}</color>" : $"SPD: {finalSpeed}";

                if (critRateText != null) critRateText.text = finalCrit > roundedBaseCrit ? $"C.RATE: <color=#00ff00>{finalCrit}%</color>" : $"C.RATE: {finalCrit}%";
                if (critDmgText != null) critDmgText.text = finalCritDmg > roundedBaseCritDmg ? $"C.DMG: <color=#00ff00>{finalCritDmg}%</color>" : $"C.DMG: {finalCritDmg}%";
                if (effectAccText != null) effectAccText.text = finalAcc > roundedBaseAcc ? $"ACC: <color=#00ff00>{finalAcc}%</color>" : $"ACC: {finalAcc}%";
                if (effectResText != null) effectResText.text = finalRes > roundedBaseRes ? $"RES: <color=#00ff00>{finalRes}%</color>" : $"RES: {finalRes}%";
            }
        }

        private void ProcessStatData(CelestialCross.Artifacts.StatModifierData stat, ref float hF, ref float hP, ref float aF, ref float aP, ref float dF, ref float dP, ref float spdF, ref float crF, ref float eaf, ref float cdF, ref float erf)
        {
            if (stat == null) return;
            switch (stat.statType)
            {
                case CelestialCross.Artifacts.StatType.HealthFlat: hF += stat.value; break;
                case CelestialCross.Artifacts.StatType.HealthPercent:  hP += stat.value; break;
                case CelestialCross.Artifacts.StatType.AttackFlat: aF += stat.value; break;
                case CelestialCross.Artifacts.StatType.AttackPercent:  aP += stat.value; break;
                case CelestialCross.Artifacts.StatType.DefenseFlat: dF += stat.value; break;
                case CelestialCross.Artifacts.StatType.DefensePercent: dP += stat.value; break;
                case CelestialCross.Artifacts.StatType.Speed:  spdF += stat.value; break;
                case CelestialCross.Artifacts.StatType.CriticalRate: crF += stat.value; break;
                case CelestialCross.Artifacts.StatType.CriticalDamage: cdF += stat.value; break;
                case CelestialCross.Artifacts.StatType.EffectHitRate: eaf += stat.value; break;
                case CelestialCross.Artifacts.StatType.EffectResistance: erf += stat.value; break;
            }
        }

        private void ProcessStatData(CelestialCross.Artifacts.StatModifier stat, ref float hF, ref float hP, ref float aF, ref float aP, ref float dF, ref float dP, ref float spdF, ref float crF, ref float eaf, ref float cdF, ref float erf)
        {
            switch (stat.statType)
            {
                case CelestialCross.Artifacts.StatType.HealthFlat: hF += stat.value; break;
                case CelestialCross.Artifacts.StatType.HealthPercent:  hP += stat.value; break;
                case CelestialCross.Artifacts.StatType.AttackFlat: aF += stat.value; break;
                case CelestialCross.Artifacts.StatType.AttackPercent:  aP += stat.value; break;
                case CelestialCross.Artifacts.StatType.DefenseFlat: dF += stat.value; break;
                case CelestialCross.Artifacts.StatType.DefensePercent: dP += stat.value; break;
                case CelestialCross.Artifacts.StatType.Speed:  spdF += stat.value; break;
                case CelestialCross.Artifacts.StatType.CriticalRate: crF += stat.value; break;
                case CelestialCross.Artifacts.StatType.CriticalDamage: cdF += stat.value; break;
                case CelestialCross.Artifacts.StatType.EffectHitRate: eaf += stat.value; break;
                case CelestialCross.Artifacts.StatType.EffectResistance: erf += stat.value; break;
            }
        }
    }
}
