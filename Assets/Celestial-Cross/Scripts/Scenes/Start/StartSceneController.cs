using UnityEngine;
using UnityEngine.SceneManagement;
using CelestialCross.System;
using CelestialCross.Audio;

namespace CelestialCross.Scenes.Start
{
    public class StartSceneController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private string nextSceneName = "HubScene";

        [Header("UI Feedback (Optional)")]
        [SerializeField] private UnityEngine.UI.Button startButton;
        [SerializeField] private TMPro.TMP_Text statusText;

        private void Awake()
        {
            if (startButton == null) startButton = GetComponent<UnityEngine.UI.Button>();
            if (statusText == null) statusText = GetComponentInChildren<TMPro.TMP_Text>();
        }

        public async void OnStartClicked()
        {
            if (startButton != null) startButton.interactable = false;
            if (statusText != null) statusText.text = "Conectando ao servidor...";

            // Play a sound if AudioManager exists
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUI(SoundKey.GameStateChange01);
            }

            Debug.Log($"[StartSceneController] Iniciando o jogo! Verificando AccountManager...");

            // Se o AccountManager estiver na cena Start, aguarda o download dos dados antes de transicionar
            if (AccountManager.Instance != null)
            {
                // Mostra um feedback visual se tiver algum painel de loading no StartScene
                // ...
                
                bool isReady = false;
                global::System.Action onReady = () => isReady = true;
                AccountManager.OnAccountReady += onReady;

                // Se por acaso já carregou muito rápido
                if (AccountManager.Instance.PlayerAccount != null) 
                {
                    // Esperamos um tiquinho só pra garantir que o OnAccountReady foi chamado
                    await global::System.Threading.Tasks.Task.Delay(100);
                }

                float timeout = 15f; // Máximo de 15 segundos esperando a nuvem
                float timer = 0f;
                while (!isReady && AccountManager.Instance.PlayerAccount == null && timer < timeout)
                {
                    await global::System.Threading.Tasks.Task.Delay(100);
                    timer += 0.1f;
                }
                
                if (timer >= timeout)
                {
                    Debug.LogWarning("[StartSceneController] Timeout aguardando conta. Forçando transição!");
                    if (statusText != null) statusText.text = "Timeout na conexão. Modo Offline...";
                }
                else
                {
                    if (statusText != null) statusText.text = "Conta sincronizada! Carregando...";
                }
                
                AccountManager.OnAccountReady -= onReady;
                Debug.Log($"[StartSceneController] Conta sincronizada! Carregando: {nextSceneName}");
            }
            else
            {
                Debug.LogWarning("[StartSceneController] AccountManager não encontrado na cena Start. O Hub pode sofrer 'pop-in' de UI ao carregar.");
            }

            // Load next scene
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadSceneWithFlash(nextSceneName);
            }
            else
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }
}
