using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Data.Dungeon;
using CelestialCross.Artifacts;
using CelestialCross.Data.Pets;
using System.Collections.Generic;
using CelestialCross.System;
using System.Collections;

namespace CelestialCross.Giulia_UI
{
    public class VictoryRewardUI : MonoBehaviour
    {
        public static VictoryRewardUI Instance { get; private set; }

        [Header("Data Catalogs")]
        public ArtifactSetCatalog artifactSetCatalog;
        public PetCatalog petCatalog;
        public UnitCatalog unitCatalog;
        public LevelingConfig levelingConfig;

        [Header("Containers")]
        [SerializeField] private GameObject rootContainer;
        [SerializeField] private Transform itemsContent;

        [Header("Text Elements")]
        [SerializeField] private TMP_Text moneyAndEnergyText;
        [SerializeField] private TMP_Text noArtifactsText;

        [Header("Controls")]
        [SerializeField] private Button continueButton;

        [Header("Prefabs")]
        [SerializeField] private GameObject artifactItemPrefab;

        private global::System.Action onCloseCallback;
        private List<GameObject> spawnedItems = new List<GameObject>();

        // === Modal Fsico (Inspector) ===
        [Header("Reward Details Modal")]
        [SerializeField] private GameObject detailsModal;
        [SerializeField] private TextMeshProUGUI modalTitle;
        [SerializeField] private TextMeshProUGUI modalDesc;
        [SerializeField] private Button modalSellBtn;
        [SerializeField] private TextMeshProUGUI modalSellTxt;
        [SerializeField] private Button modalCloseBtn;
        
        private ArtifactInstanceData currentSelectedArtifact;
        private RuntimePetData currentSelectedPet;

        [Header("XP Panel (Phase 1)")]
        [SerializeField] private Transform xpSlotsPanel;
        [SerializeField] private GameObject xpSlotPrefab;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            if (rootContainer != null)
                rootContainer.SetActive(false);

            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinueClicked);

