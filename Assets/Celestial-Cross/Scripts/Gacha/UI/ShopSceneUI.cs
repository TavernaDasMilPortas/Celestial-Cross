using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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
        [SerializeField] private TextMeshProUGUI moneyText;

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

        [Header("Animação do Gacha")]
        [SerializeField] private GachaAnimationController animationController;

        [Header("Exchange/Câmbio Placeholder")]
        [SerializeField] private Button btnConvertStardustToStarMaps;

        [Header("NavegaÃ§Ã£o")]
        [SerializeField] private Button btnBackToHub;
        [SerializeField] private string hubSceneName = "HubScene";

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

            SwitchTab(true);
        }

        private void Start()
        {
            RefreshUI();
        }

        private void SwitchTab(bool toBanners)
        {
            if (contentBanners != null) contentBanners.SetActive(toBanners);
            if (contentExchange != null) contentExchange.SetActive(!toBanners);
            RefreshUI();
        }

        public void RefreshUI()
        {
            if (AccountManager.Instance == null || AccountManager.Instance.PlayerAccount == null) return;

            var acc = AccountManager.Instance.PlayerAccount;
            acc.EnsureInitialized();

            if (starMapsText != null) starMapsText.text = $"Mapas: {acc.StarMaps}";
            if (stardustText != null) stardustText.text = $"Poeira: {acc.Stardust}";
            if (moneyText != null) moneyText.text = $"Dinheiro: {acc.Money}";

            if (contentBanners != null && contentBanners.activeSelf && availableBanners != null && availableBanners.Count > 0)
            {
                var banner = availableBanners[currentBannerIndex];
                var pity = GachaService.Instance.GetPityState(acc, banner.BannerID);

                if (bannerTitle != null) bannerTitle.text = banner.BannerName;
                if (bannerSplashArt != null) bannerSplashArt.sprite = banner.BannerSplashArt;
                
                if (bannerCostInfo != null) 
                    bannerCostInfo.text = $"Custo Base: {banner.CostPerPull} Mapas/Tiro\nPulls até Supremo Garantido: {banner.HardPityThreshold - pity.PullsSinceLastSupreme}";
                
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
                    animationController.PlayGachaSequence(results, () => OnAnimationFinished(results));
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