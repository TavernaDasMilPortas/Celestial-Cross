using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Data.Dungeon;
using CelestialCross.Artifacts;
using CelestialCross.Data.Pets;
using System.Collections.Generic;
using CelestialCross.System;
using System.Collections;
using CelestialCross.Gacha.UI;
using CelestialCross.Scenes.Unit;
using CelestialCross.Gacha;

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
        [SerializeField] private GameObject prizeStampPrefab; // Será o "Stamp" do Gacha

        [Header("Complex Modals")]
        public GachaUnitRewardModal unitPetRewardModal;
        public ArtifactActionModal artifactActionModal;
        public ArtifactUpgradeModal artifactUpgradeModal;

        private global::System.Action onCloseCallback;
        private List<GameObject> spawnedItems = new List<GameObject>();

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
        }

        private void OpenArtifactModal(ArtifactInstanceData arti, GameObject cardGo)
        {
            if (artifactActionModal != null)
            {
                ArtifactSet set = null;
                if (artifactSetCatalog != null) set = artifactSetCatalog.GetSetById(arti.artifactSetId);
                
                artifactActionModal.ShowFromGacha(arti, set, (wasSold) => {
                    if (wasSold && cardGo != null)
                    {
                        var btn = cardGo.GetComponent<Button>();
                        if (btn) btn.interactable = false;

                        var bgImg = cardGo.GetComponent<Image>();
                        if (bgImg) bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
                        
                        var iconImg = cardGo.transform.Find("Icon")?.GetComponent<Image>();
                        if (iconImg) iconImg.color = new Color(1f, 1f, 1f, 0.3f);
                    }
                });
            }
            else
            {
                Debug.LogError("[VictoryRewardUI] artifactActionModal está NULO. Atribua o prefab no Inspector!");
            }
        }
        
        private void OpenPetModal(RuntimePetData pet, GameObject cardGo)
        {
            if (unitPetRewardModal != null)
            {
                var entry = new GachaRewardEntry
                {
                    RewardType = GachaRewardType.Pet,
                    Rarity = (GachaRarity)(Mathf.Clamp(pet.RarityStars - 1, 0, 5)), // Aproximação de raridade
                    PetSpeciesData = petCatalog != null ? petCatalog.GetPetSpecies(pet.SpeciesID) : null,
                    ItemStars = pet.RarityStars
                };
                unitPetRewardModal.ShowPet(pet, entry);
            }
            else
            {
                Debug.LogError("[VictoryRewardUI] unitPetRewardModal está NULO. Atribua o prefab no Inspector!");
            }
        }

        private void OpenUnitModal(UnitData unitData, GameObject cardGo)
        {
            if (unitPetRewardModal != null)
            {
                var entry = new GachaRewardEntry
                {
                    RewardType = GachaRewardType.Unit,
                    Rarity = GachaRarity.Epic, // Assumindo base por enquanto
                    UnitData = unitData,
                    ItemStars = 3
                };
                unitPetRewardModal.ShowUnit(null, entry);
            }
            else
            {
                Debug.LogError("[VictoryRewardUI] unitPetRewardModal está NULO. Atribua o prefab no Inspector!");
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
                string nodeName = "";
                if (GameFlowManager.Instance != null && GameFlowManager.Instance.SelectedStoryNode != null)
                {
                    nodeName = $"\n<size=50%>{GameFlowManager.Instance.SelectedStoryNode.Title}</size>";
                }
                else if (GameFlowManager.Instance != null && GameFlowManager.Instance.SelectedDungeonNode != null && GameFlowManager.Instance.SelectedDungeonNode.LevelRef != null)
                {
                    nodeName = $"\n<size=50%>{GameFlowManager.Instance.SelectedDungeonNode.LevelRef.LevelName}</size>";
                }
                titleTxt.text = isVictory ? $"VITÓRIA!{nodeName}" : $"DERROTA...{nodeName}";
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

            if (reward != null && itemsContent != null && prizeStampPrefab != null)
            {
                if (reward.GeneratedArtifacts != null)
                {
                    foreach (var arti in reward.GeneratedArtifacts)
                    {
                        hasLoot = true;
                        var go = Instantiate(prizeStampPrefab, itemsContent);
                        go.name = "Art_" + arti.idGUID;
                        go.SetActive(true);
                        spawnedItems.Add(go);
                        
                        var nameText = go.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();

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
                                            img.color = Color.white;
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

                        if (nameText != null)
                        {
                            nameText.text = "<color=#" + ColorUtility.ToHtmlStringRGB(rarityColor) + "><b>" + arti.slot + "</b></color>";
                        }
                        
                        Image bg = go.GetComponent<Image>();
                        if (bg != null)
                        {
                            bg.color = new Color(rarityColor.r * 0.3f, rarityColor.g * 0.3f, rarityColor.b * 0.3f, 1f);
                            bg.raycastTarget = true; // Força para poder clicar
                        }

                        // Attach Button
                        Button btn = go.GetComponent<Button>();
                        if (btn == null) btn = go.AddComponent<Button>();
                        btn.interactable = true; // Garante que o botão tá clicável
                        
                        CanvasGroup cg = go.GetComponent<CanvasGroup>();
                        if (cg != null) cg.blocksRaycasts = true;

                        btn.onClick.RemoveAllListeners();
                        
                        var capArti = arti;
                        var capGo = go;
                        btn.onClick.AddListener(() => {
                            Debug.Log($"[VictoryRewardUI] Clicou no artefato: {capArti.slot}");
                            OpenArtifactModal(capArti, capGo);
                        });
                    }
                }

                if (reward.GeneratedPets != null)
                {
                    foreach (var p in reward.GeneratedPets)
                    {
                        hasLoot = true;
                        var go = Instantiate(prizeStampPrefab, itemsContent);
                        go.name = "Pet_" + p.UUID;
                        go.SetActive(true);
                        spawnedItems.Add(go);
                        
                        var nameText = go.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();

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
                                    img.color = Color.white;
                                }
                                iconTransform.gameObject.SetActive(true);
                            }
                        }

                        if (nameText != null)
                        {
                            nameText.text = "<color=#FF8800><b>" + p.DisplayName + "</b></color>";
                        }
                        
                        Image bg = go.GetComponent<Image>();
                        if (bg != null)
                        {
                            bg.color = new Color(0.8f, 0.3f, 0.1f, 1f);
                            bg.raycastTarget = true;
                        }

                        // Attach Button
                        Button btn = go.GetComponent<Button>();
                        if (btn == null) btn = go.AddComponent<Button>();
                        btn.interactable = true;
                        
                        CanvasGroup cg = go.GetComponent<CanvasGroup>();
                        if (cg != null) cg.blocksRaycasts = true;

                        btn.onClick.RemoveAllListeners();
                        
                        var capPet = p;
                        var capGo = go;
                        btn.onClick.AddListener(() => {
                            Debug.Log($"[VictoryRewardUI] Clicou no pet: {capPet.DisplayName}");
                            OpenPetModal(capPet, capGo);
                        });
                    }
                }

                if (reward.SourceDefinitions != null)
                {
                    foreach (var def in reward.SourceDefinitions)
                    {
                        if (def.Type == CelestialCross.Data.Rewards.RewardType.Unit && def.UnitRef != null)
                        {
                            hasLoot = true;
                            var go = Instantiate(prizeStampPrefab, itemsContent);
                            go.name = "Unit_" + def.UnitRef.UnitID;
                            go.SetActive(true);
                            spawnedItems.Add(go);
                            
                            var nameText = go.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();

                            Sprite uIcon = def.UnitRef.icon;
                            if (uIcon != null) {
                                Transform iconTransform = go.transform.Find("Icon");
                                if (iconTransform != null) {
                                    Image img = iconTransform.GetComponent<Image>();
                                    if (img != null) {
                                        img.sprite = uIcon;
                                        img.color = Color.white;
                                    }
                                    iconTransform.gameObject.SetActive(true);
                                }
                            }

                            if (nameText != null)
                            {
                                nameText.text = "<color=#AA00FF><b>" + def.UnitRef.displayName + "</b></color>";
                            }
                            
                            Image bg = go.GetComponent<Image>();
                            if (bg != null)
                            {
                                bg.color = new Color(0.3f, 0.1f, 0.5f, 1f); // Roxo para units
                                bg.raycastTarget = true;
                            }

                            Button btn = go.GetComponent<Button>();
                            if (btn == null) btn = go.AddComponent<Button>();
                            btn.interactable = true;
                            
                            CanvasGroup cg = go.GetComponent<CanvasGroup>();
                            if (cg != null) cg.blocksRaycasts = true;

                            btn.onClick.RemoveAllListeners();
                            
                            var capDef = def;
                            var capGo = go;
                            btn.onClick.AddListener(() => {
                                Debug.Log($"[VictoryRewardUI] Clicou na Unit: {capDef.UnitRef.displayName}");
                                OpenUnitModal(capDef.UnitRef, capGo);
                            });
                        }
                        else if (def.Type == CelestialCross.Data.Rewards.RewardType.Item)
                        {
                            hasLoot = true;
                            var go = Instantiate(prizeStampPrefab, itemsContent);
                            go.name = "Item_" + def.ReferenceID;
                            go.SetActive(true);
                            spawnedItems.Add(go);
                            
                            var nameText = go.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();

                            // Oculta ícone, já que mostramos só texto
                            Transform iconTransform = go.transform.Find("Icon");
                            if (iconTransform != null) {
                                iconTransform.gameObject.SetActive(false);
                            }

                            if (nameText != null)
                            {
                                nameText.text = $"<color=#00FFFF><b>+{def.Amount}\n{def.ReferenceID}</b></color>";
                            }
                            
                            Image bg = go.GetComponent<Image>();
                            if (bg != null) bg.color = new Color(0.1f, 0.3f, 0.4f, 1f); // Azul/Teal para itens
                        }
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
