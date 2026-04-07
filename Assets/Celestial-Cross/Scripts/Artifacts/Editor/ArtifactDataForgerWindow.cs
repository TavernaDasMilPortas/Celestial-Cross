using System;
using System.Collections.Generic;
using System.IO;
using CelestialCross.Artifacts;
using UnityEditor;
using UnityEngine;

namespace CelestialCross.Artifacts.Editor
{
    public class ArtifactDataForgerWindow : EditorWindow
    {
        // Forge inputs
        private ArtifactType selectedSlot;
        private ArtifactSet selectedSet;
        private ArtifactRarity selectedRarity;
        private ArtifactStars selectedStars = ArtifactStars.One;
        private StatType selectedMainStat;

        // Account target
        private bool useAccountManagerWhenAvailable = true;
        private string accountJsonPath;
        private Account workingAccount;

        // Optional equip
        private bool equipAfterForge;
        private string equipUnitId;

        private string lastLog;

        [MenuItem("Celestial Cross/Artifacts/Artifact Data Forger (Option B)")]
        public static void ShowWindow()
        {
            GetWindow<ArtifactDataForgerWindow>("Artifact Data Forger");
        }

        private void OnEnable()
        {
            if (string.IsNullOrWhiteSpace(accountJsonPath))
            {
                var defaultPath = Path.Combine(Application.persistentDataPath, "account.json");
                accountJsonPath = EditorPrefs.GetString("CC_ArtifactForger_AccountPath", defaultPath);
            }
        }

        private void OnDisable()
        {
            if (!string.IsNullOrWhiteSpace(accountJsonPath))
                EditorPrefs.SetString("CC_ArtifactForger_AccountPath", accountJsonPath);
        }

        private void OnGUI()
        {
            GUILayout.Label("Artifact Forger (Option B: Save Data)", EditorStyles.boldLabel);

            DrawAccountSection();
            GUILayout.Space(10);
            DrawForgeSection();
            GUILayout.Space(10);
            DrawEquipSection();

            GUILayout.Space(12);

            GUI.enabled = CanForge();
            if (GUILayout.Button("Forge Artifact Into Account", GUILayout.Height(40)))
            {
                ForgeIntoAccount();
            }
            GUI.enabled = true;

            if (!string.IsNullOrEmpty(lastLog))
            {
                EditorGUILayout.HelpBox(lastLog, MessageType.Info);
            }
        }

