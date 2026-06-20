using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;

namespace CelestialCross.Gacha.UI
{
    public class ShopSceneUI : MonoBehaviour
    {
        [Header("Configuração de Banners")]
        public List<GachaBannerSO> availableBanners;
        private int currentBannerIndex = 0;

        [Header("Top Bar Currencies")]
        [SerializeField] private TextMeshProUGUI starMapsText;
        [SerializeField] private TextMeshProUGUI stardustText;

        [Header("Abas da Loja")]
        [SerializeField] private Button tabBannersBtn;
        [SerializeField] private Button tabExchangeBtn;
        [SerializeField] private GameObject contentBanners;
        [SerializeField] private GameObject contentExchange;

        [Header("Painel do Banner Atual")]
        [SerializeField] private Image bannerSplashArt;
        [SerializeField] private TextMeshProUGUI bannerTitle;
        [SerializeField] private TextMeshProUGUI bannerCostInfo;
        [SerializeField] private TextMeshProUGUI pityInfoText;
        [SerializeField] private Button btnPull1;
        [SerializeField] private Button btnPull10;
        [SerializeField] private Button btnDetails;

        [Header("Navegação de Banners")]
        [SerializeField] private Button btnNextBanner;
        [SerializeField] private Button btnPrevBanner;

        [Header("Animação do Gacha")]
        [SerializeField] private GachaAnimationController animationController;

        [Header("Exchange/Câmbio Placeholder")]
        [SerializeField] private Button btnConvertStardustToStarMaps;

        [Header("Navegação")]
        [SerializeField] private Button btnBackToHub;
        [SerializeField] private string hubSceneName = "HubScene";

        [Header("Top Bar Transform (Para Animação)")]
        [SerializeField] private RectTransform topBarRect; // Opcional, se existir na cena
        
        [Header("Banner Content (Mascara/Borda)")]
        [SerializeField] private RectTransform bannerContentBorder;

        [Header("Componentes das Abas (Navegação)")]
        [SerializeField] private RectTransform active_tab_root;
        [SerializeField] private RectTransform inactive_tab_root;
        [SerializeField] private RectTransform banner_tab_component;
        [SerializeField] private RectTransform shop_tab_component;

        private Tween idleSplashTween;
        private float splashOriginalX;

        private void Awake()
        {
            GachaService.Initialize();

            if (btnBackToHub != null) btnBackToHub.onClick.AddListener(ReturnToHub);

            if (tabBannersBtn != null) tabBannersBtn.onClick.AddListener(() => SwitchTab(true));
            if (tabExchangeBtn != null) tabExchangeBtn.onClick.AddListener(() => SwitchTab(false));

            if (btnPull1 != null) btnPull1.onClick.AddListener(() => DoPull(1));
            if (btnPull10 != null) btnPull10.onClick.AddListener(() => DoPull(10));
            if (btnDetails != null) btnDetails.onClick.AddListener(ShowBannerDetails);

            if (btnConvertStardustToStarMaps != null) btnConvertStardustToStarMaps.onClick.AddListener(ConvertStardustToStarMap);

            if (btnNextBanner != null) btnNextBanner.onClick.AddListener(() => ChangeBanner(1));
            if (btnPrevBanner != null) btnPrevBanner.onClick.AddListener(() => ChangeBanner(-1));

            SwitchTab(true);
            StartCoroutine(PullButtonsAttentionRoutine());
        }

        private void Start()
        {
            RefreshUI();
            PlayIntroAnimation();
            StartIdleAnimations();
        }

