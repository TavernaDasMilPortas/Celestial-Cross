using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Unity.Services.Authentication;

namespace CelestialCross.Authentication.UI
{
    /// <summary>
    /// Camada de UI para o HubScene que gerencia o estado de Autenticação/Sync.
    /// Exibe um painel de bloqueio enquanto o Cloud Save sincroniza.
    /// </summary>
    public class HubAuthOverlay : MonoBehaviour
    {
        [Header("Status UI")]
        [SerializeField] private GameObject authPanel;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Button btnLinkGoogle; // Para o Plano 03 futuro
        [SerializeField] private Image loadingSpinner;

        [Header("Sync Visuals")]
        [SerializeField] private GameObject syncOverlay;
        [SerializeField] private TextMeshProUGUI syncText;

        private void Start()
        {
            if (authPanel != null) authPanel.SetActive(false);
            if (syncOverlay != null) syncOverlay.SetActive(false);

            StartCoroutine(CheckAuthStatusRoutine());
        }

        private IEnumerator CheckAuthStatusRoutine()
        {
            // Espera o AuthManager iniciar o login anônimo
            if (syncOverlay != null)
            {
                syncOverlay.SetActive(true);
                syncText.text = "Sincronizando Dados...";
            }

            // Aguarda o AccountManager terminar o LoadAndSync
            while (AccountManager.Instance == null || AccountManager.Instance.PlayerAccount == null)
            {
                yield return new WaitForSeconds(0.5f);
            }

            if (syncOverlay != null) syncOverlay.SetActive(false);
            
            UpdateAuthUI();
        }

        public void UpdateAuthUI()
        {
            if (authPanel == null) return;

            bool isSignedIn = AuthenticationService.Instance.IsSignedIn;
            authPanel.SetActive(true);

            if (isSignedIn)
            {
                string id = AuthenticationService.Instance.PlayerId;
                statusText.text = $"Logado como: {id.Substring(0, 8)}...";
                if (btnLinkGoogle != null) btnLinkGoogle.gameObject.SetActive(true);
            }
            else
            {
                statusText.text = "Modo Convidado (Offline)";
                if (btnLinkGoogle != null) btnLinkGoogle.gameObject.SetActive(false);
            }
        }

        public void OnClickLinkGoogle()
        {
            Debug.Log("[HubAuth] Solicitando login Google (Plano 03)...");
            // AuthManager.Instance.LinkWithGoogle(); // Implementaremos no Plano 03
        }
    }
}
