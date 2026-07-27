#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace CelestialCross.Cloud.Editor
{
    public class NetworkOverlaySetup
    {
        [MenuItem("Celestial Cross/Setup/Gerar Prefab do NetworkOverlay")]
        public static void GeneratePrefab()
        {
            string savePath = "Assets/Celestial-Cross/Resources/UI/NetworkOverlayCanvas.prefab";
            string resourcesDir = "Assets/Celestial-Cross/Resources/UI";

            // Verifica se a pasta existe
            if (!Directory.Exists(resourcesDir))
            {
                Directory.CreateDirectory(resourcesDir);
                AssetDatabase.Refresh();
            }

            // Verifica se o prefab já existe
            if (File.Exists(savePath))
            {
                bool overwrite = EditorUtility.DisplayDialog(
                    "Prefab Já Existe",
                    $"O prefab já existe no caminho:\n{savePath}\n\nDeseja sobrescrevê-lo?",
                    "Sobrescrever", "Cancelar"
                );

                if (!overwrite) return;
            }

            // --- 1. Cria a base do Canvas ---
            GameObject canvasObj = new GameObject("NetworkOverlayCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32000;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();

            // Adiciona nosso script manager
            NetworkOverlayUI overlayScript = canvasObj.AddComponent<NetworkOverlayUI>();

            // --- 2. Cria o Painel Escuro (Fundo) ---
            GameObject panelObj = new GameObject("OverlayPanel");
            panelObj.transform.SetParent(canvasObj.transform, false);
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            Image bgImage = panelObj.AddComponent<Image>();
            bgImage.color = new Color(0f, 0f, 0f, 0.85f);

            // --- 3. Cria o Texto Central ---
            GameObject textObj = new GameObject("MessageText");
            textObj.transform.SetParent(panelObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0f, 0.5f);
            textRect.anchorMax = new Vector2(1f, 0.5f);
            textRect.anchoredPosition = new Vector2(0f, -80f); // Deslocado para baixo do spinner
            textRect.sizeDelta = new Vector2(0f, 150f);
            Text textComp = textObj.AddComponent<Text>();
            textComp.text = "Sem conexão com a internet...\nAguardando...";
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.fontSize = 42;
            textComp.color = Color.white;
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // --- 4. Cria o Spinner (Imagem) ---
            GameObject spinnerObj = new GameObject("Spinner");
            spinnerObj.transform.SetParent(panelObj.transform, false);
            RectTransform spinnerRect = spinnerObj.AddComponent<RectTransform>();
            spinnerRect.anchorMin = new Vector2(0.5f, 0.5f);
            spinnerRect.anchorMax = new Vector2(0.5f, 0.5f);
            spinnerRect.anchoredPosition = new Vector2(0f, 60f);
            spinnerRect.sizeDelta = new Vector2(100f, 100f);
            Image spinnerImage = spinnerObj.AddComponent<Image>();
            
            // Tenta achar o sprite nativo da Unity de "Knob" ou círculo
            spinnerImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            spinnerImage.color = new Color(0f, 1f, 1f, 1f); // Cyan

            // --- 5. Conecta as referências no script ---
            // Como overlayContainer, messageText e spinnerImage são privados no NetworkOverlayUI.cs, 
            // precisaremos usar SerializedObject para preenchê-los.
            SerializedObject so = new SerializedObject(overlayScript);
            so.FindProperty("overlayContainer").objectReferenceValue = panelObj;
            so.FindProperty("messageText").objectReferenceValue = textComp;
            so.FindProperty("spinnerImage").objectReferenceValue = spinnerImage;
            so.ApplyModifiedProperties();

            // Esconde o painel por padrão para não atrapalhar no editor
            panelObj.SetActive(false);

            // --- 6. Salva como Prefab ---
            PrefabUtility.SaveAsPrefabAssetAndConnect(canvasObj, savePath, InteractionMode.UserAction);
            
            // --- 7. Limpeza (opcional, mas como já conectou, podemos deixar na cena se o usuário quiser mexer, 
            // ou deletar. Vamos deixar para o usuário ver). ---
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<GameObject>(savePath));
            Debug.Log($"[NetworkOverlaySetup] Prefab gerado com sucesso em: {savePath}");
        }
    }
}
#endif
