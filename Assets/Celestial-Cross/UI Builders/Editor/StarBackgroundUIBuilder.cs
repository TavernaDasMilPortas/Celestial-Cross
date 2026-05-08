using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CelestialCross.UI.Builders
{
    public static class StarBackgroundUIBuilder
    {
        [MenuItem("Celestial Cross/UI Builders/Screens/Generate Star Background")]
        public static void GenerateUI()
        {
            Canvas canvas = GameObject.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            GameObject bgObj = new GameObject("StarBackground");
            bgObj.transform.SetParent(canvas.transform, false);
            
            RectTransform rect = bgObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            RawImage img = bgObj.AddComponent<RawImage>();
            img.color = Color.white;
            
            // Add script
            bgObj.AddComponent<CelestialCross.UI.ScrollingStarBackground>();

            Selection.activeGameObject = bgObj;
            Debug.Log("Fundo de Estrelas Gerado! Lembre-se de adicionar uma textura de estrelas Seamless (Repetitiva) ao RawImage!");
        }
    }
}