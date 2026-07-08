using UnityEngine;
using UnityEngine.SceneManagement;
using CelestialCross.Authentication;
using UnityEngine.UI;
using TMPro;

namespace CelestialCross.UI
{
    public class StartSceneManager : MonoBehaviour
    {
        public Button startButton;
        public TextMeshProUGUI statusText;

        private void Start()
        {
            if (startButton != null)
            {
                startButton.onClick.AddListener(OnStartClicked);
            }

            if (PlayFabAuthManager.Instance != null)
            {
                PlayFabAuthManager.Instance.OnSignInSuccess += HandleSignInSuccess;
                PlayFabAuthManager.Instance.OnSignInFailed += HandleSignInFailed;
            }
        }

        private void OnDestroy()
        {
            if (PlayFabAuthManager.Instance != null)
            {
                PlayFabAuthManager.Instance.OnSignInSuccess -= HandleSignInSuccess;
                PlayFabAuthManager.Instance.OnSignInFailed -= HandleSignInFailed;
            }
        }

        private void OnStartClicked()
        {
            if (statusText != null)
                statusText.text = "Conectando ao servidor...";
            
            if (startButton != null)
                startButton.interactable = false;

            PlayFabAuthManager.Instance.InitializeAndSignIn();
        }

        private void HandleSignInSuccess()
        {
            if (statusText != null)
                statusText.text = "Login concluído! Entrando no Hub...";
            
            // Vai para a Hub Scene
            SceneManager.LoadScene("HubScene");
        }

        private void HandleSignInFailed(string error)
        {
            if (statusText != null)
                statusText.text = "Falha no Login: " + error;
            
            if (startButton != null)
                startButton.interactable = true;
        }
    }
}