            if (detailsModal != null)
            {
                AutoLinkModalComponents();

                if (modalSellBtn != null) 
                    modalSellBtn.onClick.AddListener(OnSellClicked);

                if (modalCloseBtn != null) 
                    modalCloseBtn.onClick.AddListener(() => { if (detailsModal != null) detailsModal.SetActive(false); });

                detailsModal.SetActive(false);
            }
        }

        private void AutoLinkModalComponents()
        {
            if (modalTitle == null) {
                var t = detailsModal.transform.Find("ModalTitle");
                if (t != null) modalTitle = t.GetComponent<TextMeshProUGUI>();
            }
            if (modalDesc == null) {
                var t = detailsModal.transform.Find("ModalDesc");
                if (t != null) modalDesc = t.GetComponent<TextMeshProUGUI>();
            }

            if (modalSellBtn == null) {
                var btnTr = detailsModal.transform.Find("Generated_Btn_Sell");
                if (btnTr == null) btnTr = detailsModal.transform.Find("Btn_Sell");
                if (btnTr != null) {
                    modalSellBtn = btnTr.GetComponent<Button>();
                    var txtTr = btnTr.Find("Text");
                    if (txtTr != null) modalSellTxt = txtTr.GetComponent<TextMeshProUGUI>();
                }
            }

            if (modalCloseBtn == null) {
                var btnTr = detailsModal.transform.Find("Generated_Btn_Close");
                if (btnTr == null) btnTr = detailsModal.transform.Find("Btn_Close");
                if (btnTr != null) modalCloseBtn = btnTr.GetComponent<Button>();
            }

            // Gerar botões dinamicamente se não existem
            if (modalSellBtn == null || modalCloseBtn == null)
            {
                // Vender Btn
                GameObject sBtnGo = new GameObject("Generated_Btn_Sell", typeof(RectTransform), typeof(Image), typeof(Button));
                sBtnGo.transform.SetParent(detailsModal.transform, false);
                RectTransform sRt = sBtnGo.GetComponent<RectTransform>();
                sRt.anchorMin = new Vector2(0.1f, 0.05f); sRt.anchorMax = new Vector2(0.45f, 0.15f);
                sRt.offsetMin = Vector2.zero; sRt.offsetMax = Vector2.zero;
                sBtnGo.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f, 1f);
                
                modalSellBtn = sBtnGo.GetComponent<Button>();
                
                GameObject sTxtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                sTxtGo.transform.SetParent(sBtnGo.transform, false);
                RectTransform stRt = sTxtGo.GetComponent<RectTransform>();
                stRt.anchorMin = Vector2.zero; stRt.anchorMax = Vector2.one;
                stRt.offsetMin = Vector2.zero; stRt.offsetMax = Vector2.zero;
                modalSellTxt = sTxtGo.GetComponent<TextMeshProUGUI>();
                modalSellTxt.alignment = TextAlignmentOptions.Center;
                modalSellTxt.color = Color.white;
                modalSellTxt.fontSize = 20;

                // Fechar Btn
                GameObject cBtnGo = new GameObject("Generated_Btn_Close", typeof(RectTransform), typeof(Image), typeof(Button));
                cBtnGo.transform.SetParent(detailsModal.transform, false);
                RectTransform cRt = cBtnGo.GetComponent<RectTransform>();
                cRt.anchorMin = new Vector2(0.55f, 0.05f); cRt.anchorMax = new Vector2(0.9f, 0.15f);
                cRt.offsetMin = Vector2.zero; cRt.offsetMax = Vector2.zero;
                cBtnGo.GetComponent<Image>().color = new Color(0.4f, 0.4f, 0.4f, 1f);

                modalCloseBtn = cBtnGo.GetComponent<Button>();
                
                GameObject cTxtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                cTxtGo.transform.SetParent(cBtnGo.transform, false);
                RectTransform ctRt = cTxtGo.GetComponent<RectTransform>();
                ctRt.anchorMin = Vector2.zero; ctRt.anchorMax = Vector2.one;
                ctRt.offsetMin = Vector2.zero; ctRt.offsetMax = Vector2.zero;
                var cTxt = cTxtGo.GetComponent<TextMeshProUGUI>();
                cTxt.text = "Fechar";
                cTxt.alignment = TextAlignmentOptions.Center;
                cTxt.color = Color.white;
                cTxt.fontSize = 20;
            }
        }

        private void OpenArtifactModal(ArtifactInstanceData arti, GameObject cardGo)
        {
            currentSelectedArtifact = arti;
            currentSelectedPet = null;
            
            string setString = string.IsNullOrWhiteSpace(arti.artifactSetId) ? "" : $" ({arti.artifactSetId})";
            if (modalTitle != null) modalTitle.text = $"{arti.slot}{setString}";
            
            string mStat = arti.mainStat != null ? $"+{arti.mainStat.value:F0} {arti.mainStat.statType}" : "none";
            if (modalDesc != null) modalDesc.text = $"Raridade: {arti.rarity}\nLevel: {arti.currentLevel}\nEstrelas: {arti.stars}*\n\nMain Stat: {mStat}";
            
            int sellValue = ArtifactEconomyService.GetSellValue(arti);
            if (modalSellTxt != null) modalSellTxt.text = $"VENDER\n(+{sellValue} Moedas)";
            
            Sprite iconSprite = null;
            if (cardGo != null) {
                Transform iTr = cardGo.transform.Find("Icon");
                if (iTr != null) {
                    Image img = iTr.GetComponent<Image>();
                    if (img != null) iconSprite = img.sprite;
                }
            }
            Transform mIconTr = detailsModal.transform.Find("ModalIcon");
            if (mIconTr != null)
            {
                if (iconSprite != null) {
                    mIconTr.gameObject.SetActive(true);
                    var img = mIconTr.GetComponent<Image>();
                    if (img != null) img.sprite = iconSprite;
                } else {
                    mIconTr.gameObject.SetActive(false);
                }
            }

            detailsModal.SetActive(true);
            detailsModal.transform.SetAsLastSibling(); // Puxa pra frente
        }
        
        private void OpenPetModal(RuntimePetData pet, GameObject cardGo)
        {
            currentSelectedArtifact = null;
            currentSelectedPet = pet;
            
            if (modalTitle != null) modalTitle.text = pet.DisplayName;
            if (modalDesc != null) modalDesc.text = $"Estrelas: {pet.RarityStars}*\nNível: {pet.CurrentLevel}\nHP: {pet.Health} | ATK: {pet.Attack} | DEF: {pet.Defense}\nSPD: {pet.Speed} | CRIT: {pet.CriticalChance}% | ACC: {pet.EffectAccuracy}%";
            
            if (modalSellTxt != null) modalSellTxt.text = "SOLTAR\n(+ Pet Souls)";
            
            Sprite iconSprite = null;
            if (cardGo != null) {
                Transform iTr = cardGo.transform.Find("Icon");
                if (iTr != null) {
                    Image img = iTr.GetComponent<Image>();
                    if (img != null) iconSprite = img.sprite;
                }
            }
            Transform mIconTr = detailsModal.transform.Find("ModalIcon");
            if (mIconTr != null)
            {
                if (iconSprite != null) {
                    mIconTr.gameObject.SetActive(true);
                    var img = mIconTr.GetComponent<Image>();
                    if (img != null) img.sprite = iconSprite;
                } else {
                    mIconTr.gameObject.SetActive(false);
                }
            }

            detailsModal.SetActive(true);
            detailsModal.transform.SetAsLastSibling();
        }

        private void OnSellClicked()
        {
            var acc = AccountManager.Instance.PlayerAccount;
            if (currentSelectedArtifact != null)
            {
                // Refund money via TrySellArtifact which removes artifact and gives money
                if (ArtifactEconomyService.TrySellArtifact(acc, currentSelectedArtifact))
                {
                    AccountManager.Instance.SaveAccount();
                    // Remove from visually spawned list (reload UI?) Wait, Victory UI is static, so just hide the button
                    foreach(var go in spawnedItems.ToArray()) {
                        if (go.name.Contains(currentSelectedArtifact.idGUID)) {
                            go.SetActive(false); // Hide the card
                        }
                    }
                    detailsModal.SetActive(false);
                }
            }
            else if (currentSelectedPet != null)
            {
                // Release Pet
                System.PetReleaseManager.Instance.ReleasePet(currentSelectedPet.UUID);
                foreach(var go in spawnedItems.ToArray()) {
                    if (go.name.Contains(currentSelectedPet.UUID)) {
                        go.SetActive(false); // Hide the card
                    }
                }
                detailsModal.SetActive(false);
            }
        }


        private void OnContinueClicked()
        {
            if (rootContainer != null)
                rootContainer.SetActive(false);

            var tempCb = onCloseCallback;
            onCloseCallback = null;
            tempCb?.Invoke();
        }

        public static void ShowVictoryUI(RuntimeReward reward, global::System.Action onClose, bool isVictory = true)
        {
            if (Instance == null)
            {
                Debug.LogWarning("[VictoryRewardUI] Nenhuma instncia encontrada. Fallback.");
                onClose?.Invoke();
                return;
            }
            Instance.SetupAndShow(reward, onClose, isVictory);
        }

        public static void ShowVictoryUIWithXP(RuntimeReward reward, Dictionary<string, XPGainResult> xpResults, global::System.Action onClose, bool isVictory = true)
        {
            if (Instance == null)
            {
                onClose?.Invoke();
                return;
            }
            Instance.SetupAndShow(reward, onClose, isVictory, xpResults);
        }

        private void SetupAndShow(RuntimeReward reward, global::System.Action onClose, bool isVictory, Dictionary<string, XPGainResult> xpResults = null)
        {
            this.onCloseCallback = onClose;

            if (rootContainer != null)
                rootContainer.SetActive(true);

            TMP_Text[] allTexts = rootContainer.GetComponentsInChildren<TMP_Text>();
            // Update Title
            var titleTxt = rootContainer.transform.Find("MainScrollView/Viewport/Content/ModalTitle")?.GetComponent<TextMeshProUGUI>();
            if (titleTxt != null)
            {
                titleTxt.text = isVictory ? "VITÓRIA!" : "DERROTA...";
                titleTxt.color = isVictory ? Color.yellow : Color.red;
            }

            if (moneyAndEnergyText != null) {
                if (reward != null) {
                    string resList = $"- Dinheiro: <color=#00FF00>+{reward.Money}</color>\n";
                    resList += $"- Energia: <color=#00FFFF>+{reward.Energy}</color>\n";
                    if (reward.Stardust > 0) resList += $"- Poeira: <color=#FFAA00>+{reward.Stardust}</color>\n";
                    if (reward.XP > 0) resList += $"- XP Equipe: <color=#00FFFF>+{reward.XP}</color>";
                    moneyAndEnergyText.text = resList;
                } else {
                    moneyAndEnergyText.text = "Nenhum recurso obtido.";
                }
            }

            if (xpResults != null && xpSlotsPanel != null && xpSlotPrefab != null)
            {
                // Limpar slots antigos de XP se houver (mas ignora o template se ele estiver no painel)
                foreach (Transform child in xpSlotsPanel) 
                {
                    if (child.gameObject != xpSlotPrefab)
                        Destroy(child.gameObject);
                }
                StartCoroutine(AnimateXPBars(xpResults));
            }

            foreach (var go in spawnedItems)
                Destroy(go);
            spawnedItems.Clear();

            bool hasLoot = false;

            if (reward != null && itemsContent != null && artifactItemPrefab != null)
            {
                if (reward.GeneratedArtifacts != null)
                {
                    foreach (var arti in reward.GeneratedArtifacts)
                    {
                        hasLoot = true;
                        var go = Instantiate(artifactItemPrefab, itemsContent);
                        go.name = "Art_" + arti.idGUID;
                        go.SetActive(true);
                        spawnedItems.Add(go);
                        TMP_Text[] texts = go.GetComponentsInChildren<TMP_Text>();

                        if (artifactSetCatalog != null)
                        {
                            var set = artifactSetCatalog.GetSetById(arti.artifactSetId);
                            if (set != null)
                            {
                                Sprite iconSprite = set.GetIconForSlot(arti.slot);
                                if (iconSprite != null)
                                {
                                    Transform iconTransform = go.transform.Find("Icon");
                                    if (iconTransform != null) {
                                        Image img = iconTransform.GetComponent<Image>();
                                        if (img != null) {
                                            img.sprite = iconSprite;
                                        }
                                        iconTransform.gameObject.SetActive(true);
                                        }
                                    }
                            }
                        }

                        Color rarityColor = Color.gray;
                        switch(arti.rarity)
                        {
                            case ArtifactRarity.Common: rarityColor = Color.white; break;
                            case ArtifactRarity.Uncommon: rarityColor = new Color(0.2f, 0.8f, 0.2f); break;
                            case ArtifactRarity.Rare: rarityColor = new Color(0.2f, 0.5f, 1f); break;
                            case ArtifactRarity.Epic: rarityColor = new Color(0.8f, 0f, 0.8f); break;
                            case ArtifactRarity.Legendary: rarityColor = new Color(1f, 0.6f, 0f); break;
                        }

                        if (texts.Length > 0)
                        {
                            texts[0].text = "<color=#" + ColorUtility.ToHtmlStringRGB(rarityColor) + "><b>" + arti.slot + "</b></color>";
                            texts[0].alignment = TextAlignmentOptions.Bottom;
                            texts[0].enableWordWrapping = false;
                        }
                        if (texts.Length > 1)
                        {
                            texts[1].gameObject.SetActive(false);
                        }
                        Image bg = go.GetComponent<Image>();
                        if (bg != null) bg.color = new Color(rarityColor.r * 0.3f, rarityColor.g * 0.3f, rarityColor.b * 0.3f, 1f);

                        // Attach Button
                        Button btn = go.GetComponent<Button>();
                        if (btn == null) btn = go.AddComponent<Button>();
                        btn.onClick.AddListener(() => OpenArtifactModal(arti, go));
                    }
                }

                if (reward.GeneratedPets != null)
                {
                    foreach (var p in reward.GeneratedPets)
                    {
                        hasLoot = true;
                        var go = Instantiate(artifactItemPrefab, itemsContent);
                        go.name = "Pet_" + p.UUID;
                        go.SetActive(true);
                        spawnedItems.Add(go);
                        TMP_Text[] texts = go.GetComponentsInChildren<TMP_Text>();

                        Sprite petIcon = null;
                        if (petCatalog != null) {
                            var speciesData = petCatalog.GetPetSpecies(p.SpeciesID);
                            if (speciesData != null) petIcon = speciesData.Icon;
                        }

                        if (petIcon != null) {
                            Transform iconTransform = go.transform.Find("Icon");
                            if (iconTransform != null) {
                                Image img = iconTransform.GetComponent<Image>();
                                if (img != null) {
                                    img.sprite = petIcon;
                                }
                                iconTransform.gameObject.SetActive(true);
                            }
                        }

                        if (texts.Length > 0)
                        {
                            texts[0].text = "<color=#FF8800><b>" + p.DisplayName + "</b></color>";
                            texts[0].alignment = TextAlignmentOptions.Bottom;
                            texts[0].enableWordWrapping = false;
                        }
                        if (texts.Length > 1)
                        {
                            texts[1].gameObject.SetActive(false);
                        }
                        Image bg = go.GetComponent<Image>();
                        if (bg != null) bg.color = new Color(0.8f, 0.3f, 0.1f, 1f);

                        // Attach Button
                        Button btn = go.GetComponent<Button>();
                        if (btn == null) btn = go.AddComponent<Button>();
                        btn.onClick.AddListener(() => OpenPetModal(p, go));
                    }
                }
            }

            if (noArtifactsText != null)
                noArtifactsText.gameObject.SetActive(!hasLoot);
        }

        private IEnumerator AnimateXPBars(Dictionary<string, XPGainResult> results)
        {
            if (levelingConfig == null) yield break;

            var animators = new List<IEnumerator>();

            foreach (var kvp in results)
            {
                var result = kvp.Value;
                var slot = Instantiate(xpSlotPrefab, xpSlotsPanel);
                slot.SetActive(true);
                
                var levelTxt = slot.transform.Find("InfoColumn/LevelText")?.GetComponent<TextMeshProUGUI>();
                var barFill = slot.transform.Find("InfoColumn/XP_Bar_BG/Fill")?.GetComponent<Image>();
                var iconImg = slot.transform.Find("UnitIcon")?.GetComponent<Image>();

                if (iconImg != null && unitCatalog != null)
                {
                    var unitData = unitCatalog.GetUnitData(kvp.Key);
                    if (unitData != null) iconImg.sprite = unitData.icon;
                }

                animators.Add(AnimateSingleXPBar(result, levelTxt, barFill));
            }

            var coroutines = new List<Coroutine>();
            foreach (var anim in animators)
            {
                coroutines.Add(StartCoroutine(anim));
            }

            foreach(var c in coroutines) {
                yield return c;
            }
        }

        private IEnumerator AnimateSingleXPBar(XPGainResult result, TextMeshProUGUI levelTxt, Image barFill)
        {
            float totalAnimationTime = 1.5f;
            
            int currentAnimLevel = result.oldLevel;
            int currentSimXP = result.oldXP;
            int xpRemainingToAnim = result.xpGained;
            
            if (xpRemainingToAnim == 0)
            {
                int xpToNext = levelingConfig.GetXPForNextLevel(currentAnimLevel);
                if (barFill != null) barFill.fillAmount = xpToNext > 0 ? (float)currentSimXP / xpToNext : 1f;
                if (levelTxt != null) levelTxt.text = $"Lv. {currentAnimLevel} ({currentSimXP}/{xpToNext}) <color=yellow>+0 XP</color>";
                yield break;
            }

            while (xpRemainingToAnim > 0)
            {
                int xpToNext = levelingConfig.GetXPForNextLevel(currentAnimLevel);
                
                // If max level reached
                if (xpToNext == 0)
                {
                    if (barFill != null) barFill.fillAmount = 1f;
                    if (levelTxt != null) levelTxt.text = $"Lv. {currentAnimLevel} (MAX) <color=yellow>+{result.xpGained} XP</color>";
                    yield break;
                }

                int xpNeededForLevelUp = xpToNext - currentSimXP;
                
                if (xpRemainingToAnim >= xpNeededForLevelUp)
                {
                    // Level Up animation segment
                    float portion = (float)xpNeededForLevelUp / result.xpGained;
                    float timeForThisSegment = totalAnimationTime * portion;
                    if (timeForThisSegment < 0.1f) timeForThisSegment = 0.1f;
                    
                    float elapsed = 0;
                    float startFill = (float)currentSimXP / xpToNext;
                    while (elapsed < timeForThisSegment)
                    {
                        elapsed += Time.deltaTime;
                        float t = elapsed / timeForThisSegment;
                        
                        if (barFill != null) barFill.fillAmount = Mathf.Lerp(startFill, 1f, t);
                        if (levelTxt != null) {
                            int displayXP = Mathf.RoundToInt(Mathf.Lerp(currentSimXP, xpToNext, t));
                            levelTxt.text = $"Lv. {currentAnimLevel} ({displayXP}/{xpToNext}) <color=yellow>+{result.xpGained} XP</color>";
                        }
                        yield return null;
                    }
                    
                    if (barFill != null) barFill.fillAmount = 1f;
                    
                    currentAnimLevel++;
                    xpRemainingToAnim -= xpNeededForLevelUp;
                    currentSimXP = 0;
                    
                    if (levelTxt != null) levelTxt.text = $"Lv. {currentAnimLevel} (0/{levelingConfig.GetXPForNextLevel(currentAnimLevel)}) <color=#00FF00>LEVEL UP!</color>";
                    if (barFill != null) barFill.fillAmount = 0f;
                    
                    yield return new WaitForSeconds(0.2f);
                }
                else
                {
                    // Partial fill animation segment
                    float portion = (float)xpRemainingToAnim / result.xpGained;
                    float timeForThisSegment = totalAnimationTime * portion;
                    if (timeForThisSegment < 0.1f) timeForThisSegment = 0.1f;
                    
                    int targetXP = currentSimXP + xpRemainingToAnim;
                    
                    float elapsed = 0;
                    float startFill = (float)currentSimXP / xpToNext;
                    float endFill = (float)targetXP / xpToNext;
                    
                    while (elapsed < timeForThisSegment)
                    {
                        elapsed += Time.deltaTime;
                        float t = elapsed / timeForThisSegment;
                        
                        if (barFill != null) barFill.fillAmount = Mathf.Lerp(startFill, endFill, t);
                        if (levelTxt != null) {
                            int displayXP = Mathf.RoundToInt(Mathf.Lerp(currentSimXP, targetXP, t));
                            levelTxt.text = $"Lv. {currentAnimLevel} ({displayXP}/{xpToNext}) <color=yellow>+{result.xpGained} XP</color>";
                        }
                        yield return null;
                    }
                    
                    currentSimXP = targetXP;
                    xpRemainingToAnim = 0;
                }
            }
            
            // Final explicit state ensurement
            if (barFill != null) {
                int finalXPToNext = levelingConfig.GetXPForNextLevel(result.newLevel);
                if (finalXPToNext > 0)
                    barFill.fillAmount = (float)result.currentXP / finalXPToNext;
                else
                    barFill.fillAmount = 1f;
            }
            if (levelTxt != null) {
                levelTxt.text = $"Lv. {result.newLevel} ({result.currentXP}/{result.xpToNextLevel}) <color=yellow>+{result.xpGained} XP</color>";
            }
        }
    }
}
