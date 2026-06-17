using UnityEngine;
using TMPro;
using DG.Tweening;

namespace CelestialCross.UI
{
    public class IntroModalUI : MonoBehaviour
    {
        public static IntroModalUI Instance { get; private set; }

        public RectTransform panelTransform;
        public TextMeshProUGUI chapterText;
        public TextMeshProUGUI stageText;
        
        [Header("Animation Settings")]
        public float animDuration = 0.5f;
        public Vector2 offscreenTop = new Vector2(0, 300);
        public Vector2 onscreenPos = new Vector2(0, 0);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (panelTransform != null)
                panelTransform.anchoredPosition = offscreenTop;
        }

        private void Start()
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

            // Toca automaticamente no inicio da cena
            ShowIntro(chapter, stage);
        }

        public void ShowIntro(string chapterName, string stageName)
        {
            gameObject.SetActive(true);
            
            if (chapterText != null)
            {
                chapterText.text = chapterName;
                chapterText.alpha = 1f;
                chapterText.gameObject.SetActive(true);
            }

            if (stageText != null)
            {
                stageText.text = stageName;
                stageText.alpha = 0f; // Comeca invisivel
                stageText.gameObject.SetActive(true);
            }

            // Desliza de cima para o centro (onscreenPos)
            panelTransform.anchoredPosition = offscreenTop;
            panelTransform.DOAnchorPos(onscreenPos, animDuration).SetEase(Ease.OutBack).OnComplete(() =>
            {
                // Após deslizar, transita os textos
                Sequence seq = DOTween.Sequence();
                seq.AppendInterval(1.5f); // Tempo exibindo capitulo
                
                if (chapterText != null)
                    seq.Append(chapterText.DOFade(0f, 0.5f));
                    
                if (stageText != null)
                    seq.Append(stageText.DOFade(1f, 0.5f));
            });
        }

        public void HideIntro()
        {
            // Sobe de volta
            panelTransform.DOAnchorPos(offscreenTop, animDuration).SetEase(Ease.InBack).OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
        }
    }
}
