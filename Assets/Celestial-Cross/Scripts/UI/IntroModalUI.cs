using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace CelestialCross.UI
{
    public class IntroModalUI : MonoBehaviour
    {
        public static IntroModalUI Instance { get; private set; }

        [Header("UI References")]
        public RectTransform panelTransform;
        public Image backgroundImage;
        public TextMeshProUGUI chapterText;
        public TextMeshProUGUI stageText;
        public RectTransform stageImageContainer; // Element that visually holds the stage text

        [Header("White Flash Transition")]
        public Image whiteFlashOverlay;
        public float flashFadeDuration = 0.5f;

        [Header("Animation Settings")]
        public float animDuration = 0.4f;
        public float readingTime = 1.5f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Começa invisível, mas ativado para o flash funcionar se necessário
            if (panelTransform != null)
                panelTransform.gameObject.SetActive(false);
                
            if (whiteFlashOverlay != null)
            {
                whiteFlashOverlay.gameObject.SetActive(true);
                whiteFlashOverlay.color = Color.white; // Fica branco puro
            }
        }

        public IEnumerator PlayIntroSequence()
        {
            string chapter = "Chapter 1";
            string stage = "The Awakening";

            if (GameFlowManager.Instance != null)
            {
                if (GameFlowManager.Instance.CurrentChapter != null)
                    chapter = GameFlowManager.Instance.CurrentChapter.ChapterTitle;
                
                if (GameFlowManager.Instance.SelectedStoryNode != null)
                    stage = GameFlowManager.Instance.SelectedStoryNode.Title;
                else if (GameFlowManager.Instance.SelectedLevel != null)
                    stage = GameFlowManager.Instance.SelectedLevel.name;
            }

            // Garante o setup inicial
            if (chapterText != null)
            {
                chapterText.text = chapter;
                chapterText.alpha = 0f;
            }

            if (stageText != null)
            {
                stageText.text = stage;
                stageText.alpha = 1f;
            }

            if (stageImageContainer != null)
            {
                stageImageContainer.localScale = Vector3.zero;
                // Deixa o container um pouco torto inicialmente para o "adesivo" ser mais estiloso
                stageImageContainer.localRotation = Quaternion.Euler(0, 0, 15f); 
            }

            // O painel agora entra com Fade e Rotação em vez de deslizar
            panelTransform.gameObject.SetActive(true);
            
            // O painel vem para a frente de tudo (inclusive do flash branco)
            panelTransform.SetAsLastSibling();
            
            CanvasGroup panelGroup = panelTransform.GetComponent<CanvasGroup>();
            if (panelGroup == null) panelGroup = panelTransform.gameObject.AddComponent<CanvasGroup>();
            panelGroup.alpha = 0f;

            // Inicia um pouco rotacionado para trás e ampliado
            panelTransform.localRotation = Quaternion.Euler(0, 0, -5f);
            panelTransform.localScale = Vector3.one * 1.1f;

            // 1. Fade IN do modal (enquanto o flash ainda está 100% branco cobrindo o mapa)
            Sequence introSeq = DOTween.Sequence();
            
            introSeq.Append(panelGroup.DOFade(1f, animDuration));
            introSeq.Join(panelTransform.DOScale(1f, animDuration).SetEase(Ease.OutBack));
            introSeq.Join(panelTransform.DORotate(Vector3.zero, animDuration).SetEase(Ease.OutBack));
            
            // Aparecer o texto do capítulo
            if (chapterText != null)
            {
                introSeq.Join(chapterText.DOFade(1f, 0.3f));
            }

            // 2. Agora que o modal está montado, o flash branco desbota revelando o mapa no fundo
            if (whiteFlashOverlay != null)
            {
                introSeq.Append(whiteFlashOverlay.DOFade(0f, flashFadeDuration).SetEase(Ease.OutCubic));
            }

            yield return introSeq.WaitForCompletion();
            
            if (whiteFlashOverlay != null) whiteFlashOverlay.gameObject.SetActive(false);

            // Pequena pausa de suspense
            yield return new WaitForSeconds(0.1f); 

            // 3. O ADESIVO DRAMÁTICO DA FASE
            if (stageImageContainer != null)
            {
                // Carimbo voa em direção a tela
                stageImageContainer.localScale = Vector3.one * 4f; 
                stageImageContainer.gameObject.SetActive(true);

                Sequence slapSeq = DOTween.Sequence();
                
                // Diminui o tamanho violentamente
                slapSeq.Join(stageImageContainer.DOScale(1f, 0.2f).SetEase(Ease.InExpo));
                // Rotaciona o adesivo para sua posição final (de 15 graus para -3 graus)
                slapSeq.Join(stageImageContainer.DORotate(new Vector3(0, 0, -3f), 0.2f).SetEase(Ease.InExpo));
                
                if (chapterText != null)
                {
                    slapSeq.Join(chapterText.DOFade(0f, 0.2f));
                }

                yield return slapSeq.WaitForCompletion();

                // IMPACTO! (Camera Shake / Panel Shake)
                // Tremer o painel principal inteiro para simular a força da colada
                panelTransform.DOShakePosition(0.3f, strength: 30f, vibrato: 20, randomness: 90f);
                panelTransform.DOShakeRotation(0.3f, strength: Vector3.forward * 5f, vibrato: 20);
                
                // Opcional: Tremer a câmera do jogo (se o CameraController suportar no futuro)
                if (Camera.main != null)
                {
                    Camera.main.transform.DOShakePosition(0.2f, strength: 0.5f, vibrato: 15);
                }
            }

            // 4. Espera o jogador ler o nome
            yield return new WaitForSeconds(readingTime);

            // 5. Saída estilizada: Folha de papel caindo (Sem Fade Out!)
            Sequence exitSeq = DOTween.Sequence();
            // Despenca para baixo (usamos um valor bem alto para garantir que saia da tela toda)
            exitSeq.Append(panelTransform.DOAnchorPosY(-Screen.height * 1.5f, animDuration * 1.2f).SetEase(Ease.InBack));
            // Gira enquanto cai
            exitSeq.Join(panelTransform.DORotate(new Vector3(0, 0, 15f), animDuration * 1.2f).SetEase(Ease.InBack));
            
            yield return exitSeq.WaitForCompletion();

            panelTransform.gameObject.SetActive(false);
            
            // Restaura as posições e rotações para a próxima vez
            panelTransform.anchoredPosition = Vector2.zero;
            panelTransform.localRotation = Quaternion.identity;
            panelTransform.localScale = Vector3.one;
            panelGroup.alpha = 1f;
        }

        public void HideIntroImmediate()
        {
            if (panelTransform != null) panelTransform.gameObject.SetActive(false);
            if (whiteFlashOverlay != null) whiteFlashOverlay.gameObject.SetActive(false);
        }
    }
}
