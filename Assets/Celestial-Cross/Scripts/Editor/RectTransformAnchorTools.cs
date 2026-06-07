using UnityEngine;
using UnityEditor;

namespace CelestialCross.Editor
{
    public class RectTransformAnchorTools : UnityEditor.Editor
    {
        [MenuItem("Celestial Cross/UI Tools/Anchors to Corners (Selecionados) %]", false, 1)]
        public static void SnapSelectedAnchors()
        {
            if (Selection.gameObjects.Length == 0)
            {
                Debug.LogWarning("Nenhum objeto selecionado para ajustar âncoras.");
                return;
            }

            int count = 0;
            foreach (GameObject go in Selection.gameObjects)
            {
                RectTransform rect = go.GetComponent<RectTransform>();
                if (rect != null)
                {
                    SnapAnchors(rect);
                    count++;
                }
            }
            
            Debug.Log($"[AutoAnchor] Âncoras ajustadas em {count} objeto(s).");
        }

        [MenuItem("Celestial Cross/UI Tools/Anchors to Corners (Com Filhos) %#]", false, 2)]
        public static void SnapSelectedAndChildrenAnchors()
        {
            if (Selection.gameObjects.Length == 0)
            {
                Debug.LogWarning("Nenhum objeto selecionado para ajustar âncoras.");
                return;
            }

            int count = 0;
            foreach (GameObject go in Selection.gameObjects)
            {
                RectTransform[] rects = go.GetComponentsInChildren<RectTransform>(true);
                foreach (var rect in rects)
                {
                    if (SnapAnchors(rect)) count++;
                }
            }

            Debug.Log($"[AutoAnchor] Âncoras ajustadas em {count} objeto(s) (incluindo filhos).");
        }

        // Adiciona atalho também ao clicar com o botão direito no objeto na Hierarchy
        [MenuItem("GameObject/UI/Snap Anchors (Ajustar Tela)", false, 0)]
        public static void SnapSelectedAnchorsContext(MenuCommand menuCommand)
        {
            // O Unity chama isso para cada objeto selecionado
            GameObject go = menuCommand.context as GameObject;
            if (go != null)
            {
                RectTransform rect = go.GetComponent<RectTransform>();
                if (rect != null)
                {
                    SnapAnchors(rect);
                }
            }
        }

        private static bool SnapAnchors(RectTransform rect)
        {
            RectTransform parent = rect.parent as RectTransform;
            if (parent == null) return false;

            Undo.RecordObject(rect, "Snap Anchors");

            Vector2 offsetMin = rect.offsetMin;
            Vector2 offsetMax = rect.offsetMax;
            Vector2 anchorMin = rect.anchorMin;
            Vector2 anchorMax = rect.anchorMax;

            float parentWidth = parent.rect.width;
            float parentHeight = parent.rect.height;

            if (parentWidth == 0 || parentHeight == 0) return false;

            anchorMin.x += offsetMin.x / parentWidth;
            anchorMin.y += offsetMin.y / parentHeight;
            anchorMax.x += offsetMax.x / parentWidth;
            anchorMax.y += offsetMax.y / parentHeight;

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Define o objeto como alterado para que a cena peça para ser salva
            EditorUtility.SetDirty(rect);
            return true;
        }
    }
}