        private void PlayIntroAnimation()
        {
            // Cascata preservando a posição original do Editor (usando .From() e movimento relativo)
            if (contentBanners != null)
            {
                var rect = contentBanners.GetComponent<RectTransform>();
                if (rect != null)
                {
                    // Desce 100 pixels e volta para a posição ORIGINAL que estava no Inspector
                    rect.DOAnchorPosY(rect.anchoredPosition.y - 100f, 0.5f).From().SetEase(Ease.OutBack).SetDelay(0.1f);
                    
                    // Fade In se houver CanvasGroup
                    var cg = contentBanners.GetComponent<CanvasGroup>();
                    if (cg != null) { cg.alpha = 0; cg.DOFade(1, 0.5f).SetDelay(0.1f); }
                }
            }

            if (contentExchange != null && contentExchange.activeSelf)
            {
                var rect = contentExchange.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.DOAnchorPosY(rect.anchoredPosition.y - 100f, 0.5f).From().SetEase(Ease.OutBack).SetDelay(0.1f);
                    
                    var cg = contentExchange.GetComponent<CanvasGroup>();
                    if (cg != null) { cg.alpha = 0; cg.DOFade(1, 0.5f).SetDelay(0.1f); }
                }
            }
            
            // Abas entrando junto com o conteúdo (animação de queda inicial)
            if (active_tab_root != null) active_tab_root.DOAnchorPosY(active_tab_root.anchoredPosition.y - 100f, 0.5f).From().SetEase(Ease.OutBack).SetDelay(0.15f);
            if (inactive_tab_root != null) inactive_tab_root.DOAnchorPosY(inactive_tab_root.anchoredPosition.y - 100f, 0.5f).From().SetEase(Ease.OutBack).SetDelay(0.15f);
            
            // Entrada elástica da barra superior, se existir
            if (topBarRect != null)
            {
                topBarRect.DOAnchorPosY(topBarRect.anchoredPosition.y + 150f, 0.4f).From().SetEase(Ease.OutBack);
            }
        }

