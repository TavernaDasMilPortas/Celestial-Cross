using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace CelestialCross.Cloud
{
    public class NetworkOverlayUI : MonoBehaviour
    {
        public static NetworkOverlayUI Instance { get; private set; }

        [SerializeField] private GameObject overlayContainer;
        [SerializeField] private Text messageText;
        [SerializeField] private Image spinnerImage;

        private Coroutine _spinnerCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeFallbackUI();
        }

        private void InitializeFallbackUI()
        {
            // Se o usuário já vinculou os componentes via Prefab, não faz nada
            if (overlayContainer != null) return;

            // Tenta carregar um prefab da pasta Resources
            GameObject prefab = Resources.Load<GameObject>("UI/NetworkOverlayCanvas");
            if (prefab != null)
            {
                var instance = Instantiate(prefab, transform);
                
                // Tenta achar as referências automaticamente no prefab
                overlayContainer = instance.transform.GetChild(0).gameObject; // Assume que o primeiro filho é o painel principal
                messageText = instance.GetComponentInChildren<Text>(true);
                spinnerImage = instance.GetComponentInChildren<Image>(true);
            }
            else
            {
                // FALLBACK: Cria um Canvas básico via código caso o prefab não exista
                CreateFallbackCanvas();
            }

            if (overlayContainer != null)
                overlayContainer.SetActive(false);
        }

        private void CreateFallbackCanvas()
        {
            // Cria o Canvas
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32000;
            gameObject.AddComponent<CanvasScaler>();
            gameObject.AddComponent<GraphicRaycaster>();

            // Cria o painel de fundo (Overlay Container)
            overlayContainer = new GameObject("OverlayPanel");
            overlayContainer.transform.SetParent(transform, false);
            var panelRect = overlayContainer.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            var bgImage = overlayContainer.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.85f); // Preto semi-transparente

            // Cria o texto
            GameObject textObj = new GameObject("MessageText");
            textObj.transform.SetParent(overlayContainer.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0.5f);
            textRect.anchorMax = new Vector2(1, 0.5f);
            textRect.anchoredPosition = new Vector2(0, -60);
            textRect.sizeDelta = new Vector2(0, 100);
            messageText = textObj.AddComponent<Text>();
            messageText.alignment = TextAnchor.MiddleCenter;
            messageText.fontSize = 24;
            messageText.color = Color.white;
            messageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Cria o spinner (um quadrado giratório como fallback)
            GameObject spinnerObj = new GameObject("Spinner");
            spinnerObj.transform.SetParent(overlayContainer.transform, false);
            var spinnerRect = spinnerObj.AddComponent<RectTransform>();
            spinnerRect.anchorMin = new Vector2(0.5f, 0.5f);
            spinnerRect.anchorMax = new Vector2(0.5f, 0.5f);
            spinnerRect.anchoredPosition = new Vector2(0, 50);
            spinnerRect.sizeDelta = new Vector2(64, 64);
            spinnerImage = spinnerObj.AddComponent<Image>();
            spinnerImage.color = Color.cyan;
        }

        public void Show(string message = "Sem conexão com a internet...\nAguardando...")
        {
            if (overlayContainer == null) return;

            if (messageText != null)
                messageText.text = message;
                
            overlayContainer.SetActive(true);

            if (_spinnerCoroutine == null && spinnerImage != null && gameObject.activeInHierarchy)
            {
                _spinnerCoroutine = StartCoroutine(RotateSpinnerRoutine());
            }
        }

        public void Hide()
        {
            if (overlayContainer == null) return;

            overlayContainer.SetActive(false);

            if (_spinnerCoroutine != null)
            {
                StopCoroutine(_spinnerCoroutine);
                _spinnerCoroutine = null;
            }
        }

        private IEnumerator RotateSpinnerRoutine()
        {
            float angle = 0f;
            while (overlayContainer.activeSelf && spinnerImage != null)
            {
                angle -= Time.unscaledDeltaTime * 360f; // 1 rotação por segundo
                angle %= 360f;
                spinnerImage.rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
                yield return null;
            }
        }
    }
}
