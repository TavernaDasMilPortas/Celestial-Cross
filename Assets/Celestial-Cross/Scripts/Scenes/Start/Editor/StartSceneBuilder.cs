#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Scenes.Start;
using CelestialCross.UI;

namespace CelestialCross.EditorScripts
{
    public class StartSceneBuilder : EditorWindow
    {
        [MenuItem("Tools/Celestial Cross/Build Start Scene UI")]
        public static void BuildUI()
        {
            // Ensure Canvas exists
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            // Ensure EventSystem exists
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // Clean up old UI if exists
            Transform oldUI = canvas.transform.Find("StartUI_Generated");
            if (oldUI != null)
            {
                DestroyImmediate(oldUI.gameObject);
            }

            // Create Controller
            StartSceneController controller = FindObjectOfType<StartSceneController>();
            if (controller == null)
            {
                GameObject controllerObj = new GameObject("StartSceneController");
                controller = controllerObj.AddComponent<StartSceneController>();
            }

            // Main UI Parent
            GameObject mainPanel = new GameObject("StartUI_Generated");
            mainPanel.transform.SetParent(canvas.transform, false);
            RectTransform mainRect = mainPanel.AddComponent<RectTransform>();
            mainRect.anchorMin = Vector2.zero;
            mainRect.anchorMax = Vector2.one;
            mainRect.sizeDelta = Vector2.zero;

            // Background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(mainPanel.transform, false);
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0.05f, 0.05f, 0.1f, 1f); // Dark blue/black background

            // Floating Logo/Image
            GameObject floatingObj = new GameObject("FloatingLogo");
            floatingObj.transform.SetParent(mainPanel.transform, false);
            RectTransform floatRect = floatingObj.AddComponent<RectTransform>();
            floatRect.anchorMin = new Vector2(0.5f, 0.5f);
            floatRect.anchorMax = new Vector2(0.5f, 0.5f);
            floatRect.anchoredPosition = new Vector2(0, 100);
            floatRect.sizeDelta = new Vector2(400, 200);
            Image floatImg = floatingObj.AddComponent<Image>();
            floatImg.color = new Color(0.8f, 0.8f, 1f, 1f); // Placeholder color
            // Add FloatingUI script
            floatingObj.AddComponent<FloatingUI>();

            GameObject logoTextObj = new GameObject("LogoPlaceholderText");
            logoTextObj.transform.SetParent(floatingObj.transform, false);
            RectTransform logoTextRect = logoTextObj.AddComponent<RectTransform>();
            logoTextRect.anchorMin = Vector2.zero;
            logoTextRect.anchorMax = Vector2.one;
            logoTextRect.sizeDelta = Vector2.zero;
            TextMeshProUGUI logoText = logoTextObj.AddComponent<TextMeshProUGUI>();
            logoText.text = "LOGO\n(Flutuante)";
            logoText.fontSize = 40;
            logoText.alignment = TextAlignmentOptions.Center;
            logoText.color = Color.black;

            // Fullscreen Button
            GameObject btnObj = new GameObject("StartButton");
            btnObj.transform.SetParent(mainPanel.transform, false);
            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = Vector2.zero;
            btnRect.anchorMax = Vector2.one;
            btnRect.sizeDelta = Vector2.zero;
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0, 0, 0, 0); // Transparent button covering the screen
            Button btn = btnObj.AddComponent<Button>();

            // Button Text (Blinking or static)
            GameObject btnTextObj = new GameObject("Text");
            btnTextObj.transform.SetParent(btnObj.transform, false);
            RectTransform btnTextRect = btnTextObj.AddComponent<RectTransform>();
            btnTextRect.anchorMin = new Vector2(0, 0);
            btnTextRect.anchorMax = new Vector2(1, 0.3f); // Lower third of the screen
            btnTextRect.sizeDelta = Vector2.zero;
            TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
            btnText.text = "Clique para iniciar";
            btnText.fontSize = 36;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.color = new Color(1f, 1f, 1f, 0.8f);

            // Hook the button to the controller
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, controller.OnStartClicked);

            Debug.Log("[StartSceneBuilder] UI da cena inicial gerada com sucesso!");
            
            // Mark scene dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
    }
}
#endif
