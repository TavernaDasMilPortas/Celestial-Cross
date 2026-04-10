using UnityEditor;
using UnityEngine;
using CelestialCross.Artifacts;

namespace CelestialCross.Artifacts.Editor
{
    public class ArtifactCreatorWindow : EditorWindow
    {
        private ArtifactType selectedSlot;
        private ArtifactSet selectedSet;
        private ArtifactRarity selectedRarity;
        private ArtifactStars selectedStars = ArtifactStars.One;
        private StatType selectedMainStat;

        [MenuItem("Celestial Cross/Artifacts/Artifact Creator")]
        public static void ShowWindow()
        {
            GetWindow<ArtifactCreatorWindow>("Artifact Forger");
        }

        private void OnGUI()
        {
            GUILayout.Label("Create New Artifact Instance", EditorStyles.boldLabel);

            // Campos da interface base
            selectedSlot = (ArtifactType)EditorGUILayout.EnumPopup("Slot Type", selectedSlot);
            selectedSet = (ArtifactSet)EditorGUILayout.ObjectField("Artifact Family Set", selectedSet, typeof(ArtifactSet), false);
            selectedRarity = (ArtifactRarity)EditorGUILayout.EnumPopup("Rarity (Dictates # of Init Substats)", selectedRarity);
            selectedStars = (ArtifactStars)EditorGUILayout.EnumPopup("Stars (Level Scaling & Range)", selectedStars);
            selectedMainStat = (StatType)EditorGUILayout.EnumPopup("Forced Main Stat", selectedMainStat);

            GUILayout.Space(20);

            if (GUILayout.Button("Forge Artifact", GUILayout.Height(40)))
            {
                GenerateArtifact();
            }
        }

        private void GenerateArtifact()
        {
            // Instancia o SO em memória
            ArtifactInstance newArtifact = ScriptableObject.CreateInstance<ArtifactInstance>();
            newArtifact.GenerateGUID();
            
            // Popula Dados Fixos
            newArtifact.slot = selectedSlot;
            newArtifact.artifactSet = selectedSet;
            newArtifact.rarity = selectedRarity;
            newArtifact.stars = selectedStars;
            newArtifact.currentLevel = 1;

            // Determina Valor Final Fixo Base baseado nas Estrelas para o lvl 1
            float baseMainValue = ArtifactGenerator.GetMainStatBaseValue(selectedMainStat, selectedStars);
            newArtifact.mainStat = new StatModifier(selectedMainStat, baseMainValue);

            // Roll RNG Inicial da Janela de Substatus de acordo com Raridade e Estrelas
            int substatsCountToGenerate = ArtifactGenerator.GetInitialSubstatCount(selectedRarity);

            for (int i = 0; i < substatsCountToGenerate; i++)
            {
                StatType rolledType = ArtifactGenerator.GetRandomSubstatType(selectedMainStat, newArtifact.subStats);
                float rolledValue = ArtifactGenerator.GenerateSubstatValue(rolledType, selectedStars);
                newArtifact.subStats.Add(new StatModifier(rolledType, rolledValue));
            }

            // Salva fisicamente o SO no projeto (na pasta criada pro Dev brincar)
            string dateStr = global::System.DateTime.Now.ToString("dd-HH-mm");
            string path = $"Assets/Celestial-Cross/Artifacts_Test/{selectedSlot}_{selectedRarity}_{(int)selectedStars}Star_{dateStr}.asset";
            
            // Cria diretório se não existir
            if (!AssetDatabase.IsValidFolder("Assets/Celestial-Cross/Artifacts_Test"))
            {
                AssetDatabase.CreateFolder("Assets/Celestial-Cross", "Artifacts_Test");
            }

            AssetDatabase.CreateAsset(newArtifact, path);
            AssetDatabase.SaveAssets();

            Debug.Log($"<color=green>Artifact Forged!</color> Saved at {path}", newArtifact);
            Selection.activeObject = newArtifact;
        }
    }
}
