using UnityEditor;
using UnityEngine;

namespace CelestialCross.Artifacts.Editor
{
    public class ArtifactGenerationTuningWindow : EditorWindow
    {
        private const string ResourceName = "ArtifactGenerationTuning";
        private const string DefaultAssetPath = "Assets/Celestial-Cross/Resources/ArtifactGenerationTuning.asset";

        private ArtifactGenerationTuning tuning;
        private UnityEditor.Editor tuningEditor;

        [MenuItem("Celestial Cross/Artifacts/Artifact Generation Tuning")]
        public static void ShowWindow()
        {
            GetWindow<ArtifactGenerationTuningWindow>("Tuning de Artefatos");
        }

        private void OnEnable()
        {
            Reload();
        }

        private void OnDisable()
        {
            if (tuningEditor != null)
                DestroyImmediate(tuningEditor);
        }

        private void Reload()
        {
            tuning = Resources.Load<ArtifactGenerationTuning>(ResourceName);
            if (tuningEditor != null)
                DestroyImmediate(tuningEditor);

            if (tuning != null)
                tuningEditor = UnityEditor.Editor.CreateEditor(tuning);
        }

        private void OnGUI()
        {
            GUILayout.Label("Tuning de Geração de Artefatos", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                "Edite aqui os valores mínimos/máximos usados para gerar atributos (main stat e substats).\n" +
                "Quando este asset existir (e 'useTuning' estiver ligado), o ArtifactGenerator usa estes valores automaticamente.",
                MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(tuning == null ? "Criar Asset de Tuning" : "Selecionar Asset de Tuning"))
            {
                if (tuning == null)
                {
                    CreateAssetIfMissing();
                    Reload();
                }

                if (tuning != null)
                    Selection.activeObject = tuning;
            }

            if (GUILayout.Button("Recarregar"))
                Reload();
            EditorGUILayout.EndHorizontal();

            if (tuning == null)
            {
                EditorGUILayout.HelpBox(
                    $"Nenhum asset encontrado em Resources com nome '{ResourceName}'. Clique em 'Criar Asset de Tuning'.",
                    MessageType.Warning);
                return;
            }

            GUILayout.Space(8);

            if (tuningEditor != null)
                tuningEditor.OnInspectorGUI();
        }

        private static void CreateAssetIfMissing()
        {
            var existing = AssetDatabase.LoadAssetAtPath<ArtifactGenerationTuning>(DefaultAssetPath);
            if (existing != null)
                return;

            var asset = CreateInstance<ArtifactGenerationTuning>();
            asset.ResetToGeneratorDefaults();

            string folder = System.IO.Path.GetDirectoryName(DefaultAssetPath);
            if (!AssetDatabase.IsValidFolder(folder))
            {
                // Creates missing folders recursively.
                string[] parts = folder.Split('/');
                string current = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    string next = current + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(next))
                        AssetDatabase.CreateFolder(current, parts[i]);
                    current = next;
                }
            }

            AssetDatabase.CreateAsset(asset, DefaultAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
