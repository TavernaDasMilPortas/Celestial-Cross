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

        private string upgradeConsole = "";
        private Vector2 consoleScroll;
        private bool scrollConsoleToBottom;

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
            GUILayout.Label("Upgrader de Artefatos (Opção B: Save Data)", EditorStyles.boldLabel);

            DrawAccountSection();

            var account = GetTargetAccount();
            if (account == null)
            {
                EditorGUILayout.HelpBox("Nenhuma conta disponível. Use AccountManager no Play Mode ou carregue o JSON.", MessageType.Warning);
                DrawConsole();
                return;
            }

            account.EnsureInitialized();

            if (account.OwnedArtifacts == null || account.OwnedArtifacts.Count == 0)
            {
                EditorGUILayout.HelpBox("A conta não tem OwnedArtifacts. Forge alguns primeiro.", MessageType.Info);
                DrawConsole();
                return;
            }

            DrawArtifactPicker(account);

            var artifact = GetSelectedArtifact(account);
            if (artifact == null)
            {
                EditorGUILayout.HelpBox("O artefato selecionado é null.", MessageType.Warning);
                DrawConsole();
                return;
            }

            RenderArtifactInfo(artifact);

            GUILayout.Space(12);

            bool canLevelUp = artifact.currentLevel < 15;
            using (new EditorGUI.DisabledScope(!canLevelUp))
            {
                if (GUILayout.Button($"Level Up para +{artifact.currentLevel + 1}", GUILayout.Height(40)))
                {
                    PerformLevelUp(account, artifact);
                }
            }

            DrawConsole();
        }

        private void DrawAccountSection()
        {
            GUILayout.Label("Conta Alvo", EditorStyles.boldLabel);

            bool accountManagerAvailable = AccountManager.Instance != null && AccountManager.Instance.PlayerAccount != null;

            EditorGUILayout.BeginVertical(GUI.skin.box);
            useAccountManagerWhenAvailable = EditorGUILayout.ToggleLeft(
                new GUIContent("Usar AccountManager quando disponível (Play Mode)", "Se AccountManager.Instance existir, os upgrades são aplicados na conta em memória e salvos via AccountManager.SaveAccount()."),
                useAccountManagerWhenAvailable);

            if (accountManagerAvailable && useAccountManagerWhenAvailable)
            {
                GUILayout.Label($"AccountManager detectado. Artefatos: {AccountManager.Instance.PlayerAccount.OwnedArtifacts?.Count ?? 0}");
            }
            else
            {
                GUILayout.Label("Usando arquivo JSON (compatível com Edit Mode)");

                EditorGUILayout.BeginHorizontal();
                accountJsonPath = EditorGUILayout.TextField("Account JSON", accountJsonPath);
                if (GUILayout.Button("Escolher", GUILayout.Width(70)))
                {
                    string picked = EditorUtility.OpenFilePanel("Select account.json", Application.persistentDataPath, "json");
                    if (!string.IsNullOrWhiteSpace(picked))
                        accountJsonPath = picked;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Carregar"))
                {
                    LoadAccountFromDisk();
                }
                GUI.enabled = workingAccount != null;
                if (GUILayout.Button("Salvar"))
                {
                    SaveAccountToDisk();
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();

                if (workingAccount != null)
                    GUILayout.Label($"Artefatos carregados: {workingAccount.OwnedArtifacts?.Count ?? 0}");
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawArtifactPicker(Account account)
        {
            GUILayout.Label("Selecionar Artefato", EditorStyles.boldLabel);

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
                options[i] = $"[{i}] {a.slot} {a.rarity} {a.GetStarsAsIntClamped()}* +{a.currentLevel} ({setLabel})";
            }

            selectedArtifactIndex = Mathf.Clamp(selectedArtifactIndex, 0, account.OwnedArtifacts.Count - 1);
            selectedArtifactIndex = EditorGUILayout.Popup("Artefato", selectedArtifactIndex, options);
        }

        private ArtifactInstanceData GetSelectedArtifact(Account account)
        {
            if (account?.OwnedArtifacts == null || account.OwnedArtifacts.Count == 0)
                return null;

            if (selectedArtifactIndex < 0 || selectedArtifactIndex >= account.OwnedArtifacts.Count)
                return null;

            return account.OwnedArtifacts[selectedArtifactIndex];
        }

        private void RenderArtifactInfo(ArtifactInstanceData artifact)
        {
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 10, 10)
            };

            GUILayout.BeginVertical(boxStyle);

            string setLabel = string.IsNullOrWhiteSpace(artifact.artifactSetId) ? "<sem set>" : artifact.artifactSetId;
            GUILayout.Label($"Raridade: {artifact.rarity}  |  Estrelas: {artifact.GetStarsAsIntClamped()}*", EditorStyles.label);
            GUILayout.Label($"Nível Atual: +{artifact.currentLevel}", EditorStyles.boldLabel);
            GUILayout.Label($"Slot: {artifact.slot}  |  SetId: {setLabel}");
            GUILayout.Label($"GUID: {artifact.idGUID}");

            GUILayout.Space(6);

            if (artifact.mainStat != null)
                GUILayout.Label($"Atributo Principal: {artifact.mainStat.statType} +{artifact.mainStat.value:F0}");
            else
                EditorGUILayout.HelpBox("Main stat está null. Este ArtifactInstanceData está inválido.", MessageType.Error);

            GUILayout.Space(5);
            GUILayout.Label("Sub-Atributos Obtidos:", EditorStyles.boldLabel);
            if (artifact.subStats == null || artifact.subStats.Count == 0)
            {
                GUILayout.Label("(nenhum)");
            }
            else
            {
                for (int i = 0; i < artifact.subStats.Count; i++)
                {
                    var s = artifact.subStats[i];
                    if (s == null) continue;
                    GUILayout.Label($" - {s.statType} +{s.value:F0}");
                }
            }

            GUILayout.EndVertical();
        }

        private void PerformLevelUp(Account account, ArtifactInstanceData artifact)
        {
            if (artifact == null) return;
            if (artifact.currentLevel >= 15)
            {
                AppendToConsole("Artefato já está no nível máximo (+15).");
                return;
            }

            if (artifact.mainStat == null)
            {
                AppendToConsole("Não foi possível upar: mainStat está null.");
                return;
            }

            artifact.currentLevel++;

            // 1) Fixed main stat increment
            float upgradeMainIncrement = ArtifactGenerator.GetMainStatUpgradeIncrement(artifact.mainStat.statType, artifact.stars);
            artifact.mainStat.value += upgradeMainIncrement;

            string upgradeLog = $"Atributo Base subiu +{upgradeMainIncrement:F0}!";

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
                    upgradeLog += $"\n{artifact.subStats.Count}º Substat nascido: {newType} +{startSubValue:F0}";
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

                    upgradeLog += $"\nEvento Level {artifact.currentLevel}! Substat '{target.statType}' tomou proc RNG e ganhou +{upgradeRngIncrement:F0} na barra!";
                }
            }

            AppendToConsole($"+{artifact.currentLevel}: {upgradeLog}");

            SaveTargetAccount(account);
        }

        private void DrawConsole()
        {
            GUILayout.Space(10);

            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Console de Upgrades", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Limpar Console", GUILayout.Width(120)))
            {
                upgradeConsole = "";
                lastLog = null;
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(lastLog))
                EditorGUILayout.HelpBox(lastLog, MessageType.Info);

            consoleScroll = EditorGUILayout.BeginScrollView(consoleScroll, GUILayout.MinHeight(140));
            EditorGUILayout.TextArea(string.IsNullOrEmpty(upgradeConsole) ? "(sem mensagens ainda)" : upgradeConsole, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            if (scrollConsoleToBottom)
            {
                consoleScroll.y = float.MaxValue;
                scrollConsoleToBottom = false;
            }

            EditorGUILayout.EndVertical();
        }

        private void AppendToConsole(string message)
        {
            lastLog = message;

            if (string.IsNullOrWhiteSpace(upgradeConsole))
                upgradeConsole = message;
            else
                upgradeConsole += "\n" + message;

            scrollConsoleToBottom = true;
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
                AppendToConsole($"Conta carregada de: {accountJsonPath}");
            }
            catch (Exception ex)
            {
                workingAccount = null;
                AppendToConsole($"Falha ao carregar conta: {ex.Message}");
            }
        }

        private void SaveAccountToDisk()
        {
            if (workingAccount == null)
            {
                AppendToConsole("Nenhuma conta carregada para salvar.");
                return;
            }

            try
            {
                workingAccount.EnsureInitialized();
                string json = JsonUtility.ToJson(workingAccount, true);
                File.WriteAllText(accountJsonPath, json);
                AppendToConsole($"Conta salva em: {accountJsonPath}");
            }
            catch (Exception ex)
            {
                AppendToConsole($"Falha ao salvar conta: {ex.Message}");
            }
        }
    }
}
