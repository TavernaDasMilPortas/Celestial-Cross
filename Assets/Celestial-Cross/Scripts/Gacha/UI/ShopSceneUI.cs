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

        [Header("Modal de Resultado")]
        [SerializeField] private GameObject resultModal;
        [SerializeField] private Transform resultGridContent;
        [SerializeField] private GameObject resultItemPrefab;
        [SerializeField] private Button resultCloseBtn;

        [Header("Exchange/Câmbio Placeholder")]
        [SerializeField] private Button btnConvertStardustToStarMaps;

        private void Awake()
        {
            GachaService.Initialize();

            if (tabBannersBtn != null) tabBannersBtn.onClick.AddListener(() => SwitchTab(true));
            if (tabExchangeBtn != null) tabExchangeBtn.onClick.AddListener(() => SwitchTab(false));

            if (btnPull1 != null) btnPull1.onClick.AddListener(() => DoPull(1));
            if (btnPull10 != null) btnPull10.onClick.AddListener(() => DoPull(10));
            if (btnDetails != null) btnDetails.onClick.AddListener(ShowBannerDetails);

            if (resultCloseBtn != null) resultCloseBtn.onClick.AddListener(CloseResultModal);
            
            if (btnConvertStardustToStarMaps != null) btnConvertStardustToStarMaps.onClick.AddListener(ConvertStardustToStarMap);

            if (resultModal != null) resultModal.SetActive(false);
            
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

        private void DoPull(int times)
        {
            if (availableBanners == null || availableBanners.Count == 0) return;
            var banner = availableBanners[currentBannerIndex];
            
            var acc = AccountManager.Instance.PlayerAccount;
            var results = GachaService.Instance.PerformPulls(acc, banner, times);

            if (results != null && results.Count > 0)
            {
                ShowResults(results);
            }
            RefreshUI();
        }

        private void ShowResults(List<GachaRewardEntry> results)
        {
            if (resultModal == null || resultGridContent == null || resultItemPrefab == null) return;

            // Clear old Grid
            foreach (Transform child in resultGridContent)
            {
                if (child.gameObject != resultItemPrefab)
                    Destroy(child.gameObject);
            }

            foreach (var r in results)
            {
                var go = Instantiate(resultItemPrefab, resultGridContent);
                go.SetActive(true);
                var texts = go.GetComponentsInChildren<TextMeshProUGUI>();
                
                string nameShow = "Miss";
                if (r.RewardType == GachaRewardType.Unit) nameShow = r.UnitData != null ? $"[Herói]\n{r.UnitData.displayName}" : $"[Herói]\n???";
                if (r.RewardType == GachaRewardType.Pet) nameShow = r.PetSpeciesData != null ? $"[Pet]\n{r.PetSpeciesData.SpeciesName}" : $"[Pet]\n???";
                if (r.RewardType == GachaRewardType.Artifact && r.ArtifactSet != null) nameShow = $"[Artefato]\n{r.ArtifactSet.setName}";

                string rarityHex = "#FFFFFF";
                if (r.Rarity == GachaRarity.Uncommon) rarityHex = "#00FF00";
                if (r.Rarity == GachaRarity.Rare) rarityHex = "#0088FF";
                if (r.Rarity == GachaRarity.Epic) rarityHex = "#8800FF";
                if (r.Rarity == GachaRarity.Legendary) rarityHex = "#FFAA00";
                if (r.Rarity == GachaRarity.Supreme) rarityHex = "#FF0000";

                if (texts.Length > 0) texts[0].text = $"<color={rarityHex}>{nameShow}</color>";
                if (texts.Length > 1) texts[1].text = r.Rarity.ToString();
            }

            resultModal.SetActive(true);
            resultModal.transform.SetAsLastSibling();
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

        private void CloseResultModal()
        {
            if (resultModal != null) resultModal.SetActive(false);
            RefreshUI();
        }

        private void ShowBannerDetails()
        {
            Debug.Log("Pressionado botão Detalhes da Pool. (A ser implementado o Modal de Rates!)");
        }
    }
}