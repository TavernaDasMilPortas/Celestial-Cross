using System;
using System.Collections.Generic;
using System.IO;
using CelestialCross.Artifacts;
using UnityEditor;
using UnityEngine;

namespace CelestialCross.Artifacts.Editor
{
    public class ArtifactDataUpgraderWindow : EditorWindow
    {
        private bool useAccountManagerWhenAvailable = true;
        private string accountJsonPath;
        private Account workingAccount;

        private int selectedArtifactIndex;
        private string lastLog;

        [MenuItem("Celestial Cross/Artifacts/Artifact Data Upgrader (Option B)")]
        public static void ShowWindow()
        {
            GetWindow<ArtifactDataUpgraderWindow>("Artifact Data Upgrader");
        }

        private void OnEnable()
        {
            if (string.IsNullOrWhiteSpace(accountJsonPath))
            {
                var defaultPath = Path.Combine(Application.persistentDataPath, "account.json");
                accountJsonPath = EditorPrefs.GetString("CC_ArtifactUpgrader_AccountPath", defaultPath);
            }
        }

        private void OnDisable()
        {
            if (!string.IsNullOrWhiteSpace(accountJsonPath))
                EditorPrefs.SetString("CC_ArtifactUpgrader_AccountPath", accountJsonPath);
        }

        private void OnGUI()
        {
            GUILayout.Label("Artifact Upgrader (Option B: Save Data)", EditorStyles.boldLabel);

            DrawAccountSection();

            var account = GetTargetAccount();
            if (account == null)
            {
                EditorGUILayout.HelpBox("No account available. Use AccountManager in Play Mode or load JSON.", MessageType.Warning);
                DrawLog();
                return;
            }

            account.EnsureInitialized();

            if (account.OwnedArtifacts == null || account.OwnedArtifacts.Count == 0)
            {
                EditorGUILayout.HelpBox("Account has no OwnedArtifacts. Forge some first.", MessageType.Info);
                DrawLog();
                return;
            }

            DrawArtifactPicker(account);

            var artifact = GetSelectedArtifact(account);
            if (artifact == null)
            {
                EditorGUILayout.HelpBox("Selected artifact is null.", MessageType.Warning);
                DrawLog();
                return;
            }

            DrawArtifactInfo(artifact);

            GUILayout.Space(12);

            bool canLevelUp = artifact.currentLevel < 15;
            using (new EditorGUI.DisabledScope(!canLevelUp))
            {
                if (GUILayout.Button($"Level Up to +{artifact.currentLevel + 1}", GUILayout.Height(40)))
                {
                    PerformLevelUp(account, artifact);
                }
            }

            DrawLog();
        }

