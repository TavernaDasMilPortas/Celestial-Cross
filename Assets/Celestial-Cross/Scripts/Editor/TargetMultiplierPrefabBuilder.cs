using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using Celestial_Cross.Scripts.UI;

namespace CelestialCross.EditorArea
{
    public class TargetMultiplierPrefabBuilder : EditorWindow
    {
        private const string PrefabPath = "Assets/Celestial-Cross/Prefabs/UI/TargetMultiplierText.prefab";

        [MenuItem("Celestial Cross/3. UI Builders/Target Multiplier Prefab")]
        public static void ShowWindow()
        {
            GetWindow<TargetMultiplierPrefabBuilder>("Target Multiplier Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("Target Multiplier UI Builder", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            GUILayout.Label("Este utilitário cria e atualiza o prefab do texto");
            GUILayout.Label("multiplicador (x2, x3...) mostrado em cima dos alvos.");
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Build TargetMultiplierText Prefab", GUILayout.Height(40)))
            {
                BuildPrefab();
            }
        }

        private void BuildPrefab()
        {
            // Criar diretórios se não existirem
            if (!AssetDatabase.IsValidFolder("Assets/Celestial-Cross/Prefabs/UI"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Celestial-Cross/Prefabs"))
                    AssetDatabase.CreateFolder("Assets/Celestial-Cross", "Prefabs");
                AssetDatabase.CreateFolder("Assets/Celestial-Cross/Prefabs", "UI");
            }

            // Criar estrutura temporária
            GameObject rootGo = new GameObject("TargetMultiplierText", typeof(RectTransform), typeof(CanvasGroup), typeof(TargetMultiplierUI));
            RectTransform rootRt = rootGo.GetComponent<RectTransform>();
            rootRt.sizeDelta = new Vector2(80, 40);

            // Adicionar Fundo
            GameObject bgGo = new GameObject("BG", typeof(RectTransform), typeof(Image));
            bgGo.transform.SetParent(rootRt, false);
            RectTransform bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            
            Image bgImage = bgGo.GetComponent<Image>();
            bgImage.color = new Color(0.7f, 0.15f, 0.15f, 0.85f); // Vermelho escuro semi-transparente
            
            // Adicionar Texto
            GameObject textGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(rootRt, false);
            RectTransform textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.text = "x2";
            tmp.fontSize = 24;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            
            // Tentar adicionar outline básico
            UnityEngine.UI.Outline outline = textGo.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.8f);
            outline.effectDistance = new Vector2(1, -1);

            // Salvar Prefab
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(rootGo, PrefabPath);
            DestroyImmediate(rootGo);
            
            Debug.Log($"[UIBuilder] Prefab salvo com sucesso em: {PrefabPath}");

            // Tentar linkar no Manager ativo
            TargetMultiplierUIManager manager = FindObjectOfType<TargetMultiplierUIManager>();
            if (manager != null)
            {
                SerializedObject so = new SerializedObject(manager);
                SerializedProperty prop = so.FindProperty("multiplierTextPrefab");
                if (prop != null)
                {
                    prop.objectReferenceValue = prefab;
                    so.ApplyModifiedProperties();
                }
                Debug.Log("[UIBuilder] Prefab associado automaticamente ao TargetMultiplierUIManager na cena.");
            }
        }
    }
}
