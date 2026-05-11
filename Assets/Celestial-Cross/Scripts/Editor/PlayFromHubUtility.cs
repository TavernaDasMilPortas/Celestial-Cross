using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CelestialCross.Editor
{
    public static class PlayFromHubUtility
    {
        private const string HubScenePath = "Assets/Celestial-Cross/Scenes/HubScene.unity";

        // Cria um item no menu superior da Unity
        // Atalho: Ctrl + G (Windows) ou Cmd + G (Mac)
        [MenuItem("Tools/Jogar da Hub %g")]
        public static void PlayFromHub()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                return;
            }

            // Tenta carregar o asset da cena
            SceneAsset hubScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(HubScenePath);

            if (hubScene != null)
            {
                // Define que SEMPRE que o jogo iniciar, começará pela Hub
                EditorSceneManager.playModeStartScene = hubScene;
                EditorApplication.isPlaying = true;
                
                // Registra um evento para limpar essa configuração quando você parar o jogo
                // Assim, se você apertar o "Play" normal da Unity depois, ele volta ao comportamento padrão
                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            }
            else
            {
                Debug.LogError($"[PlayFromHub] Não foi possível encontrar a HubScene no caminho: {HubScenePath}. Verifique se a pasta ou nome da cena mudou.");
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            // Quando o editor volta para o modo de edição (parou o jogo)
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                // Limpa a cena de início forçado
                EditorSceneManager.playModeStartScene = null;
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                Debug.Log("[PlayFromHub] Voltando para a cena de edição original.");
            }
        }
    }
}
