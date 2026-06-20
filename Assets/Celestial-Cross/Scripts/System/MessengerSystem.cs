using System.Collections.Generic;
using UnityEngine;
using CelestialCross.System.UI;

namespace CelestialCross.System
{
    public class MessengerSystem : MonoBehaviour
    {
        public static MessengerSystem Instance { get; private set; }

        [Header("Prefabs")]
        [Tooltip("O prefab do MessageBubbleUI que contém a animação e o texto")]
        public MessageBubbleUI messageBubblePrefab;

        private Canvas overlayCanvas;
        private UnityEngine.UI.Image blockerImage;
        private Queue<MessageData> messageQueue = new Queue<MessageData>();
        private bool isShowingMessage = false;
        private float originalTimeScale = 1f;

        private class MessageData
        {
            public string Text;
            public Sprite Icon;
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

            CreateOverlayCanvas();
        }

        private void CreateOverlayCanvas()
        {
            GameObject canvasObj = new GameObject("MessengerOverlayCanvas");
            canvasObj.transform.SetParent(transform);

            overlayCanvas = canvasObj.AddComponent<Canvas>();
            overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            overlayCanvas.sortingOrder = 32000; // Maior order possível para ficar por cima de tudo

            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>().uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Cria um fundo bloqueador invisível/semitransparente para não deixar clicar em nada
            GameObject blockerObj = new GameObject("BlockerPanel", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            blockerObj.transform.SetParent(overlayCanvas.transform, false);
            
            RectTransform rt = blockerObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            blockerImage = blockerObj.GetComponent<UnityEngine.UI.Image>();
            blockerImage.color = new Color(0, 0, 0, 0.5f); // Fundo semi-transparente escurecendo a tela
            blockerImage.gameObject.SetActive(false);
        }

        public void ShowMessage(string text, Sprite icon = null)
        {
            messageQueue.Enqueue(new MessageData { Text = text, Icon = icon });

            if (!isShowingMessage)
            {
                ProcessNextMessage();
            }
        }

        private void ProcessNextMessage()
        {
            if (messageQueue.Count == 0)
            {
                isShowingMessage = false;
                Time.timeScale = originalTimeScale; // Restaura o tempo
                if (blockerImage != null) blockerImage.gameObject.SetActive(false);
                return;
            }

            if (!isShowingMessage)
            {
                // Primeira mensagem da fila começando
                originalTimeScale = Time.timeScale;
                Time.timeScale = 0f; // Pausa o jogo
                if (blockerImage != null) blockerImage.gameObject.SetActive(true);
            }

            isShowingMessage = true;
            MessageData nextMessage = messageQueue.Dequeue();

            if (messageBubblePrefab != null)
            {
                MessageBubbleUI bubble = Instantiate(messageBubblePrefab, overlayCanvas.transform);
                bubble.Setup(nextMessage.Text, nextMessage.Icon, OnMessageCompleted);
            }
            else
            {
                Debug.LogWarning($"[MessengerSystem] {nextMessage.Text}");
                OnMessageCompleted();
            }
        }

        private void OnMessageCompleted()
        {
            ProcessNextMessage();
        }
    }
}
