using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CelestialCross.UI;

namespace CelestialCross.EditorScripts
{
    public class HubSceneUIBuilder : EditorWindow
    {
        [MenuItem("Celestial Cross/UI/Build Hub Scene (Somente Painel de Teste)")]
        public static void BuildHubSceneUI()
        {
            Debug.Log("[HubSceneUIBuilder] Injetando painel de testes no Hub...");

            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("Não encontrei um Canvas! Você precisa abrir a cena do Hub primeiro.");
                return;
            }

            // Remove o painel antigo se houver
            Transform existingHubPanel = canvas.transform.Find("CloudTestPanel");
            if (existingHubPanel != null)
            {
                DestroyImmediate(existingHubPanel.gameObject);
            }

            // Cria um painel flutuante de debug no canto da tela
            RectTransform hubPanel = UIBuilderHelper.CreatePanel(canvas.transform, "CloudTestPanel", new Color(0.15f, 0.15f, 0.15f, 0.8f));
            hubPanel.anchorMin = new Vector2(0.6f, 0.6f);
            hubPanel.anchorMax = new Vector2(0.95f, 0.95f);
            hubPanel.offsetMin = Vector2.zero;
            hubPanel.offsetMax = Vector2.zero;
            hubPanel.SetAsLastSibling(); // Põe por cima

            // Título
            TextMeshProUGUI titleText = UIBuilderHelper.CreateText(hubPanel, "TitleText", "TESTES DO PLAYFAB", 30, Color.white, TextAlignmentOptions.Center);
            RectTransform titleRect = titleText.rectTransform;
            titleRect.anchorMin = new Vector2(0.1f, 0.85f);
            titleRect.anchorMax = new Vector2(0.9f, 0.95f);
            titleRect.anchoredPosition = Vector2.zero;
            titleRect.sizeDelta = Vector2.zero;

            // Fundo do Log
            RectTransform logBgPanel = UIBuilderHelper.CreatePanel(hubPanel, "LogBg", new Color(0, 0, 0, 0.5f));
            logBgPanel.anchorMin = new Vector2(0.05f, 0.4f);
            logBgPanel.anchorMax = new Vector2(0.95f, 0.8f);
            logBgPanel.anchoredPosition = Vector2.zero;
            logBgPanel.sizeDelta = Vector2.zero;

            // Log Text (filho do Fundo)
            TextMeshProUGUI logText = UIBuilderHelper.CreateText(logBgPanel, "LogText", "Aguardando...", 20, Color.yellow, TextAlignmentOptions.TopLeft);
            RectTransform logRect = logText.rectTransform;
            logRect.anchorMin = Vector2.zero;
            logRect.anchorMax = Vector2.one;
            logRect.offsetMin = new Vector2(10, 10);
            logRect.offsetMax = new Vector2(-10, -10);

            // Botões de Teste
            Button btnArtifact = UIBuilderHelper.CreateButton(hubPanel, "BtnArtifact", "Gerar Artefato", new Color(0.2f, 0.4f, 0.8f));
            SetButtonPosition(btnArtifact.GetComponent<RectTransform>(), new Vector2(0.05f, 0.2f), new Vector2(0.25f, 0.35f));

            Button btnPet = UIBuilderHelper.CreateButton(hubPanel, "BtnPet", "Gerar Pet", new Color(0.8f, 0.4f, 0.2f));
            SetButtonPosition(btnPet.GetComponent<RectTransform>(), new Vector2(0.28f, 0.2f), new Vector2(0.50f, 0.35f));

            Button btnGacha = UIBuilderHelper.CreateButton(hubPanel, "BtnGacha", "Gacha", new Color(0.8f, 0.2f, 0.8f));
            SetButtonPosition(btnGacha.GetComponent<RectTransform>(), new Vector2(0.53f, 0.2f), new Vector2(0.75f, 0.35f));

            Button btnConfig = UIBuilderHelper.CreateButton(hubPanel, "BtnConfig", "Perfil", new Color(0.4f, 0.4f, 0.4f));
            SetButtonPosition(btnConfig.GetComponent<RectTransform>(), new Vector2(0.78f, 0.2f), new Vector2(0.95f, 0.35f));

            // Ajusta fonte dos botões para caber no mini painel
            btnArtifact.GetComponentInChildren<TextMeshProUGUI>().fontSize = 20;
            btnPet.GetComponentInChildren<TextMeshProUGUI>().fontSize = 20;
            btnGacha.GetComponentInChildren<TextMeshProUGUI>().fontSize = 20;
            btnConfig.GetComponentInChildren<TextMeshProUGUI>().fontSize = 20;

            // Anexa o Manager Runtime
            HubSceneManager manager = hubPanel.gameObject.AddComponent<HubSceneManager>();
            manager.generateArtifactBtn = btnArtifact;
            manager.generatePetBtn = btnPet;
            manager.executeGachaBtn = btnGacha;
            manager.configBtn = btnConfig;
            manager.logText = logText;

            Debug.Log("[HubSceneUIBuilder] Painel flutuante de testes injetado com sucesso!");
        }

        private static void SetButtonPosition(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
