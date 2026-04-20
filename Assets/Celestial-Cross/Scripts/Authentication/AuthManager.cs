using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace CelestialCross.Authentication
{
    public class AuthManager : MonoBehaviour
    {
        public static AuthManager Instance { get; private set; }

        public event Action OnSignInSuccess;
        public event Action<string> OnSignInFailed;

        public bool IsSignedIn => AuthenticationService.Instance.IsSignedIn;
        public string PlayerId => AuthenticationService.Instance.PlayerId;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public async Task InitializeAndSignInAsync()
        {
            try
            {
                // Inicializa o SDK da Unity se ainda não foi feito
                if (UnityServices.State == ServicesInitializationState.Uninitialized)
                {
                    await UnityServices.InitializeAsync();
                }

                // Tenta login anônimo por padrão (Padrão UGS)
                if (!IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }

                Debug.Log($"[AuthManager] Signed In! PlayerID: {PlayerId}");
                OnSignInSuccess?.Invoke();
            }
            catch (AuthenticationException ex)
            {
                Debug.LogError($"[AuthManager] Sign In Failed: {ex.Message}");
                OnSignInFailed?.Invoke(ex.Message);
            }
            catch (RequestFailedException ex)
            {
                Debug.LogError($"[AuthManager] Request Failed: {ex.Message}");
                OnSignInFailed?.Invoke(ex.Message);
            }
        }

        /// <summary>
        /// Placeholder para o futuro login do Google Play Games.
        /// </summary>
        public async Task SignInWithGoogleAsync(string idToken)
        {
            try
            {
                await AuthenticationService.Instance.SignInWithGoogleAsync(idToken);
                Debug.Log("[AuthManager] Signed in with Google!");
                OnSignInSuccess?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AuthManager] Google Sign In Failed: {ex.Message}");
                OnSignInFailed?.Invoke(ex.Message);
            }
        }

        /// <summary>
        /// Vincula a conta corrente (Ex: Anônima) a um ID do Google.
        /// </summary>
        public async Task LinkWithGoogleAsync(string idToken)
        {
            try
            {
                await AuthenticationService.Instance.LinkWithGoogleAsync(idToken);
                Debug.Log("[AuthManager] Account Linked with Google!");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AuthManager] Linking Failed: {ex.Message}");
            }
        }
    }
}
