using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using CelestialCross.UI;
using CelestialCross.Authentication;

namespace CelestialCross.EditorScripts
{
    public class StartSceneUIBuilder : EditorWindow
    {
        [MenuItem("Celestial Cross/UI/Build Start Scene (Somente Componentes Novos)")]
        public static void BuildStartSceneUI()
        {
            Debug.Log("[StartSceneUIBuilder] Injetando componentes de Login na cena existente...");

            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("Não encontrei um Canvas! Você precisa abrir a cena inicial primeiro.");
                return;
            }

            // Garante que o AuthManager exista na cena
            PlayFabAuthManager authManager = Object.FindFirstObjectByType<PlayFabAuthManager>();
            if (authManager == null)
            {
                GameObject authObj = new GameObject("PlayFabAuthManager", typeof(PlayFabAuthManager));
                Debug.Log("[StartSceneUIBuilder] PlayFabAuthManager instanciado na cena.");
            }

            // Injeta o Hook de Login (um painel invisível que captura o clique)
            Transform existingHook = canvas.transform.Find("PlayFabLoginHook");
            if (existingHook != null)
            {
                DestroyImmediate(existingHook.gameObject);
            }

            GameObject hookObj = new GameObject("PlayFabLoginHook", typeof(RectTransform), typeof(Image), typeof(Button), typeof(StartSceneManager));
            hookObj.transform.SetParent(canvas.transform, false);

            RectTransform rect = hookObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Deixa a imagem invisível para capturar o toque em qualquer lugar da tela
            Image img = hookObj.GetComponent<Image>();
            img.color = new Color(0, 0, 0, 0);

            Button btn = hookObj.GetComponent<Button>();
            StartSceneManager manager = hookObj.GetComponent<StartSceneManager>();
            manager.startButton = btn;
            
            // Coloca o hook por cima de tudo na hierarquia do canvas
            hookObj.transform.SetAsLastSibling();

            Debug.Log("[StartSceneUIBuilder] Componentes de Login injetados com sucesso! (Botão invisível de tela cheia criado)");
        }
    }
}
