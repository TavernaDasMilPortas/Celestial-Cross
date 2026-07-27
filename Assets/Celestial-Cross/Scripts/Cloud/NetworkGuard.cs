using System;
using System.Threading.Tasks;
using UnityEngine;

namespace CelestialCross.Cloud
{
    public class NetworkGuard : MonoBehaviour
    {
        public static NetworkGuard Instance { get; private set; }

        private bool _isOnline = true;
        private bool _isCheckingConnection = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            if (Instance == null)
            {
                var go = new GameObject("NetworkGuard");
                Instance = go.AddComponent<NetworkGuard>();
                DontDestroyOnLoad(go);

                // Initialize the UI overlay
                var overlayGo = new GameObject("NetworkOverlayUI");
                overlayGo.AddComponent<NetworkOverlayUI>();
                overlayGo.transform.SetParent(go.transform);
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _isOnline = Application.internetReachability != NetworkReachability.NotReachable;
        }

        private void Update()
        {
            bool currentlyOnline = Application.internetReachability != NetworkReachability.NotReachable;
            if (currentlyOnline != _isOnline)
            {
                _isOnline = currentlyOnline;
                if (_isOnline)
                {
                    Debug.Log("[NetworkGuard] Conexão restabelecida.");
                    NetworkOverlayUI.Instance?.Hide();
                }
                else
                {
                    Debug.LogWarning("[NetworkGuard] Conexão perdida!");
                    NetworkOverlayUI.Instance?.Show("Sem conexão com a internet...\nAguardando...");
                }
            }
        }

        public async Task<T> ExecuteWithGuardAsync<T>(string functionName, object functionArgs, int maxRetries = 3, bool showOverlayOnWait = true)
        {
            int attempts = 0;
            
            while (attempts <= maxRetries)
            {
                // Wait until online
                while (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    if (showOverlayOnWait && NetworkOverlayUI.Instance != null)
                        NetworkOverlayUI.Instance.Show("Aguardando conexão...");
                    
                    await Task.Delay(1000);
                }

                if (showOverlayOnWait && NetworkOverlayUI.Instance != null)
                {
                    // Hide if it was showing due to offline
                    NetworkOverlayUI.Instance.Hide(); 
                }

                // Execute function
                T result = default;
                try
                {
                    result = await PlayFabCloudFunctionCaller.ExecuteFunctionAsync<T>(functionName, functionArgs);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[NetworkGuard] Exceção ao chamar {functionName}: {e.Message}");
                }

                if (result != null)
                {
                    return result;
                }

                // Failure (null returned by PlayFabCloudFunctionCaller)
                attempts++;
                if (attempts <= maxRetries)
                {
                    Debug.LogWarning($"[NetworkGuard] Chamada {functionName} falhou. Retentativa {attempts}/{maxRetries}...");
                    if (showOverlayOnWait && NetworkOverlayUI.Instance != null)
                        NetworkOverlayUI.Instance.Show($"Falha na comunicação...\nTentando novamente ({attempts}/{maxRetries})");
                    
                    // Exponential backoff
                    int backoff = 1000 * (int)Mathf.Pow(2, attempts - 1);
                    await Task.Delay(backoff);
                }
            }

            Debug.LogError($"[NetworkGuard] Chamada {functionName} falhou após {maxRetries} retentativas.");
            if (showOverlayOnWait && NetworkOverlayUI.Instance != null)
            {
                // Mostra um erro temporário e depois esconde
                NetworkOverlayUI.Instance.Show("Erro de comunicação com o servidor.");
                _ = DelayHideOverlay();
            }

            return default;
        }

        private async Task DelayHideOverlay()
        {
            await Task.Delay(3000);
            NetworkOverlayUI.Instance?.Hide();
        }
    }
}
