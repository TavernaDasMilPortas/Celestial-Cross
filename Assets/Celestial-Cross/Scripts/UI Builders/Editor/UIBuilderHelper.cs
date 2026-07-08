using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CelestialCross.EditorScripts
{
    public static class UIBuilderHelper
    {
        public static Canvas GetOrCreateMainCanvas()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Main Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasObj.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;

                Debug.Log("[UIBuilder] Canvas criado!");
            }
            return canvas;
        }

        public static RectTransform CreatePanel(Transform parent, string name, Color color)
        {
            GameObject panelObj = new GameObject(name, typeof(RectTransform), typeof(Image));
            panelObj.transform.SetParent(parent, false);

            RectTransform rect = panelObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image img = panelObj.GetComponent<Image>();
            img.color = color;

            return rect;
        }

        public static TextMeshProUGUI CreateText(Transform parent, string name, string textContent, int fontSize, Color color, TextAlignmentOptions alignment)
        {
            GameObject textObj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(parent, false);

            TextMeshProUGUI tmpro = textObj.GetComponent<TextMeshProUGUI>();
            tmpro.text = textContent;
            tmpro.fontSize = fontSize;
            tmpro.color = color;
            tmpro.alignment = alignment;

            return tmpro;
        }

        public static Button CreateButton(Transform parent, string name, string btnText, Color btnColor)
        {
            GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);

            Image img = btnObj.GetComponent<Image>();
            img.color = btnColor;

            Button btn = btnObj.GetComponent<Button>();

            // Adiciona o texto
            CreateText(btnObj.transform, "Text", btnText, 36, Color.white, TextAlignmentOptions.Center);

            return btn;
        }
    }
}
