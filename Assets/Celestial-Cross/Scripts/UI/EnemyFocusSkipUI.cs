using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

namespace CelestialCross.UI
{
    [RequireComponent(typeof(Button))]
    public class EnemyFocusSkipUI : MonoBehaviour
    {
        public static EnemyFocusSkipUI Instance { get; private set; }

        public bool IsSkipRequested { get; private set; }

        private Button skipButton;
        [SerializeField] private CanvasGroup canvasGroup;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            skipButton = GetComponent<Button>();
            skipButton.onClick.AddListener(OnSkipClicked);
            
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            
            Hide(); // Começa escondido
        }

        public void Show()
        {
            IsSkipRequested = false;
            gameObject.SetActive(true);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, 0.3f);
            }
        }

        public void Hide()
        {
            if (canvasGroup != null)
            {
                canvasGroup.DOFade(0f, 0.2f).OnComplete(() => gameObject.SetActive(false));
            }
            else
            {
                gameObject.SetActive(false);
            }
            IsSkipRequested = false;
        }

        private void OnSkipClicked()
        {
            IsSkipRequested = true;
            Hide();
        }
    }
}
