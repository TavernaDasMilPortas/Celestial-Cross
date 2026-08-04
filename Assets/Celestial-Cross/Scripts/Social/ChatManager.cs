using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CelestialCross.Cloud;

#if PHOTON_CHAT_IMPORTED
using Photon.Chat;
using ExitGames.Client.Photon;
#endif

namespace CelestialCross.Social
{
    [global::System.Serializable]
    public class ChatTokenResponse
    {
        public bool Success;
        public string Token;
    }

    [global::System.Serializable]
    public class ChatMessage
    {
        public string MessageId;
        public string ChannelId;
        public string SenderPlayFabId;
        public string SenderName;
        public string SenderIconId;
        public int SenderLevel;
        public string Content;
        public string SentAtUTC;
        public string MessageType; // "text", "system"
    }

    public class ChatManager : MonoBehaviour
#if PHOTON_CHAT_IMPORTED
    , IChatClientListener
#endif
    {
        public static ChatManager Instance { get; private set; }

        public Action<string, ChatMessage> OnMessageReceived;
        public Action<string> OnChatConnected;
        public Action<string> OnChatDisconnected;
        public Action<string> OnChannelHistoryLoaded; // Dispara após o histórico de um canal ser carregado

        [Header("Photon Settings")]
        [SerializeField] private string chatAppId = "INSERIR_APP_ID_AQUI"; // Deve ser preenchido no Inspector
        [SerializeField] private string appVersion = "1.0.0";
        
#if PHOTON_CHAT_IMPORTED
        private ChatClient chatClient;
#endif

        private string currentPlayFabId;
        private string currentUsername;
        public string LocalUsername => currentUsername;
        public string LocalPlayFabId => currentPlayFabId;
        
        private string globalChannelName = "global";

        // Cache de mensagens no Manager (sobrevive a troca de cenas e UI fechada)
        private Dictionary<string, List<ChatMessage>> channelMessages = new Dictionary<string, List<ChatMessage>>();

        // Controle de reconexão
        private bool hasConnectedOnce = false;
        private float reconnectTimer = 0f;
        private const float RECONNECT_DELAY = 3f;
        private bool isReconnecting = false;

        public bool IsConnected
        {
            get
            {
#if PHOTON_CHAT_IMPORTED
                return chatClient != null && chatClient.CanChat;
#else
                return false;
#endif
            }
        }

        public List<ChatMessage> GetMessagesForChannel(string channelName)
        {
            if (channelMessages.ContainsKey(channelName))
                return channelMessages[channelName];
            return new List<ChatMessage>();
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
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Start()
        {
            TryAutoConnect();
        }

        /// <summary>
        /// Chamado sempre que uma nova cena é carregada.
        /// Se o chat estava conectado antes e perdeu a conexão durante a transição, reconecta.
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (hasConnectedOnce && !IsConnected && !isReconnecting)
            {
                Debug.Log($"[ChatManager] Cena '{scene.name}' carregada e chat desconectado. Tentando reconectar...");
                TryAutoConnect();
            }
        }

        /// <summary>
        /// Tenta conectar ao chat usando os dados já disponíveis do PlayFab/AccountManager.
        /// Pode ser chamado em Start() ou OnSceneLoaded().
        /// </summary>
        private void TryAutoConnect()
        {
            // Se já está conectado, não faz nada
            if (IsConnected) return;

            // Se já conectou antes, temos os dados salvos — usa eles para reconectar
            if (hasConnectedOnce && !string.IsNullOrEmpty(currentPlayFabId))
            {
                if (PlayFab.PlayFabClientAPI.IsClientLoggedIn())
                {
                    Debug.Log("[ChatManager] Reconectando ao chat com dados salvos...");
                    ConnectToChat(currentPlayFabId, currentUsername);
                    return;
                }
            }

            // Primeira conexão: pega dados do AccountManager
            if (CelestialCross.Authentication.PlayFabAuthManager.Instance != null && 
                CelestialCross.Authentication.PlayFabAuthManager.Instance.IsSignedIn &&
                AccountManager.Instance != null && 
                AccountManager.Instance.PlayerAccount != null)
            {
                string playFabId = CelestialCross.Authentication.PlayFabAuthManager.Instance.PlayFabId;
                string playerName = AccountManager.Instance.PlayerAccount.Profile.PlayerName;
                
                if (string.Equals(playerName, "Viajante", StringComparison.OrdinalIgnoreCase) && 
                    !string.IsNullOrEmpty(AccountManager.Instance.PlayerAccount.Profile.FriendCode))
                {
                    playerName = $"viajante({AccountManager.Instance.PlayerAccount.Profile.FriendCode})";
                }
                
                ConnectToChat(playFabId, playerName);
            }
        }

        private void Update()
        {
#if PHOTON_CHAT_IMPORTED
            // Service() precisa rodar mesmo com timeScale=0 (durante transições de cena)
            if (chatClient != null)
            {
                chatClient.Service();
            }
#endif

            // Timer de reconexão automática
            if (isReconnecting)
            {
                reconnectTimer += Time.unscaledDeltaTime;
                if (reconnectTimer >= RECONNECT_DELAY)
                {
                    isReconnecting = false;
                    reconnectTimer = 0f;
                    TryAutoConnect();
                }
            }
        }

        public void ConnectToChat(string playFabId, string username)
        {
            currentPlayFabId = playFabId;
            currentUsername = username;

            if (!PlayFab.PlayFabClientAPI.IsClientLoggedIn())
            {
                Debug.LogError("[ChatManager] O jogador precisa estar logado no PlayFab para conectar ao chat.");
                return;
            }

            string sessionTicket = PlayFab.PlayFabSettings.staticPlayer.ClientSessionTicket;

            Debug.Log("[ChatManager] Conectando ao Photon Chat via PlayFab Auth...");

#if PHOTON_CHAT_IMPORTED
            chatClient = new ChatClient(this);
            chatClient.AuthValues = new AuthenticationValues(playFabId);
            chatClient.AuthValues.AuthType = CustomAuthenticationType.Custom;
            
            // O Photon precisa do Ticket e do TitleId para validar com o PlayFab
            chatClient.AuthValues.AddAuthParameter("Ticket", sessionTicket);
            chatClient.AuthValues.AddAuthParameter("TitleId", PlayFab.PlayFabSettings.staticSettings.TitleId);

            chatClient.Connect(chatAppId, appVersion, new AuthenticationValues(playFabId));
#else
            Debug.LogWarning("[ChatManager] Photon Chat não está importado! Defina PHOTON_CHAT_IMPORTED nas opções de build ou importe o SDK.");
#endif
        }

        public void Disconnect()
        {
#if PHOTON_CHAT_IMPORTED
            if (chatClient != null)
            {
                chatClient.Disconnect();
            }
#endif
        }

        public void SendMessageToChannel(string channelName, string content)
        {
#if PHOTON_CHAT_IMPORTED
            if (chatClient != null && chatClient.CanChat)
            {
                var msg = new ChatMessage
                {
                    MessageId = Guid.NewGuid().ToString(),
                    ChannelId = channelName,
                    SenderPlayFabId = currentPlayFabId,
                    SenderName = currentUsername,
                    Content = content,
                    SentAtUTC = DateTime.UtcNow.ToString("O"),
                    MessageType = "text"
                };

                string jsonMsg = JsonUtility.ToJson(msg);
                chatClient.PublishMessage(channelName, jsonMsg);
            }
            else
            {
                Debug.LogWarning("[ChatManager] Tentou enviar mensagem mas o chat não está conectado.");
            }
#endif
        }

        public void SubscribeToGuildChannel(string groupId)
        {
#if PHOTON_CHAT_IMPORTED
            if (chatClient != null && chatClient.CanChat)
            {
                chatClient.Subscribe(new string[] { $"guild_{groupId}" }, new int[] { 50 });
            }
#endif
        }

        // ----------------------------------------------------
        // MÉTODOS DO IChatClientListener (apenas compilados se PHOTON_CHAT_IMPORTED)
        // ----------------------------------------------------
#if PHOTON_CHAT_IMPORTED
        public void DebugReturn(DebugLevel level, string message) { }

        public void OnChatStateChange(ChatState state) { }

        public void OnConnected()
        {
            Debug.Log("[ChatManager] Conectado ao Photon Chat com sucesso!");
            hasConnectedOnce = true;
            isReconnecting = false;
            reconnectTimer = 0f;
            chatClient.Subscribe(new string[] { globalChannelName }, new int[] { 50 });
            OnChatConnected?.Invoke(currentPlayFabId);
        }

        public void OnDisconnected()
        {
            Debug.Log("[ChatManager] Desconectado do Photon Chat.");
            OnChatDisconnected?.Invoke("Disconnected");

            // Se já conectou antes, agenda reconexão automática
            if (hasConnectedOnce && !isReconnecting)
            {
                Debug.Log($"[ChatManager] Agendando reconexão em {RECONNECT_DELAY}s...");
                isReconnecting = true;
                reconnectTimer = 0f;
            }
        }

        public void OnGetMessages(string channelName, string[] senders, object[] messages)
        {
            Debug.Log($"[ChatManager] OnGetMessages: canal='{channelName}', {messages.Length} mensagem(ns) recebidas.");
            
            if (!channelMessages.ContainsKey(channelName))
            {
                channelMessages[channelName] = new List<ChatMessage>();
            }

            for (int i = 0; i < messages.Length; i++)
            {
                ChatMessage finalMsg = null;
                try
                {
                    string json = messages[i]?.ToString() ?? "";
                    if (string.IsNullOrEmpty(json)) continue;
                    
                    finalMsg = JsonUtility.FromJson<ChatMessage>(json);
                    
                    // Se o JsonUtility falhar silenciosamente (campos vazios)
                    if (string.IsNullOrEmpty(finalMsg.Content) && string.IsNullOrEmpty(finalMsg.SenderName))
                    {
                         finalMsg = new ChatMessage
                         {
                             SenderName = senders[i],
                             Content = json,
                             MessageType = "text"
                         };
                    }
                }
                catch
                {
                    // Trata como mensagem normal (texto puro) caso dê erro no parse
                    finalMsg = new ChatMessage
                    {
                        SenderName = senders[i],
                        Content = messages[i]?.ToString() ?? "",
                        MessageType = "text"
                    };
                }

                if (finalMsg != null)
                {
                    channelMessages[channelName].Add(finalMsg);
                    if (channelMessages[channelName].Count > 100)
                    {
                        channelMessages[channelName].RemoveAt(0);
                    }

                    OnMessageReceived?.Invoke(channelName, finalMsg);
                }
            }
        }

        public void OnPrivateMessage(string sender, object message, string channelName) { }
        public void OnSubscribed(string[] channels, bool[] results)
        {
            for (int i = 0; i < channels.Length; i++)
            {
                Debug.Log($"[ChatManager] Inscrito no canal '{channels[i]}' — sucesso: {results[i]}");
                
                // O Photon entrega o histórico via OnGetMessages logo após OnSubscribed.
                // Precisamos esperar um frame para que todas as mensagens históricas cheguem,
                // e então avisar a UI para dar refresh.
                if (results[i])
                {
                    StartCoroutine(NotifyHistoryLoadedNextFrame(channels[i]));
                }
            }
        }

        private global::System.Collections.IEnumerator NotifyHistoryLoadedNextFrame(string channelName)
        {
            // Espera 2 frames para o Service() processar as mensagens históricas
            yield return null;
            yield return null;
            
            int count = channelMessages.ContainsKey(channelName) ? channelMessages[channelName].Count : 0;
            Debug.Log($"[ChatManager] Histórico do canal '{channelName}' carregado. {count} mensagem(ns) no cache.");
            OnChannelHistoryLoaded?.Invoke(channelName);
        }
        public void OnUnsubscribed(string[] channels) { }
        public void OnStatusUpdate(string user, int status, bool gotMessage, object message) { }
        public void OnUserSubscribed(string channel, string user) { }
        public void OnUserUnsubscribed(string channel, string user) { }
#endif
    }
}