        private void DrawAccountSection()
        {
            GUILayout.Label("Target Account", EditorStyles.boldLabel);

            bool accountManagerAvailable = AccountManager.Instance != null && AccountManager.Instance.PlayerAccount != null;

            EditorGUILayout.BeginVertical(GUI.skin.box);
            useAccountManagerWhenAvailable = EditorGUILayout.ToggleLeft(
                new GUIContent("Use AccountManager when available (Play Mode)", "If AccountManager.Instance exists, upgrades are applied to the live account and saved via AccountManager.SaveAccount()."),
                useAccountManagerWhenAvailable);

            if (accountManagerAvailable && useAccountManagerWhenAvailable)
            {
                GUILayout.Label($"AccountManager detected. Artifacts: {AccountManager.Instance.PlayerAccount.OwnedArtifacts?.Count ?? 0}");
            }
            else
            {
                GUILayout.Label("Using JSON file (Edit Mode compatible)");

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
                    GUILayout.Label($"Loaded artifacts: {workingAccount.OwnedArtifacts?.Count ?? 0}");
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawArtifactPicker(Account account)
        {
            GUILayout.Label("Select Artifact", EditorStyles.boldLabel);

            var options = new string[account.OwnedArtifacts.Count];
            for (int i = 0; i < account.OwnedArtifacts.Count; i++)
            {
                var a = account.OwnedArtifacts[i];
                if (a == null)
                {
                    options[i] = $"[{i}] <null>";
                    continue;
                }

                string setLabel = string.IsNullOrWhiteSpace(a.artifactSetId) ? "NoSet" : a.artifactSetId;
                options[i] = $"[{i}] {a.slot} {a.rarity} {a.stars}* +{a.currentLevel} ({setLabel})";
            }

            selectedArtifactIndex = Mathf.Clamp(selectedArtifactIndex, 0, account.OwnedArtifacts.Count - 1);
            selectedArtifactIndex = EditorGUILayout.Popup("Artifact", selectedArtifactIndex, options);
        }

        private ArtifactInstanceData GetSelectedArtifact(Account account)
        {
            if (account?.OwnedArtifacts == null || account.OwnedArtifacts.Count == 0)
                return null;

            if (selectedArtifactIndex < 0 || selectedArtifactIndex >= account.OwnedArtifacts.Count)
                return null;

            return account.OwnedArtifacts[selectedArtifactIndex];
        }

        private void DrawArtifactInfo(ArtifactInstanceData artifact)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            GUILayout.Label($"GUID: {artifact.idGUID}");
            GUILayout.Label($"Slot: {artifact.slot} | Rarity: {artifact.rarity} | Stars: {artifact.stars}*");
            GUILayout.Label($"Level: +{artifact.currentLevel} / +15");
            GUILayout.Label($"SetId: {(string.IsNullOrWhiteSpace(artifact.artifactSetId) ? "<none>" : artifact.artifactSetId)}");

            GUILayout.Space(6);

            if (artifact.mainStat != null)
                GUILayout.Label($"Main Stat: {artifact.mainStat.statType} +{artifact.mainStat.value:F0}", EditorStyles.boldLabel);
            else
                EditorGUILayout.HelpBox("Main stat is null. This artifact data is invalid.", MessageType.Error);

            GUILayout.Space(6);

            GUILayout.Label("Substats:", EditorStyles.boldLabel);
            if (artifact.subStats == null || artifact.subStats.Count == 0)
            {
                GUILayout.Label("(none)");
            }
            else
            {
                for (int i = 0; i < artifact.subStats.Count; i++)
                {
                    var s = artifact.subStats[i];
                    if (s == null) continue;
                    GUILayout.Label($"- {s.statType} +{s.value:F0}");
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void PerformLevelUp(Account account, ArtifactInstanceData artifact)
        {
            if (artifact == null) return;
            if (artifact.currentLevel >= 15)
            {
                lastLog = "Artifact already at max level (+15).";
                return;
            }

            if (artifact.mainStat == null)
            {
                lastLog = "Cannot upgrade: mainStat is null.";
                return;
            }

            artifact.currentLevel++;

            // 1) Fixed main stat increment
            float upgradeMainIncrement = ArtifactGenerator.GetMainStatUpgradeIncrement(artifact.mainStat.statType, artifact.stars);
            artifact.mainStat.value += upgradeMainIncrement;

            lastLog = $"Main stat +{upgradeMainIncrement:F0}.";

            // Ensure list exists
            artifact.subStats ??= new List<StatModifierData>();

            // 2) Every 3 levels: add or roll substat
            if (artifact.currentLevel % 3 == 0)
            {
                if (artifact.subStats.Count < 4)
                {
                    var currentSubMods = new List<StatModifier>();
                    for (int i = 0; i < artifact.subStats.Count; i++)
                    {
                        var s = artifact.subStats[i];
                        if (s != null) currentSubMods.Add(new StatModifier(s.statType, s.value));
                    }

                    StatType newType = ArtifactGenerator.GetRandomSubstatType(artifact.mainStat.statType, currentSubMods);
                    float startSubValue = ArtifactGenerator.GenerateSubstatValue(newType, artifact.stars);

                    artifact.subStats.Add(new StatModifierData(newType, startSubValue));
                    lastLog += $"\nNew substat born: {newType} +{startSubValue:F0}";
                }
                else
                {
                    int randomIndex = UnityEngine.Random.Range(0, 4);
                    var target = artifact.subStats[randomIndex];
                    if (target == null)
                    {
                        // repair null entry
                        target = new StatModifierData(StatType.HealthFlat, 0);
                        artifact.subStats[randomIndex] = target;
                    }

                    float upgradeRngIncrement = ArtifactGenerator.GetSubstatUpgradeIncrement(target.statType, artifact.stars);
                    target.value += upgradeRngIncrement;

                    lastLog += $"\nProc at +{artifact.currentLevel}: '{target.statType}' +{upgradeRngIncrement:F0}.";
                }
            }

            SaveTargetAccount(account);
        }

        private void DrawLog()
        {
            if (!string.IsNullOrEmpty(lastLog))
                EditorGUILayout.HelpBox(lastLog, MessageType.Info);
        }

        private Account GetTargetAccount()
        {
            if (useAccountManagerWhenAvailable && AccountManager.Instance != null && AccountManager.Instance.PlayerAccount != null)
                return AccountManager.Instance.PlayerAccount;

            return workingAccount;
        }

        private void SaveTargetAccount(Account account)
        {
            if (useAccountManagerWhenAvailable && AccountManager.Instance != null && AccountManager.Instance.PlayerAccount != null)
            {
                AccountManager.Instance.SaveAccount();
                return;
            }

            // Ensure we're saving the same object
            if (workingAccount == null)
                workingAccount = account;

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
                selectedArtifactIndex = 0;
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
    }
}
