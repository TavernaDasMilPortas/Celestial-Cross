using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Cloud;
using CelestialCross.Artifacts;
using System.Text;

namespace CelestialCross.UI
{
    public class HubSceneManager : MonoBehaviour
    {
        public Button generateArtifactBtn;
        public Button generatePetBtn;
        public Button executeGachaBtn;
        public Button configBtn; // Novo botão de Config
        public TextMeshProUGUI logText;

        [global::System.Serializable]
        public class GachaReq { public string BannerId; public int Amount; }

        [global::System.Serializable]
        public class GachaRes { public global::System.Collections.Generic.List<GachaReward> Rewards; }

        [global::System.Serializable]
        public class GachaReward { public string ItemID; public string ItemType; public string Rarity; }

        private void Start()
        {
            if (generateArtifactBtn != null)
                generateArtifactBtn.onClick.AddListener(OnGenerateArtifactClicked);
            
            if (generatePetBtn != null)
                generatePetBtn.onClick.AddListener(OnGeneratePetClicked);

            if (executeGachaBtn != null)
                executeGachaBtn.onClick.AddListener(OnExecuteGachaClicked);
                
            if (configBtn != null)
                configBtn.onClick.AddListener(OnConfigClicked);
        }

        private void OnConfigClicked()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("ConfigScene");
        }

        private async void OnGenerateArtifactClicked()
        {
            Log("Solicitando artefato da Nuvem...");
            SetButtonsInteractable(false);

            var artifact = await CloudArtifactGenerator.GenerateArtifactAsync("Set_Gladiator", ArtifactRarity.Epic, ArtifactStars.Five);
            
            if (artifact != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"<color=#00FF00>Artefato Gerado!</color>");
                sb.AppendLine($"Tipo: {artifact.slot} | Raridade: {artifact.rarity}");
                sb.AppendLine($"Main Stat: {artifact.mainStat.statType} +{artifact.mainStat.value}");
                foreach(var sub in artifact.subStats)
                {
                    sb.AppendLine($"Sub: {sub.statType} +{sub.value}");
                }
                Log(sb.ToString());
            }
            else
            {
                Log("<color=#FF0000>Erro ao gerar artefato.</color>");
            }

            SetButtonsInteractable(true);
        }

        private async void OnGeneratePetClicked()
        {
            Log("Solicitando Pet da Nuvem...");
            SetButtonsInteractable(false);

            var petCatalog = CelestialCross.System.GlobalCatalogs.Instance.petCatalog;
            var poringSO = petCatalog.GetPetSpecies("Pet_Poring");
            var pet = await CloudPetGenerator.GeneratePetAsync(poringSO, 3);
            
            if (pet != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"<color=#00FF00>Pet Gerado!</color>");
                sb.AppendLine($"Espécie: {pet.DisplayName} | {pet.RarityStars} Estrelas");
                sb.AppendLine($"HP: {pet.Health} | ATK: {pet.Attack} | DEF: {pet.Defense}");
                Log(sb.ToString());
            }
            else
            {
                Log("<color=#FF0000>Erro ao gerar pet.</color>");
            }

            SetButtonsInteractable(true);
        }

        private async void OnExecuteGachaClicked()
        {
            Log("Solicitando Gacha Roll da Nuvem...");
            SetButtonsInteractable(false);

            var req = new GachaReq { BannerId = "Standard_Banner", Amount = 1 };
            var result = await PlayFabCloudFunctionCaller.ExecuteFunctionAsync<GachaRes>("ExecuteGacha", req);
            
            if (result != null && result.Rewards != null && result.Rewards.Count > 0)
            {
                var reward = result.Rewards[0];
                string color = reward.Rarity.Contains("Super") ? "#FFD700" : "#FFFFFF";
                
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"<color={color}>Gacha Roll Completado!</color>");
                sb.AppendLine($"Recompensa: {reward.ItemID}");
                sb.AppendLine($"Tipo: {reward.ItemType} | Raridade: {reward.Rarity}");
                Log(sb.ToString());
            }
            else
            {
                Log("<color=#FF0000>Erro no gacha pull.</color>");
            }

            SetButtonsInteractable(true);
        }

        private void Log(string message)
        {
            if (logText != null)
            {
                logText.text = message;
                Debug.Log($"[HubScene] {message}");
            }
        }

        private void SetButtonsInteractable(bool state)
        {
            if (generateArtifactBtn != null) generateArtifactBtn.interactable = state;
            if (generatePetBtn != null) generatePetBtn.interactable = state;
            if (executeGachaBtn != null) executeGachaBtn.interactable = state;
            if (configBtn != null) configBtn.interactable = state;
        }
    }
}
