#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Authentication.UI;

namespace CelestialCross.Editor.Utils
{
    public class HubAuthUIBuilder : EditorWindow
    {
        [MenuItem("Celestial Cross/Utils/Update Hub with Auth Layer")]
        public static void UpdateHubScene()
        {
            // 1. Achar o Root da UI no Hub
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[AuthBuilder] Canvas não encontrado na cena!");
                return;
            }

            // 2. Criar o Overlay de Autenticação
            GameObject overlayObj = new GameObject("AuthOverlay", typeof(RectTransform));
            overlayObj.transform.SetParent(canvas.transform, false);
            
            RectTransform rect = overlayObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            HubAuthOverlay script = overlayObj.AddComponent<HubAuthOverlay>();

            // 3. Criar Sub-elementos (Sync Overlay)
            GameObject syncPanel = CreateUIObject("SyncOverlay", overlayObj.transform);
            SetFullscreen(syncPanel.GetComponent<RectTransform>());
            syncPanel.AddComponent<Image>().color = new Color(0, 0, 0, 0.8f);

            GameObject syncTextObj = CreateUIObject("SyncText", syncPanel.transform);
            TextMeshProUGUI syncText = syncTextObj.AddComponent<TextMeshProUGUI>();
            syncText.text = "Sincronizando com a Nuvem...";
            syncText.alignment = TextAlignmentOptions.Center;
            syncText.fontSize = 32;

            // 4. Criar Painel de Status (Canto superior)
            GameObject statusPanel = CreateUIObject("AuthStatusPanel", overlayObj.transform);
            RectTransform statusRect = statusPanel.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(1, 1);
            statusRect.anchorMax = new Vector2(1, 1);
            statusRect.pivot = new Vector2(1, 1);
            statusRect.sizeDelta = new Vector2(200, 50);
            statusRect.anchoredPosition = new Vector2(-10, -10);

            GameObject statusTextObj = CreateUIObject("StatusText", statusPanel.transform);
            TextMeshProUGUI statusText = statusTextObj.AddComponent<TextMeshProUGUI>();
            statusText.fontSize = 14;
            statusText.text = "Verificando...";
            statusText.color = Color.white;

            // 5. Vincular ao script
            var so = new SerializedObject(script);
            so.FindProperty("syncOverlay").objectReferenceValue = syncPanel;
            so.FindProperty("syncText").objectReferenceValue = syncText;
            so.FindProperty("authPanel").objectReferenceValue = statusPanel;
            so.FindProperty("statusText").objectReferenceValue = statusText;
            so.ApplyModifiedProperties();

            Debug.Log("[AuthBuilder] HubScene atualizada com a camada de Autenticação e Sync!");
            Selection.activeObject = overlayObj;
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            return obj;
        }

        private static void SetFullscreen(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
#endif