        private void DrawAccountSection()
        {
            GUILayout.Label("Target Account", EditorStyles.boldLabel);

            bool accountManagerAvailable = AccountManager.Instance != null && AccountManager.Instance.PlayerAccount != null;
            usingAccountManagerGUI(accountManagerAvailable);

            if (!useAccountManagerWhenAvailable || !accountManagerAvailable)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label("Edit JSON directly (works in Edit Mode)");

                EditorGUILayout.BeginHorizontal();
                accountJsonPath = EditorGUILayout.TextField("Account JSON", accountJsonPath);
                if (GUILayout.Button("Pick", GUILayout.Width(50)))
                {
                    string picked = EditorUtility.OpenFilePanel("Select account.json", Application.persistentDataPath, "json");
                    if (!string.IsNullOrWhiteSpace(picked))
                        accountJsonPath = picked;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Load"))
                {
                    LoadAccountFromDisk();
                }
                GUI.enabled = workingAccount != null;
                if (GUILayout.Button("Save"))
                {
                    SaveAccountToDisk();
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();

                if (workingAccount != null)
                {
                    GUILayout.Label($"Loaded artifacts: {workingAccount.OwnedArtifacts?.Count ?? 0}");
                    GUILayout.Label($"Loaded loadouts: {workingAccount.UnitLoadouts?.Count ?? 0}");
                }
                else
                {
                    EditorGUILayout.HelpBox("No account loaded yet. Click Load, or use AccountManager in Play Mode.", MessageType.Warning);
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void usingAccountManagerGUI(bool accountManagerAvailable)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            useAccountManagerWhenAvailable = EditorGUILayout.ToggleLeft(
                new GUIContent("Use AccountManager when available (Play Mode)", "If AccountManager.Instance exists, the artifact is added to the in-memory account and saved via AccountManager.SaveAccount()."),
                useAccountManagerWhenAvailable);

            if (accountManagerAvailable && useAccountManagerWhenAvailable)
            {
                GUILayout.Label("AccountManager detected. Artifacts will be added to the live account.");
                GUILayout.Label($"Artifacts in account: {AccountManager.Instance.PlayerAccount.OwnedArtifacts?.Count ?? 0}");
            }
            else if (accountManagerAvailable && !useAccountManagerWhenAvailable)
            {
                GUILayout.Label("AccountManager detected, but disabled. Using JSON file instead.");
            }
            else
            {
                GUILayout.Label("AccountManager not available (not in Play Mode or missing in scene). Using JSON file instead.");
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawForgeSection()
        {
            GUILayout.Label("Forge Inputs", EditorStyles.boldLabel);

            selectedSlot = (ArtifactType)EditorGUILayout.EnumPopup("Slot Type", selectedSlot);
            selectedSet = (ArtifactSet)EditorGUILayout.ObjectField("Artifact Set", selectedSet, typeof(ArtifactSet), false);
            selectedRarity = (ArtifactRarity)EditorGUILayout.EnumPopup("Rarity", selectedRarity);
            selectedStars = (ArtifactStars)EditorGUILayout.EnumPopup("Stars", selectedStars);
            selectedMainStat = (StatType)EditorGUILayout.EnumPopup("Forced Main Stat", selectedMainStat);

            if (selectedSet != null && string.IsNullOrWhiteSpace(selectedSet.id))
            {
                EditorGUILayout.HelpBox("Selected ArtifactSet has empty 'id'. Set bonuses/passives will not resolve at runtime until you set ArtifactSet.id.", MessageType.Warning);
            }
        }

        private void DrawEquipSection()
        {
            GUILayout.Label("Optional: Equip After Forge", EditorStyles.boldLabel);

            equipAfterForge = EditorGUILayout.Toggle("Equip into UnitLoadout", equipAfterForge);
            using (new EditorGUI.DisabledScope(!equipAfterForge))
            {
                equipUnitId = EditorGUILayout.TextField("UnitID", equipUnitId);
                EditorGUILayout.HelpBox("This sets the GUID into the matching slot field in UnitLoadout (Helmet/Chest/etc).", MessageType.None);
            }
        }

        private bool CanForge()
        {
            int starsInt = (int)selectedStars;
            if (starsInt < 1 || starsInt > 6) return false;

            // Account must be accessible either via AccountManager or loaded JSON.
            if (useAccountManagerWhenAvailable && AccountManager.Instance != null && AccountManager.Instance.PlayerAccount != null)
                return true;

            return workingAccount != null;
        }

        private void ForgeIntoAccount()
        {
            var account = GetTargetAccount();
            if (account == null)
            {
                lastLog = "No target account available.";
                return;
            }

            account.EnsureInitialized();

            var artifact = new ArtifactInstanceData
            {
                artifactSetId = selectedSet != null ? selectedSet.id : string.Empty,
                slot = selectedSlot,
                rarity = selectedRarity,
                stars = selectedStars,
                currentLevel = 1,
                mainStat = new StatModifierData(selectedMainStat, ArtifactGenerator.GetMainStatBaseValue(selectedMainStat, selectedStars)),
                subStats = new List<StatModifierData>()
            };

            // Initial substats
            int substatsCountToGenerate = ArtifactGenerator.GetInitialSubstatCount(selectedRarity);
            var existingSubstats = new List<StatModifier>();

            for (int i = 0; i < substatsCountToGenerate; i++)
            {
                StatType rolledType = ArtifactGenerator.GetRandomSubstatType(selectedMainStat, existingSubstats);
                float rolledValue = ArtifactGenerator.GenerateSubstatValue(rolledType, selectedStars);
                artifact.subStats.Add(new StatModifierData(rolledType, rolledValue));
                existingSubstats.Add(new StatModifier(rolledType, rolledValue));
            }

            account.OwnedArtifacts.Add(artifact);

            if (equipAfterForge && !string.IsNullOrWhiteSpace(equipUnitId))
            {
                var loadout = account.GetLoadoutForUnit(equipUnitId);
                EquipGuidToLoadout(loadout, selectedSlot, artifact.idGUID);
            }

            SaveTargetAccount();

            lastLog = $"Forged ArtifactData GUID={artifact.idGUID}\nSlot={artifact.slot} | Rarity={artifact.rarity} | Stars={artifact.GetStarsAsIntClamped()} | SetId='{artifact.artifactSetId}'\nMain={artifact.mainStat.statType}+{artifact.mainStat.value:F0} | Substats={artifact.subStats.Count}";
        }

        private Account GetTargetAccount()
        {
            if (useAccountManagerWhenAvailable && AccountManager.Instance != null && AccountManager.Instance.PlayerAccount != null)
                return AccountManager.Instance.PlayerAccount;

            return workingAccount;
        }

        private void SaveTargetAccount()
        {
            if (useAccountManagerWhenAvailable && AccountManager.Instance != null && AccountManager.Instance.PlayerAccount != null)
            {
                AccountManager.Instance.SaveAccount();
                return;
            }

            SaveAccountToDisk();
        }

        private void LoadAccountFromDisk()
        {
            try
            {
                if (File.Exists(accountJsonPath))
                {
                    string json = File.ReadAllText(accountJsonPath);
                    workingAccount = JsonUtility.FromJson<Account>(json);
                    if (workingAccount == null)
                        workingAccount = new Account();
                }
                else
                {
                    workingAccount = new Account();
                }

                workingAccount.EnsureInitialized();
                lastLog = $"Loaded account from: {accountJsonPath}";
            }
            catch (Exception ex)
            {
                workingAccount = null;
                lastLog = $"Failed to load account: {ex.Message}";
            }
        }

        private void SaveAccountToDisk()
        {
            if (workingAccount == null)
            {
                lastLog = "No working account loaded.";
                return;
            }

            try
            {
                workingAccount.EnsureInitialized();
                string json = JsonUtility.ToJson(workingAccount, true);
                File.WriteAllText(accountJsonPath, json);
                lastLog = $"Saved account to: {accountJsonPath}";
            }
            catch (Exception ex)
            {
                lastLog = $"Failed to save account: {ex.Message}";
            }
        }

        private static void EquipGuidToLoadout(UnitLoadout loadout, ArtifactType slot, string guid)
        {
            if (loadout == null) return;

            switch (slot)
            {
                case ArtifactType.Helmet: loadout.HelmetID = guid; break;
                case ArtifactType.Chestplate: loadout.ChestplateID = guid; break;
                case ArtifactType.Gloves: loadout.GlovesID = guid; break;
                case ArtifactType.Boots: loadout.BootsID = guid; break;
                case ArtifactType.Necklace: loadout.NecklaceID = guid; break;
                case ArtifactType.Ring: loadout.RingID = guid; break;
                default: break;
            }
        }
    }
}
