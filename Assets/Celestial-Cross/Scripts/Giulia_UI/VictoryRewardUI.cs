using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Data.Dungeon;
using CelestialCross.Artifacts;
using System.Collections.Generic;

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

        private void OnContinueClicked()
        {
            if (rootContainer != null)
                rootContainer.SetActive(false);

            var tempCb = onCloseCallback;
            onCloseCallback = null;
            tempCb?.Invoke();
        }

        public static void ShowVictoryUI(RuntimeReward reward, global::System.Action onClose)
        {
            if (Instance == null)
            {
                Debug.LogWarning("[VictoryRewardUI] Nenhuma instância de VictoryRewardUI encontrada na cena. Fallback para retorno direto.");
                onClose?.Invoke();
                return;
            }

            Instance.SetupAndShow(reward, onClose);
        }

        private void SetupAndShow(RuntimeReward reward, global::System.Action onClose)
        {
            this.onCloseCallback = onClose;

            if (rootContainer != null)
                rootContainer.SetActive(true);

            if (moneyAndEnergyText != null)
                moneyAndEnergyText.text = $"Dinheiro: <color=#00FF00>+{reward.Money}</color>   Energia: <color=#00FFFF>+{reward.Energy}</color>";

            // Limpa grid anterior
            foreach (var go in spawnedItems)
                Destroy(go);
            spawnedItems.Clear();

            if (reward.GeneratedArtifacts == null || reward.GeneratedArtifacts.Count == 0)
            {
                if (noArtifactsText != null)
                    noArtifactsText.gameObject.SetActive(true);
            }
            else
            {
                if (noArtifactsText != null)
                    noArtifactsText.gameObject.SetActive(false);

                if (itemsContent != null && artifactItemPrefab != null)
                {
                    foreach (var arti in reward.GeneratedArtifacts)
                    {
                        var go = Instantiate(artifactItemPrefab, itemsContent);
                        spawnedItems.Add(go);

                        // Como é um objeto genérico no MVP, procuramos componentes de texto lá dentro
                        // O UIBuilder vai criar o prefab com dois textos: 0 = Title, 1 = Stat
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
                            string setString = string.IsNullOrWhiteSpace(arti.artifactSetId) ? "" : $" ({arti.artifactSetId})";
                            texts[0].text = $"<color=#{ColorUtility.ToHtmlStringRGB(rarityColor)}>{arti.slot}{setString}</color>\n{arti.GetStarsAsIntClamped()}* Lv.{arti.currentLevel}";
                        }
                        if (texts.Length > 1)
                        {
                            string statLine = "sem main stats";
                            if (arti.mainStat != null) statLine = $"+{arti.mainStat.value:F0} {arti.mainStat.statType}";
                            texts[1].text = statLine;
                            texts[1].color = Color.yellow;
                        }

                        // Colore fundo
                        Image bg = go.GetComponent<Image>();
                        if (bg != null)
                            bg.color = new Color(rarityColor.r * 0.3f, rarityColor.g * 0.3f, rarityColor.b * 0.3f, 1f);
                    }
                }
            }
        }
    }
}
