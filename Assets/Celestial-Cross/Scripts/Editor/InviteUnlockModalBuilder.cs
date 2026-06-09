#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Progression;

namespace CelestialCross.EditorTools
{
    public class InviteUnlockModalBuilder : EditorWindow
    {
        [MenuItem("Celestial Cross/4. Tools/Build Invite Modal UI")]
        public static void BuildUI()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            GameObject modalPanel = new GameObject("InviteUnlockModal");
            modalPanel.transform.SetParent(canvas.transform, false);
            var rect = modalPanel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var img = modalPanel.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.8f);

            GameObject window = new GameObject("Window");
            window.transform.SetParent(modalPanel.transform, false);
            var winRect = window.AddComponent<RectTransform>();
            winRect.sizeDelta = new Vector2(600, 400);
            window.AddComponent<Image>().color = Color.white;

            GameObject container = new GameObject("OptionsContainer");
            container.transform.SetParent(window.transform, false);
            var contRect = container.AddComponent<RectTransform>();
            contRect.anchorMin = new Vector2(0.1f, 0.2f);
            contRect.anchorMax = new Vector2(0.9f, 0.8f);
            contRect.offsetMin = Vector2.zero;
            contRect.offsetMax = Vector2.zero;
            container.AddComponent<VerticalLayoutGroup>().childControlHeight = true;

            GameObject btnObj = new GameObject("OptionButtonPrefab");
            btnObj.transform.SetParent(container.transform, false);
            btnObj.AddComponent<Image>().color = Color.gray;
            var btn = btnObj.AddComponent<Button>();

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            var txt = textObj.AddComponent<TextMeshProUGUI>();
            txt.text = "Option";
            txt.color = Color.black;
            txt.alignment = TextAlignmentOptions.Center;
            var txtRect = textObj.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = Vector2.zero;
            txtRect.offsetMax = Vector2.zero;

            var modalComp = modalPanel.AddComponent<InviteUnlockModal>();

            Selection.activeGameObject = modalPanel;
        }
    }
}
#endif
