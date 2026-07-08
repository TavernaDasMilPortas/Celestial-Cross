using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using CelestialCross.Authentication;

namespace CelestialCross.UI
{
    public class ConfigSceneManager : MonoBehaviour
    {
        public TextMeshProUGUI usernameText;
        public TextMeshProUGUI uidText;
        public Button backButton;

        private void Start()
        {
            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);

            LoadProfileData();
        }

        private void LoadProfileData()
        {
            // Placeholder: Num jogo real puxaríamos os dados do PlayFab Client API
            if (usernameText != null)
                usernameText.text = "Username: Convidado";

            if (uidText != null)
            {
                if (PlayFabAuthManager.Instance != null && PlayFabAuthManager.Instance.IsSignedIn)
                {
                    uidText.text = $"UID: {PlayFabAuthManager.Instance.PlayFabId}";
                }
                else
                {
                    uidText.text = "UID: Não conectado";
                }
            }
        }

        private void OnBackClicked()
        {
            // Retorna para o Hub
            SceneManager.LoadScene("HubScene");
        }
    }
}
