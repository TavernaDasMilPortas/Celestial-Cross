using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Data.Dungeon;
using CelestialCross.Artifacts;
using CelestialCross.Data.Pets;
using System.Collections.Generic;
using CelestialCross.System;

namespace CelestialCross.Giulia_UI
{
    public class VictoryRewardUI : MonoBehaviour
    {
        public static VictoryRewardUI Instance { get; private set; }

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

        // === Modal Físico (Inspector) ===
        [Header("Reward Details Modal")]
        [SerializeField] private GameObject detailsModal;
        [SerializeField] private TextMeshProUGUI modalTitle;
        [SerializeField] private TextMeshProUGUI modalDesc;
        [SerializeField] private Button modalSellBtn;
        [SerializeField] private TextMeshProUGUI modalSellTxt;
        [SerializeField] private Button modalCloseBtn;
        
        private ArtifactInstanceData currentSelectedArtifact;
        private RuntimePetData currentSelectedPet;

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
                
            if (modalSellBtn != null) 
                modalSellBtn.onClick.AddListener(OnSellClicked);

            if (modalCloseBtn != null) 
                modalCloseBtn.onClick.AddListener(() => { if (detailsModal != null) detailsModal.SetActive(false); });

            if (detailsModal != null)
                detailsModal.SetActive(false);
        }

        private void OpenArtifactModal(ArtifactInstanceData arti, GameObject cardGo)
        {
            currentSelectedArtifact = arti;
            currentSelectedPet = null;
            
            string setString = string.IsNullOrWhiteSpace(arti.artifactSetId) ? "" : $" ({arti.artifactSetId})";
            modalTitle.text = $"{arti.slot}{setString}";
            
            string mStat = arti.mainStat != null ? $"+{arti.mainStat.value:F0} {arti.mainStat.statType}" : "none";
            modalDesc.text = $"Raridade: {arti.rarity}\nLevel: {arti.currentLevel}\nEstrelas: {arti.stars}*\n\nMain Stat: {mStat}";
            
            int sellValue = ArtifactEconomyService.GetSellValue(arti);
            modalSellTxt.text = $"VENDER\n(+{sellValue} Moedas)";
            
            detailsModal.SetActive(true);
            detailsModal.transform.SetAsLastSibling(); // Puxa pra frente
        }
        
        private void OpenPetModal(RuntimePetData pet, GameObject cardGo)
        {
            currentSelectedArtifact = null;
            currentSelectedPet = pet;
            
            modalTitle.text = pet.DisplayName;
            modalDesc.text = $"Estrelas: {pet.RarityStars}*\nNível: {pet.CurrentLevel}\nHP: {pet.Health} | ATK: {pet.Attack} | DEF: {pet.Defense}\nSPD: {pet.Speed} | CRIT: {pet.CriticalChance}% | ACC: {pet.EffectAccuracy}%";
            
            modalSellTxt.text = "SOLTAR\n(+ Pet Souls)";
            
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
                Debug.LogWarning("[VictoryRewardUI] Nenhuma instância encontrada. Fallback.");
                onClose?.Invoke();
                return;
            }
            Instance.SetupAndShow(reward, onClose, isVictory);
        }

        private void SetupAndShow(RuntimeReward reward, global::System.Action onClose, bool isVictory)
        {
            this.onCloseCallback = onClose;

            if (rootContainer != null)
                rootContainer.SetActive(true);

            TMP_Text[] allTexts = rootContainer.GetComponentsInChildren<TMP_Text>();
            foreach(var t in allTexts) {
                // filter explicitly by name to avoid hijacking my modal texts
                if (t.name.Contains("Title") || t.text.Contains("VIT") || t.text.Contains("Vit") || t.text.Contains("DERROTA")) {
                    if (t.transform.parent != detailsModal.transform) {
                        t.text = isVictory ? "VITÓRIA!" : "DERROTA...";
                        t.color = isVictory ? Color.yellow : Color.red;
                    }
                }
            }

            if (moneyAndEnergyText != null) {
                if (reward != null) {
                    moneyAndEnergyText.text = "Dinheiro: <color=#00FF00>+" + reward.Money + "</color>   Energia: <color=#00FFFF>+" + reward.Energy + "</color>";
                    if (reward.Stardust > 0) moneyAndEnergyText.text += "   Poeira: <color=#FFAA00>+" + reward.Stardust + "</color>";
                } else {
                    moneyAndEnergyText.text = "Sorte na próxima!";
                }
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
                            string setString = string.IsNullOrWhiteSpace(arti.artifactSetId) ? "" : " (" + arti.artifactSetId + ")";
                            texts[0].text = "<color=#" + ColorUtility.ToHtmlStringRGB(rarityColor) + ">" + arti.slot + setString + "</color>\n" + arti.GetStarsAsIntClamped() + "* Lv." + arti.currentLevel;
                        }
                        if (texts.Length > 1)
                        {
                            string statLine = "sem main stats";
                            if (arti.mainStat != null) statLine = "+" + arti.mainStat.value.ToString("F0") + " " + arti.mainStat.statType;
                            texts[1].text = statLine;
                            texts[1].color = Color.yellow;
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
                        
                        if (texts.Length > 0)
                        {
                            texts[0].text = "<color=#FF8800>NOVO PET!</color>\n" + p.DisplayName + " " + p.RarityStars + "*";
                        }
                        if (texts.Length > 1)
                        {
                            texts[1].text = "HP: " + p.Health + " ATK: " + p.Attack;
                            texts[1].color = Color.white;
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
    }
}





