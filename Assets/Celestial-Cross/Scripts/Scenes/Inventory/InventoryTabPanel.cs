using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace CelestialCross.Scenes.Inventory
{
    public abstract class InventoryTabPanel : MonoBehaviour
    {
        [Header("Tab Identity")]
        public bool isComingSoon;
        [Header("Tab Visual")]
        public Button tabButton;
        public Image tabButtonImage;
        public Color activeColor = Color.white;
        public Color inactiveColor = Color.gray;

        [Header("Containers")]
        public GameObject topPanelContent; // Conteúdo que vai no Top Panel
        public GameObject gridContent;     // Conteúdo que vai no Scroll View (Grid)
        public GameObject scrollView;      // O GameObject do Scroll View em si

        protected virtual void Awake()
        {
            if (tabButton != null)
            {
                tabButton.onClick.AddListener(() => InventorySceneController.Instance.SelectTab(this));
            }
        }

        public virtual void Show()
        {
            if (topPanelContent != null) topPanelContent.SetActive(true);
            if (gridContent != null) gridContent.SetActive(true);
            if (scrollView != null) 
            {
                scrollView.SetActive(true);
                var rt = scrollView.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.DOKill(true);
                    float startX = rt.anchoredPosition.x + 150f;
                    float endX = rt.anchoredPosition.x;
                    rt.anchoredPosition = new Vector2(startX, rt.anchoredPosition.y);
                    rt.DOAnchorPosX(endX, 0.4f).SetEase(Ease.OutBack);
                }
            }
            if (tabButtonImage != null) tabButtonImage.color = activeColor;
            
            OnShow();
        }

        public virtual void Hide()
        {
            if (topPanelContent != null) topPanelContent.SetActive(false);
            if (gridContent != null) gridContent.SetActive(false);
            if (scrollView != null) 
            {
                var rt = scrollView.GetComponent<RectTransform>();
                if (rt != null) rt.DOKill(true);
                scrollView.SetActive(false);
            }
            if (tabButtonImage != null) tabButtonImage.color = inactiveColor;
            
            OnHide();
        }

        // Métodos para sobrescrever na lógica de cada tab
        protected virtual void OnShow() { }
        protected virtual void OnHide() { }
        public virtual void Refresh() { }
    }
}
