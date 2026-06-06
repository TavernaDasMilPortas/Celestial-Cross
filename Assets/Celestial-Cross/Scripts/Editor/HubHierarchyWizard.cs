using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using System.IO;
using CelestialCross.Scenes.Hub;
using CelestialCross.Progression;
using CelestialCross.Dialogue.Graph;

namespace CelestialCross.EditorTools
{
    public class HubHierarchyWizard : OdinEditorWindow
    {
        [MenuItem("Celestial Cross/1. Editors/Wizards/Hub Hierarchy Wizard")]
        private static void OpenWindow()
        {
            GetWindow<HubHierarchyWizard>("Hub Wizard").Show();
        }

        [TitleGroup("Categoria")]
        [LabelText("Nome da Categoria"), Required]
        public string categoryName = "Nova Categoria";
        
        [TitleGroup("Categoria")]
        [LabelText("Tipo")]
        public HubCategoryType categoryType = HubCategoryType.Story;

        [TitleGroup("Categoria")]
        [LabelText("Ícone"), PreviewField(50, ObjectFieldAlignment.Left)]
        public Sprite categoryIcon;

        [TitleGroup("Categoria")]
        [LabelText("Descrição"), TextArea(2, 4)]
        public string categoryDescription;

        [TitleGroup("Hierarquia")]
        [ListDrawerSettings(ShowIndexLabels = true, AddCopiesLastElement = true)]
        [LabelText("Capítulos")]
        public List<WizardChapter> chapters = new List<WizardChapter>();

        [TitleGroup("Geração")]
        [Button("Gerar Hierarquia Completa", ButtonSizes.Gigantic), GUIColor(0.2f, 0.8f, 0.2f)]
        public void GenerateHierarchy()
        {
            if (string.IsNullOrEmpty(categoryName))
            {
                EditorUtility.DisplayDialog("Erro", "Nome da Categoria não pode ser vazio.", "OK");
                return;
            }

            string basePath = $"Assets/Celestial-Cross/Data/Hub/{categoryName}";
            
            // Cria pastas
            if (!AssetDatabase.IsValidFolder("Assets/Celestial-Cross/Data/Hub"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Celestial-Cross/Data"))
                    AssetDatabase.CreateFolder("Assets/Celestial-Cross", "Data");
                AssetDatabase.CreateFolder("Assets/Celestial-Cross/Data", "Hub");
            }

            if (!AssetDatabase.IsValidFolder(basePath))
            {
                AssetDatabase.CreateFolder("Assets/Celestial-Cross/Data/Hub", categoryName);
            }
            if (!AssetDatabase.IsValidFolder($"{basePath}/Chapters"))
                AssetDatabase.CreateFolder(basePath, "Chapters");
            if (!AssetDatabase.IsValidFolder($"{basePath}/Levels"))
                AssetDatabase.CreateFolder(basePath, "Levels");
            if (!AssetDatabase.IsValidFolder($"{basePath}/Dialogues"))
                AssetDatabase.CreateFolder(basePath, "Dialogues");

            // 1. Cria Categoria
            HubCategorySO categorySO = ScriptableObject.CreateInstance<HubCategorySO>();
            categorySO.CategoryName = categoryName;
            categorySO.CategoryType = categoryType;
            categorySO.Icon = categoryIcon;
            categorySO.Description = categoryDescription;

            string categoryPath = $"{basePath}/Cat_{categoryName}.asset";
            AssetDatabase.CreateAsset(categorySO, categoryPath);

            // 2. Cria Capítulos e Nós
            for (int i = 0; i < chapters.Count; i++)
            {
                var wizardChapter = chapters[i];
                ChapterData chapterSO = ScriptableObject.CreateInstance<ChapterData>();
                chapterSO.ChapterTitle = string.IsNullOrEmpty(wizardChapter.title) ? $"Capítulo {i+1}" : wizardChapter.title;
                chapterSO.Description = wizardChapter.description;
                chapterSO.ChapterID = global::System.Guid.NewGuid().ToString();

                foreach (var wNode in wizardChapter.nodes)
                {
                    StoryNode finalNode = null;

                    if (wNode.nodeType == WizardNodeType.Dialogue)
                    {
                        var dNode = new DialogueStoryNode();
                        
                        if (wNode.generateExternalAsset)
                        {
                            DialogueGraph dGraph = ScriptableObject.CreateInstance<DialogueGraph>();
                            string dPath = $"{basePath}/Dialogues/Dial_{chapterSO.ChapterTitle}_{wNode.title}.asset";
                            AssetDatabase.CreateAsset(dGraph, dPath);
                            dNode.DialogueGraph = dGraph;
                        }
                        finalNode = dNode;
                    }
                    else if (wNode.nodeType == WizardNodeType.Combat)
                    {
                        var cNode = new CombatStoryNode();
                        
                        if (wNode.generateExternalAsset)
                        {
                            LevelData levelData = ScriptableObject.CreateInstance<LevelData>();
                            levelData.LevelName = wNode.title;
                            string lPath = $"{basePath}/Levels/Level_{chapterSO.ChapterTitle}_{wNode.title}.asset";
                            AssetDatabase.CreateAsset(levelData, lPath);
                            cNode.LevelRef = levelData;
                        }
                        finalNode = cNode;
                    }

                    if (finalNode != null)
                    {
                        finalNode.NodeID = global::System.Guid.NewGuid().ToString();
                        finalNode.Title = string.IsNullOrEmpty(wNode.title) ? "Novo Nó" : wNode.title;
                        finalNode.NodeIcon = wNode.nodeIcon;
                        finalNode.MaxCompletions = wNode.maxCompletions;
                        chapterSO.Nodes.Add(finalNode);
                    }
                }

                string chapterPath = $"{basePath}/Chapters/Ch_{chapterSO.ChapterTitle}.asset";
                AssetDatabase.CreateAsset(chapterSO, chapterPath);
                
                categorySO.Chapters.Add(chapterSO);
            }

            EditorUtility.SetDirty(categorySO);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Sucesso", $"Hierarquia de '{categoryName}' gerada com sucesso em {basePath}!", "OK");
            Selection.activeObject = categorySO;
        }

        [global::System.Serializable]
        public class WizardChapter
        {
            [LabelText("Título do Capítulo")]
            public string title;
            [LabelText("Descrição")]
            public string description;
            
            [ListDrawerSettings(ShowIndexLabels = true)]
            [LabelText("Nós (Fases/Diálogos)")]
            public List<WizardNode> nodes = new List<WizardNode>();
        }

        public enum WizardNodeType { Combat, Dialogue }

        [global::System.Serializable]
        public class WizardNode
        {
            [HorizontalGroup("Top")]
            [LabelText("Título do Nó")]
            public string title;

            [HorizontalGroup("Top")]
            [LabelText("Tipo"), EnumToggleButtons]
            public WizardNodeType nodeType;

            [HorizontalGroup("Mid")]
            [LabelText("Gerar Asset Externo?")]
            [Tooltip("Se marcado, criará um LevelData (Combate) ou um DialogueGraph (Diálogo) vazio automaticamente na pasta correspondente.")]
            public bool generateExternalAsset = true;

            [HorizontalGroup("Mid")]
            [LabelText("Máx Conclusões")]
            [Tooltip("-1 para ilimitado.")]
            public int maxCompletions = -1;

            [LabelText("Ícone do Nó (Opcional)"), PreviewField(40, ObjectFieldAlignment.Left)]
            public Sprite nodeIcon;
        }
    }
}
