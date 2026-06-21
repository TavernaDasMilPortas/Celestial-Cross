using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

namespace CelestialCross.System
{
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; private set; }

        [SerializeField] private Canvas transitionCanvas;
        [SerializeField] private Image flashImage;

        public float flashInDuration = 0.5f;
        public float flashHoldMinDuration = 0.1f; // Tempo mínimo que o branco dura, mesmo que a cena carregue instantaneamente
        public float flashOutDuration = 0.5f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (transitionCanvas == null)
            {
                CreateCanvas();
            }

            flashImage.color = new Color(1, 1, 1, 0);
            flashImage.gameObject.SetActive(false);
        }

        private void CreateCanvas()
        {
            GameObject canvasObj = new GameObject("TransitionCanvas");
            canvasObj.transform.SetParent(transform);
            transitionCanvas = canvasObj.AddComponent<Canvas>();
            transitionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            transitionCanvas.sortingOrder = 9999; // Fica por cima de tudo

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            GameObject imageObj = new GameObject("FlashImage");
            imageObj.transform.SetParent(canvasObj.transform, false);
            flashImage = imageObj.AddComponent<Image>();
            flashImage.color = Color.white;

            RectTransform rect = flashImage.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public static void LoadScene(string sceneName)
        {
            if (Instance != null)
            {
                Instance.LoadSceneWithFlash(sceneName);
            }
            else
            {
                Debug.LogWarning("[SceneTransitionManager] Instância não encontrada. Criando fallback para transição da cena: " + sceneName);
                GameObject transitionObj = new GameObject("TempTransitionManager");
                SceneTransitionManager tempManager = transitionObj.AddComponent<SceneTransitionManager>();
                tempManager.LoadSceneWithFlash(sceneName);
            }
        }

        public void LoadSceneWithFlash(string sceneName)
        {
            StartCoroutine(TransitionRoutine(sceneName));
        }

        private IEnumerator TransitionRoutine(string sceneName)
        {
            flashImage.gameObject.SetActive(true);
            
            // Pausa o jogo para que animações da nova cena não rodem por debaixo do painel branco
            Time.timeScale = 0f;

            // 1. Flash In (Fica Branco) - ignorando timeScale
            yield return flashImage.DOFade(1f, flashInDuration).SetUpdate(true).SetEase(Ease.InOutSine).WaitForCompletion();

            // 2. Segura o Branco e carrega a cena de forma assíncrona
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;

            // Usamos unscaledDeltaTime já que timeScale é 0
            float holdTimer = 0f;
            while (!asyncLoad.isDone)
            {
                holdTimer += Time.unscaledDeltaTime;
                // Quando o progresso chegar a 0.9, a cena está pronta para ser ativada
                if (asyncLoad.progress >= 0.9f)
                {
                    // Garante que a tela fique branca por pelo menos o mínimo definido
                    if (holdTimer >= flashHoldMinDuration)
                    {
                        asyncLoad.allowSceneActivation = true;
                    }
                }
                yield return null;
            }

            // Aguarda um frame extra para garantir que a Unity instanciou tudo na nova cena
            yield return null;

            // 3. Opcional: Avisa algum gerenciador da cena nova que o flash vai sumir
            // Como o IntroModalUI que espera para começar a intro

            // 4. Flash Out (Branco some) - ignorando timeScale
            yield return flashImage.DOFade(0f, flashOutDuration).SetUpdate(true).SetEase(Ease.OutSine).WaitForCompletion();
            flashImage.gameObject.SetActive(false);

            // Restaura o tempo normal, agora as animações da cena nova começam a rodar fluidamente nas telas
            Time.timeScale = 1f;
        }
    }
}
