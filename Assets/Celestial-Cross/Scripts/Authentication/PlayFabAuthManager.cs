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

            var request = new LoginWithCustomIDRequest
            {
                CustomId = SystemInfo.deviceUniqueIdentifier,
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
                tcs.SetResult(true);
            };

            failedCallback = (err) => {
                OnSignInSuccess -= successCallback;
                OnSignInFailed -= failedCallback;
                tcs.SetException(new global::System.Exception(err));
            };

            OnSignInSuccess += successCallback;
            OnSignInFailed += failedCallback;

            InitializeAndSignIn();
            
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
    }
}
