#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CelestialCross.System;
using CelestialCross.System.UI;
using UnityEditor.SceneManagement;

namespace CelestialCross.EditorTools
{
    public class MessengerSystemBuilder : EditorWindow
    {
        private const string PrefabPath = "Assets/Celestial-Cross/Prefabs/UI/MessageBubbleUI.prefab";

        [MenuItem("Celestial Cross/4. Tools/Setup Messenger System")]
        public static void SetupMessengerSystem()
        {
            // 1. Ensure Directory
            if (!AssetDatabase.IsValidFolder("Assets/Celestial-Cross/Prefabs"))
                AssetDatabase.CreateFolder("Assets/Celestial-Cross", "Prefabs");
            if (!AssetDatabase.IsValidFolder("Assets/Celestial-Cross/Prefabs/UI"))
                AssetDatabase.CreateFolder("Assets/Celestial-Cross/Prefabs", "UI");

            // 2. Build or Load Prefab
            MessageBubbleUI prefabRef = AssetDatabase.LoadAssetAtPath<MessageBubbleUI>(PrefabPath);
            if (prefabRef == null)
            {
                prefabRef = BuildMessageBubblePrefab();
                Debug.Log("Created new MessageBubbleUI Prefab at: " + PrefabPath);
            }
            else
            {
                Debug.Log("MessageBubbleUI Prefab already exists.");
            }

            // 3. Find or Create Manager in Scene
            MessengerSystem sys = FindObjectOfType<MessengerSystem>();
            if (sys == null)
            {
                // Tenta achar o GameObject persistente que guarda a ProgressionService
                var progression = FindObjectOfType<ProgressionService>();
                if (progression != null)
                {
                    sys = progression.gameObject.AddComponent<MessengerSystem>();
                    Debug.Log("Added MessengerSystem to existing ProgressionService object.");
                }
                else
                {
                    GameObject go = new GameObject("SystemsManager");
                    sys = go.AddComponent<MessengerSystem>();
                    Debug.Log("Created new SystemsManager with MessengerSystem.");
                }
            }

            // 4. Assign Prefab
            if (sys.messageBubblePrefab == null)
            {
                sys.messageBubblePrefab = prefabRef;
                EditorUtility.SetDirty(sys);
                EditorSceneManager.MarkSceneDirty(sys.gameObject.scene);
                Debug.Log("Assigned Prefab to MessengerSystem.");
            }

            Debug.Log("Messenger System Setup Complete!");
        }

        private static MessageBubbleUI BuildMessageBubblePrefab()
        {
            // Root
            GameObject rootObj = new GameObject("MessageBubbleUI", typeof(RectTransform), typeof(CanvasGroup));
            RectTransform rootRt = rootObj.GetComponent<RectTransform>();
            rootRt.sizeDelta = new Vector2(800, 300);

            // Add the Logic Script
            MessageBubbleUI bubbleUI = rootObj.AddComponent<MessageBubbleUI>();
            bubbleUI.canvasGroup = rootObj.GetComponent<CanvasGroup>();
            bubbleUI.bubbleTransform = rootRt;

            // Background
            GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(rootObj.transform, false);
            RectTransform bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            Image bgImg = bgObj.GetComponent<Image>();
            bgImg.color = new Color(0, 0, 0, 0.9f);

            // Icon
            GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(rootObj.transform, false);
            RectTransform iconRt = iconObj.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0, 0.5f);
            iconRt.anchorMax = new Vector2(0, 0.5f);
            iconRt.pivot = new Vector2(0, 0.5f);
            iconRt.anchoredPosition = new Vector2(20, 0);
            iconRt.sizeDelta = new Vector2(250, 250);
            Image iconImg = iconObj.GetComponent<Image>();
            bubbleUI.iconImage = iconImg;

            // Text
            GameObject textObj = new GameObject("MessageText", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(rootObj.transform, false);
            RectTransform textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = new Vector2(0, 0);
            textRt.anchorMax = new Vector2(1, 1);
            textRt.offsetMin = new Vector2(300, 80);
            textRt.offsetMax = new Vector2(-20, -20);
            TextMeshProUGUI textTmp = textObj.GetComponent<TextMeshProUGUI>();
            textTmp.text = "Exemplo de Mensagem Extravagante...";
            textTmp.fontSize = 42;
            textTmp.color = Color.white;
            textTmp.alignment = TextAlignmentOptions.TopLeft;
            textTmp.enableWordWrapping = true;
            bubbleUI.messageText = textTmp;

            // Skip Button
            GameObject btnObj = new GameObject("SkipButton", typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(rootObj.transform, false);
            RectTransform btnRt = btnObj.GetComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(1, 0);
            btnRt.anchorMax = new Vector2(1, 0);
            btnRt.pivot = new Vector2(1, 0);
            btnRt.anchoredPosition = new Vector2(-20, 20);
            btnRt.sizeDelta = new Vector2(200, 60);
            
            Image btnImg = btnObj.GetComponent<Image>();
            btnImg.color = new Color(0.8f, 0.2f, 0.2f, 1f); // Vermelho estilo P5
            
            Button btn = btnObj.GetComponent<Button>();
            bubbleUI.skipButton = btn;

            GameObject btnTextObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            btnTextObj.transform.SetParent(btnObj.transform, false);
            RectTransform btnTextRt = btnTextObj.GetComponent<RectTransform>();
            btnTextRt.anchorMin = Vector2.zero;
            btnTextRt.anchorMax = Vector2.one;
            btnTextRt.offsetMin = Vector2.zero;
            btnTextRt.offsetMax = Vector2.zero;
            TextMeshProUGUI btnTmp = btnTextObj.GetComponent<TextMeshProUGUI>();
            btnTmp.text = "PULAR >>";
            btnTmp.fontSize = 28;
            btnTmp.color = Color.white;
            btnTmp.alignment = TextAlignmentOptions.Center;
            btnTmp.fontStyle = FontStyles.Bold | FontStyles.Italic;

            // Save Prefab
            MessageBubbleUI savedPrefab = PrefabUtility.SaveAsPrefabAsset(rootObj, PrefabPath).GetComponent<MessageBubbleUI>();
            DestroyImmediate(rootObj);

            return savedPrefab;
        }
    }
}
#endif