        private void StartIdleAnimations()
        {
            // Flutuação geral do Content Banners (Scale Breathing ao invés de Y) para não dar enjoo
            if (contentBanners != null)
            {
                var rt = contentBanners.GetComponent<RectTransform>();
                if (rt != null)
                {
                    DOVirtual.DelayedCall(0.6f, () => 
                    {
                        rt.DOScale(1.01f, 3f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
                    });
                }
            }

            // Flutuação das Setinhas (Eixo X)
            if (btnNextBanner != null) btnNextBanner.GetComponent<RectTransform>().DOAnchorPosX(10f, 1f).SetRelative(true).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            if (btnPrevBanner != null) btnPrevBanner.GetComponent<RectTransform>().DOAnchorPosX(-10f, 1f).SetRelative(true).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }

        private global::System.Collections.IEnumerator PullButtonsAttentionRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(4f); // Checa a cada 4 segundos
                
                if (AccountManager.Instance != null && AccountManager.Instance.PlayerAccount != null && availableBanners != null && availableBanners.Count > 0)
                {
                    var acc = AccountManager.Instance.PlayerAccount;
                    var banner = availableBanners[currentBannerIndex];
                    
                    if (acc.StarMaps >= banner.CostPerPull * 10 && btnPull10 != null)
                    {
                        // Chacoalha o botão 10x
                        btnPull10.transform.DOPunchScale(Vector3.one * 0.1f, 0.5f, 5);
                        btnPull10.transform.DOPunchRotation(new Vector3(0, 0, 5f), 0.5f, 5);
                    }
                    else if (acc.StarMaps >= banner.CostPerPull && btnPull1 != null)
                    {
                        // Chacoalha o botão 1x
                        btnPull1.transform.DOPunchScale(Vector3.one * 0.1f, 0.5f, 5);
                        btnPull1.transform.DOPunchRotation(new Vector3(0, 0, 5f), 0.5f, 5);
                    }
                }
            }
        }

        private void SwitchTab(bool toBanners)
        {
            float duration = 0.3f;

            // Transição suave de conteúdo (Fade Out / Fade In) ao invés de sumir secamente
            void TransitionContent(GameObject content, bool show)
            {
                if (content == null) return;
                
                var cg = content.GetComponent<CanvasGroup>();
                if (cg == null) cg = content.AddComponent<CanvasGroup>();
                
                var rect = content.GetComponent<RectTransform>();

                if (show)
                {
                    content.SetActive(true);
                    cg.DOFade(1f, duration);
                    if (rect != null) rect.DOScale(1f, duration).From(0.95f).SetEase(Ease.OutBack);
                }
                else
                {
                    cg.DOFade(0f, duration).OnComplete(() => content.SetActive(false));
                    if (rect != null) rect.DOScale(0.95f, duration);
                }
            }

            TransitionContent(contentBanners, toBanners);
            TransitionContent(contentExchange, !toBanners);

            if (toBanners)
            {
                AnimateTab(banner_tab_component, active_tab_root, true, duration);
                AnimateTab(shop_tab_component, inactive_tab_root, false, duration);
            }
            else
            {
                AnimateTab(shop_tab_component, active_tab_root, true, duration);
                AnimateTab(banner_tab_component, inactive_tab_root, false, duration);
            }

            RefreshUI();
        }

        private void AnimateTab(RectTransform tabContent, RectTransform targetRoot, bool isActive, float duration)
        {
            if (tabContent == null || targetRoot == null) return;
            
            // Efeito "Somem e Voltam" (Igual aos Cards do HubScene)
            // A aba some (escala 0), troca de Root fisicamente e volta a crescer no novo lugar.
            float finalScale = isActive ? 1.05f : 0.95f;

            Sequence seq = DOTween.Sequence();
            
            // 1. Encolhe
            seq.Append(tabContent.DOScale(0f, duration * 0.5f).SetEase(Ease.InBack));
            
            // 2. Reparenta no meio da animação, mantendo a posição original intacta na tela
            seq.AppendCallback(() => 
            {
                tabContent.SetParent(targetRoot, true);
            });

            // 3. Cresce de volta
            seq.Append(tabContent.DOScale(finalScale, duration * 0.5f).SetEase(Ease.OutBack));
        }

        public void ChangeBanner(int direction)
        {
            if (availableBanners == null || availableBanners.Count <= 1) return;

            currentBannerIndex += direction;
            if (currentBannerIndex < 0) currentBannerIndex = availableBanners.Count - 1;
            else if (currentBannerIndex >= availableBanners.Count) currentBannerIndex = 0;

            if (bannerSplashArt != null)
            {
                // Swipe Dramático estilo Persona 5
                RectTransform rt = bannerSplashArt.rectTransform;
                
                // Grava o X original exato antes do movimento para não dar pau com o CanvasScaler
                splashOriginalX = rt.anchoredPosition.x;
                
                Sequence swapSeq = DOTween.Sequence();
                
                // Pause no idle enquanto rola o swap
                if (idleSplashTween != null) idleSplashTween.Pause();

                // 1. Gira levemente (Skew P5) e é ejetado pra DIREITA
                swapSeq.Append(rt.DOAnchorPosX(1500f, 0.15f).SetEase(Ease.InBack).SetRelative(true));
                swapSeq.Join(rt.DORotate(new Vector3(0, 0, -5f), 0.15f));

                // 2. Troca o Banner no meio do caminho (invisível pro jogador pois já tá fora)
                swapSeq.AppendCallback(() => 
                {
                    RefreshUI(); // Isso atualiza as sprites e textos
                    rt.anchoredPosition = new Vector2(1500f, rt.anchoredPosition.y); // Set start position far right
                });

                // 3. Volta da DIREITA estalando no lugar ORIGINAL (X)
                swapSeq.Append(rt.DOAnchorPosX(splashOriginalX, 0.35f).SetEase(Ease.OutBack));
                swapSeq.Join(rt.DORotate(Vector3.zero, 0.3f));
                
                // 4. Resume o Idle flutuante
                swapSeq.AppendCallback(() => 
                {
                    if (idleSplashTween != null) idleSplashTween.Play();
                });
            }
            else
            {
                RefreshUI();
            }
        }

        public void RefreshUI()
        {
            if (AccountManager.Instance == null || AccountManager.Instance.PlayerAccount == null) return;

            var acc = AccountManager.Instance.PlayerAccount;
            acc.EnsureInitialized();

            if (starMapsText != null) starMapsText.text = $"{acc.StarMaps}";
            if (stardustText != null) stardustText.text = $"{acc.Stardust}";

            if (contentBanners != null && contentBanners.activeSelf && availableBanners != null && availableBanners.Count > 0)
            {
                // Navegação Dinâmica
                if (btnPrevBanner != null) btnPrevBanner.gameObject.SetActive(currentBannerIndex > 0);
                if (btnNextBanner != null) btnNextBanner.gameObject.SetActive(currentBannerIndex < availableBanners.Count - 1);

                var banner = availableBanners[currentBannerIndex];
                var pity = GachaService.Instance.GetPityState(acc, banner.BannerID);

                if (bannerTitle != null) bannerTitle.text = banner.BannerName;
                if (bannerSplashArt != null) bannerSplashArt.sprite = banner.BannerSplashArt;
                
                if (bannerCostInfo != null) 
                    bannerCostInfo.text = $"{banner.HardPityThreshold - pity.PullsSinceLastSupreme}";
                
                if (pityInfoText != null)
                {
                    string info = $"Garantia 10x (Uncommon+): {banner.GuaranteedAboveBaseEvery - pity.PullsSinceLastOverBase} tiros rest.";
                    if (banner.HasEpitomizedPath)
                        info += $"\n50/50 Anterior foi perdido? {(pity.Lost5050 ? "Sim (Garantido foco)" : "Não")}";
                    pityInfoText.text = info;
                }

                bool canPull1 = acc.StarMaps >= banner.CostPerPull * 1;
                bool canPull10 = acc.StarMaps >= banner.CostPerPull * 10;
                
                if (btnPull1 != null) btnPull1.interactable = canPull1;
                if (btnPull10 != null) btnPull10.interactable = canPull10;
            }
        }

        private async void DoPull(int times)
        {
            if (availableBanners == null || availableBanners.Count == 0) return;
            var banner = availableBanners[currentBannerIndex];
            
            var acc = AccountManager.Instance.PlayerAccount;
            var results = await GachaService.Instance.PerformPullsAsync(acc, banner, times);

            if (results != null && results.Count > 0)
            {
                if (btnPull1 != null) btnPull1.interactable = false;
                if (btnPull10 != null) btnPull10.interactable = false;

                if (animationController != null)
                {
                    animationController.PlayGachaSequence(results, banner.pullVisualConfig, () => OnAnimationFinished(results));
                }
                else
                {
                    // Fallback se não tiver gacha controller
                    OnAnimationFinished(results);
                }
            }
            else
            {
                RefreshUI();
            }
        }

        private void OnAnimationFinished(List<GachaRewardEntry> results)
        {
            RefreshUI();
        }

        private void ConvertStardustToStarMap()
        {
            // Custo placeholder arbitrário, ex: 100 Stardust = 1 StarMap
            var acc = AccountManager.Instance.PlayerAccount;
            if (acc.Stardust >= 100)
            {
                acc.Stardust -= 100;
                acc.StarMaps += 1;
                AccountManager.Instance.SaveAccount();
                RefreshUI();
            }
            else
            {
                Debug.LogWarning("Sem Stardust suficiente (Requer 100)");
            }
        }

        private void ShowBannerDetails()
        {
            Debug.Log("Pressionado botão Detalhes da Pool. (A ser implementado o Modal de Rates!)");
        }
        private void ReturnToHub()
        {
            if (!string.IsNullOrEmpty(hubSceneName))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(hubSceneName);
            }
        }    }
}