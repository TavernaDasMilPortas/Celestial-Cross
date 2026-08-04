using System;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

namespace CelestialCross.Authentication
{
    public class PlayFabAuthManager : MonoBehaviour
    {
        private static PlayFabAuthManager _instance;
        public static PlayFabAuthManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<PlayFabAuthManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("PlayFabAuthManager");
                        _instance = go.AddComponent<PlayFabAuthManager>();
                    }
                }
                return _instance;
            }
        }

        public event Action OnSignInSuccess;
        public event Action<string> OnSignInFailed;

        public bool IsSignedIn => PlayFabClientAPI.IsClientLoggedIn();
        public string PlayFabId { get; private set; }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void InitializeAndSignIn()
        {
            if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
            {
                PlayFabSettings.staticSettings.TitleId = "1206FE";
            }

            Debug.Log("[PlayFabAuthManager] Iniciando Login Anônimo...");

            string deviceId = "";
            try 
            {
                deviceId = SystemInfo.deviceUniqueIdentifier;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[PlayFabAuthManager] Falha ao obter deviceUniqueIdentifier: {e.Message}");
            }

            if (string.IsNullOrEmpty(deviceId) || deviceId == SystemInfo.unsupportedIdentifier)
            {
                deviceId = PlayerPrefs.GetString("FallbackDeviceId", "");
                if (string.IsNullOrEmpty(deviceId))
                {
                    deviceId = global::System.Guid.NewGuid().ToString();
                    PlayerPrefs.SetString("FallbackDeviceId", deviceId);
                    PlayerPrefs.Save();
                }
            }

            var request = new LoginWithCustomIDRequest
            {
                CustomId = deviceId,
                CreateAccount = true
            };

            PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginError);
        }

        public global::System.Threading.Tasks.Task InitializeAndSignInAsync()
        {
            var tcs = new global::System.Threading.Tasks.TaskCompletionSource<bool>();

            Action successCallback = null;
            Action<string> failedCallback = null;

            successCallback = () => {
                OnSignInSuccess -= successCallback;
                OnSignInFailed -= failedCallback;
                tcs.TrySetResult(true);
            };

            failedCallback = (err) => {
                OnSignInSuccess -= successCallback;
                OnSignInFailed -= failedCallback;
                tcs.TrySetException(new global::System.Exception(err));
            };

            OnSignInSuccess += successCallback;
            OnSignInFailed += failedCallback;

            InitializeAndSignIn();
            
            // Timeout de 10 segundos para não travar o jogo eternamente
            global::System.Threading.Tasks.Task.Delay(10000).ContinueWith(_ => {
                if (!tcs.Task.IsCompleted)
                {
                    OnSignInSuccess -= successCallback;
                    OnSignInFailed -= failedCallback;
                    tcs.TrySetException(new global::System.Exception("Timeout ao conectar com PlayFab."));
                }
            });

            return tcs.Task;
        }

        private void OnLoginSuccess(LoginResult result)
        {
            PlayFabId = result.PlayFabId;
            Debug.Log($"[PlayFabAuthManager] Sucesso! PlayFabID: {PlayFabId}");
            
            // Verifica se a conta acabou de ser criada para talvez dar itens iniciais
            if (result.NewlyCreated)
            {
                Debug.Log("[PlayFabAuthManager] Conta recém-criada!");
            }

            OnSignInSuccess?.Invoke();
        }

        private void OnLoginError(PlayFabError error)
        {
            string errorMsg = error.GenerateErrorReport();
            Debug.LogError($"[PlayFabAuthManager] Erro no Login: {errorMsg}");
            OnSignInFailed?.Invoke(errorMsg);
        }

        // --- MÉTODOS DE VÍNCULO DE CONTA (Fase 1: Identidade Real) ---

        public void LinkGoogleAccount(string serverAuthCode, Action onSuccess, Action<string> onError)
        {
            var request = new LinkGoogleAccountRequest
            {
                ServerAuthCode = serverAuthCode,
                ForceLink = false // Não fazemos force link para avisar sobre conflito
            };

            PlayFabClientAPI.LinkGoogleAccount(request, 
                res => 
                {
                    Debug.Log("[PlayFabAuthManager] Conta Google vinculada com sucesso.");
                    onSuccess?.Invoke();
                }, 
                err => 
                {
                    if (err.Error == PlayFabErrorCode.AccountAlreadyLinked || err.Error == PlayFabErrorCode.LinkedAccountAlreadyClaimed)
                    {
                        onError?.Invoke("AccountAlreadyClaimed"); // Código especial para a UI tratar o conflito
                    }
                    else
                    {
                        onError?.Invoke(err.GenerateErrorReport());
                    }
                });
        }

        public void LinkAppleAccount(string identityToken, Action onSuccess, Action<string> onError)
        {
            var request = new LinkAppleRequest
            {
                IdentityToken = identityToken,
                ForceLink = false
            };

            PlayFabClientAPI.LinkApple(request, 
                res => 
                {
                    Debug.Log("[PlayFabAuthManager] Conta Apple vinculada com sucesso.");
                    onSuccess?.Invoke();
                }, 
                err => 
                {
                    if (err.Error == PlayFabErrorCode.AccountAlreadyLinked || err.Error == PlayFabErrorCode.LinkedAccountAlreadyClaimed)
                    {
                        onError?.Invoke("AccountAlreadyClaimed");
                    }
                    else
                    {
                        onError?.Invoke(err.GenerateErrorReport());
                    }
                });
        }

        public void LinkEmailAccount(string email, string password, Action onSuccess, Action<string> onError)
        {
            var request = new AddUsernamePasswordRequest
            {
                Email = email,
                Password = password,
                Username = email.Split('@')[0] // Username simplificado
            };

            PlayFabClientAPI.AddUsernamePassword(request,
                res => 
                {
                    Debug.Log("[PlayFabAuthManager] E-mail vinculado com sucesso.");
                    onSuccess?.Invoke();
                },
                err => onError?.Invoke(err.GenerateErrorReport())
            );
        }
    }
}
