using UnityEditor;
using UnityEngine;
using CelestialCross.Artifacts;
using System.Linq;

namespace CelestialCross.Artifacts.Editor
{
    public class ArtifactUpgraderWindow : EditorWindow
    {
        private ArtifactInstance targetArtifact;
        private string lastUpgradeLog = "";

        [MenuItem("Celestial Cross/Artifacts/Artifact Upgrader")]
        public static void ShowWindow()
        {
            GetWindow<ArtifactUpgraderWindow>("Artifact Upgrader");
        }

        private void OnGUI()
        {
            GUILayout.Label("Upgrade Artifact Instance (+1 to +15)", EditorStyles.boldLabel);
            
            targetArtifact = (ArtifactInstance)EditorGUILayout.ObjectField("Artifact Data File", targetArtifact, typeof(ArtifactInstance), false);
            
            GUILayout.Space(10);

            if (targetArtifact != null)
            {
                RenderArtifactInfo();
                
                GUILayout.Space(20);

                GUI.enabled = targetArtifact.currentLevel < 15;
                if (GUILayout.Button($"Level Up para {targetArtifact.currentLevel + 1}", GUILayout.Height(40)))
                {
                    PerformLevelUp();
                }
                GUI.enabled = true;

                if (!string.IsNullOrEmpty(lastUpgradeLog))
                {
                    EditorGUILayout.HelpBox(lastUpgradeLog, MessageType.Info);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Arraste um Artefato Forjado (Scriptable Object) para poder evoluí-lo.", MessageType.Warning);
            }
        }

        private void RenderArtifactInfo()
        {
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.padding = new RectOffset(10, 10, 10, 10);
            GUILayout.BeginVertical(boxStyle);

            GUILayout.Label($"Raridade: {targetArtifact.rarity}  |  Estrelas: {targetArtifact.GetStarsAsIntClamped()}*", EditorStyles.label);
            GUILayout.Label($"Nível Atual: +{targetArtifact.currentLevel}", EditorStyles.boldLabel);
            GUILayout.Label($"Atributo Principal: {targetArtifact.mainStat.ToString()}");
            
            GUILayout.Space(5);
            GUILayout.Label("Sub-Atributos Obtidos:", EditorStyles.boldLabel);
            foreach (var sub in targetArtifact.subStats)
            {
                GUILayout.Label($" - {sub.ToString()}");
            }
            
            GUILayout.EndVertical();
        }

        private void PerformLevelUp()
        {
            if (targetArtifact.currentLevel >= 15) return;

            targetArtifact.currentLevel++;
            
            // 1. Upgrade linear e fixo do Atributo Principal 
            float upgradeMainIncrement = ArtifactGenerator.GetMainStatUpgradeIncrement(targetArtifact.mainStat.statType, targetArtifact.stars);
            targetArtifact.mainStat.value += upgradeMainIncrement;

            lastUpgradeLog = $"Atributo Base subiu +{upgradeMainIncrement:F0}!";

            // 2. Checa as janelas (3, 6, 9, 12, 15) que engatilham Upgrades RNG de Substats
            if (targetArtifact.currentLevel % 3 == 0)
            {
                if (targetArtifact.subStats.Count < 4) // Tem Vaga para um Substat novo
                {
                    StatType newType = ArtifactGenerator.GetRandomSubstatType(targetArtifact.mainStat.statType, targetArtifact.subStats);
                    float startSubValue = ArtifactGenerator.GenerateSubstatValue(newType, targetArtifact.stars); // Pega um valor RNG inicial novinho
                    targetArtifact.subStats.Add(new StatModifier(newType, startSubValue));
                    
                    lastUpgradeLog += $"\n{targetArtifact.subStats.Count}º Substat nascido: {newType} +{startSubValue:F0}";
                }
                else // Já está lotado com 4 limites. Sorteia um dos existentes para tomar um Upgrade Extra Varíavel.
                {
                    int randomIndex = Random.Range(0, 4);
                    StatModifier targetSubToBuff = targetArtifact.subStats[randomIndex];
                    
                    float upgradeRNGIncrement = ArtifactGenerator.GetSubstatUpgradeIncrement(targetSubToBuff.statType, targetArtifact.stars);
                    targetArtifact.subStats[randomIndex] = new StatModifier(targetSubToBuff.statType, targetSubToBuff.value + upgradeRNGIncrement);

                    lastUpgradeLog += $"\nEvento Level {targetArtifact.currentLevel}! Substat '{targetSubToBuff.statType}' tomou proc RNG e ganhou +{upgradeRNGIncrement:F0} na barra!";
                }
            }

            // Avisa o Editor para salvar as modificações feitas nesse scriptable object fisicamente
            EditorUtility.SetDirty(targetArtifact);
            AssetDatabase.SaveAssets();
        }
    }
}
