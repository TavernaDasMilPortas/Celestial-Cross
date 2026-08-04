using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Social;

namespace CelestialCross.UI
{
    public class ChatUIManager : MonoBehaviour
    {
        [Header("UI References")]
        public Button globalTabBtn;
        public Button guildTabBtn;
        public TMP_InputField inputField;
        public Button sendButton;
        public Button closeButton;
        public Transform contentParent;
        public GameObject messagePrefab; // Prefab TMP

        private string currentChannel = "global";
        
        // Armazena os GameObjects instanciados (para não destruir e criar de novo)
        // Uma abordagem simples: apagar todos os filhos ao trocar de aba e recriar,
        // ou manter um pool. Para simplificar: vamos limpar e recriar ao trocar de aba.

        private void OnEnable()
        {
            // Sempre que a UI abrir, inscreve-se nos eventos e atualiza o display
            if (ChatManager.Instance != null)
            {
                ChatManager.Instance.OnMessageReceived += HandleMessageReceived;
                ChatManager.Instance.OnChannelHistoryLoaded += HandleHistoryLoaded;
                RefreshChatDisplay();
            }
        }

        private void OnDisable()
        {
            // Remove o listener ao fechar/desativar a UI para evitar callbacks em objeto destruído
            if (ChatManager.Instance != null)
            {
                ChatManager.Instance.OnMessageReceived -= HandleMessageReceived;
                ChatManager.Instance.OnChannelHistoryLoaded -= HandleHistoryLoaded;
            }
        }

        private void Start()
        {
            if (sendButton != null)
                sendButton.onClick.AddListener(OnSendClicked);

            if (closeButton != null)
                closeButton.onClick.AddListener(OnCloseClicked);

            if (globalTabBtn != null)
                globalTabBtn.onClick.AddListener(() => SwitchChannel("global"));
                
            if (guildTabBtn != null)
            {
                // Como não temos sistema de guilda ativo ainda, usaremos um ID de guilda genérico para testes
                guildTabBtn.onClick.AddListener(() => SwitchChannel("guild_test"));
            }

            if (ChatManager.Instance == null)
            {
                Debug.LogWarning("[ChatUIManager] ChatManager.Instance é NULO!");
            }
        }

        private void OnDestroy()
        {
            if (sendButton != null) sendButton.onClick.RemoveListener(OnSendClicked);
            if (closeButton != null) closeButton.onClick.RemoveListener(OnCloseClicked);
            // Segurança extra: remove listener caso OnDisable não tenha rodado
            if (ChatManager.Instance != null) ChatManager.Instance.OnMessageReceived -= HandleMessageReceived;
        }

        private void OnCloseClicked()
        {
            gameObject.SetActive(false);
        }

        private void SwitchChannel(string channelName)
        {
            currentChannel = channelName;
            
            // Tenta assinar o canal (se já estiver assinado o Photon ignora)
            if (currentChannel.StartsWith("guild_") && ChatManager.Instance != null)
            {
                ChatManager.Instance.SubscribeToGuildChannel(currentChannel.Replace("guild_", ""));
            }

            RefreshChatDisplay();
            
            // Destaque visual (opcional) nas abas
            if (globalTabBtn != null)
                globalTabBtn.GetComponent<Image>().color = currentChannel == "global" ? new Color(0.5f, 0.5f, 0.5f) : Color.black;
            if (guildTabBtn != null)
                guildTabBtn.GetComponent<Image>().color = currentChannel != "global" ? new Color(0.5f, 0.5f, 0.5f) : Color.black;
        }

        private void RefreshChatDisplay()
        {
            if (contentParent == null) return;
            
            // Limpa as mensagens atuais na tela (iterando de trás para frente para evitar problemas)
            for (int i = contentParent.childCount - 1; i >= 0; i--)
            {
                Destroy(contentParent.GetChild(i).gameObject);
            }

            if (ChatManager.Instance != null)
            {
                var messages = ChatManager.Instance.GetMessagesForChannel(currentChannel);
                if (messages != null)
                {
                    foreach (var msg in messages)
                    {
                        RenderMessage(msg);
                    }
                }
            }
        }

        private void OnSendClicked()
        {
            if (inputField != null && !string.IsNullOrWhiteSpace(inputField.text))
            {
                if (ChatManager.Instance != null)
                {
                    ChatManager.Instance.SendMessageToChannel(currentChannel, inputField.text);
                    inputField.text = ""; // Limpa o campo
                }
            }
        }

        private void HandleMessageReceived(string channelName, ChatMessage msg)
        {
            // Se a mensagem chegou no canal atual, renderiza imediatamente
            if (channelName == currentChannel)
            {
                RenderMessage(msg);
            }
        }

        private void HandleHistoryLoaded(string channelName)
        {
            // Quando o histórico do canal atual chegar, redesenha tudo para mostrar as mensagens antigas
            if (channelName == currentChannel)
            {
                Debug.Log($"[ChatUIManager] Histórico do canal '{channelName}' recebido. Atualizando display...");
                RefreshChatDisplay();
            }
        }

        private void RenderMessage(ChatMessage msg)
        {
            if (msg == null) return;
            
            if (messagePrefab != null && contentParent != null)
            {
                GameObject newMsgObj = Instantiate(messagePrefab, contentParent);
                newMsgObj.SetActive(true); // Ativa o clone!
                
                TextMeshProUGUI txt = newMsgObj.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null)
                {
                    string sender = string.IsNullOrEmpty(msg.SenderName) ? "Desconhecido" : msg.SenderName;
                    string content = string.IsNullOrEmpty(msg.Content) ? "" : msg.Content;
                    
                    bool isMine = false;
                    if (ChatManager.Instance != null && !string.IsNullOrEmpty(ChatManager.Instance.LocalUsername))
                    {
                        isMine = sender == ChatManager.Instance.LocalUsername;
                    }
                    
                    if (isMine)
                    {
                        txt.alignment = TextAlignmentOptions.MidlineRight;
                        txt.text = $"<color=#A0FFA0>{sender}:</color> {content}";
                    }
                    else
                    {
                        txt.alignment = TextAlignmentOptions.MidlineLeft;
                        txt.text = $"<b>{sender}:</b> {content}";
                    }
                }
            }
        }
    }
}
