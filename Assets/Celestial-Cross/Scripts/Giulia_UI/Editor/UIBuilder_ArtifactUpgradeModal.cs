using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CelestialCross.Giulia_UI.Editor
{
    public class UIBuilder_ArtifactUpgradeModal
    {
        [MenuItem("Celestial Cross/UI Builders/Generate Artifact Upgrade Modal")]
        public static void BuildModal()
        {
            // Tenta encontrar um Canvas na cena atual
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Nenhum Canvas encontrado na cena. Por favor, abra uma cena com Canvas ou crie um.");
                return;
            }

            // 1. Root Modal (Fundo Escuro)
            GameObject modalGO = new GameObject("ArtifactUpgradeModal", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            modalGO.transform.SetParent(canvas.transform, false);
            RectTransform modalRT = modalGO.GetComponent<RectTransform>();
            modalRT.anchorMin = Vector2.zero;
            modalRT.anchorMax = Vector2.one;
            modalRT.offsetMin = Vector2.zero;
            modalRT.offsetMax = Vector2.zero;
            modalGO.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);

            // Adiciona o Componente que você quer referenciar
            ArtifactUpgradeModal modalComp = modalGO.AddComponent<ArtifactUpgradeModal>();

            // 2. Painel Central (Fundo Cinza)
            GameObject panelGO = new GameObject("CenterPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panelGO.transform.SetParent(modalRT, false);
            RectTransform panelRT = panelGO.GetComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.1f, 0.1f);
            panelRT.anchorMax = new Vector2(0.9f, 0.9f);
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;
            panelGO.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 1f);

            // 3. Textos e Botões
            TextMeshProUGUI titleTMP = CreateText(panelRT, "TitleText", new Vector2(0, 0.85f), new Vector2(1, 1), 36, "Aprimorar Artefato", Color.yellow);
            
            TextMeshProUGUI detailsTMP = CreateText(panelRT, "DetailsText", new Vector2(0.05f, 0.3f), new Vector2(0.95f, 0.8f), 28, "Detalhes...", Color.white);
            detailsTMP.alignment = TextAlignmentOptions.TopLeft;

            TextMeshProUGUI upgradeCostTMP;
            Button upgradeBtn = CreateButton(panelRT, "UpgradeButton", new Vector2(0.1f, 0.15f), new Vector2(0.9f, 0.25f), new Color(0.2f, 0.8f, 0.2f), out upgradeCostTMP);
            
            TextMeshProUGUI sellPriceTMP;
            Button sellBtn = CreateButton(panelRT, "SellButton", new Vector2(0.1f, 0.05f), new Vector2(0.45f, 0.12f), new Color(0.8f, 0.2f, 0.2f), out sellPriceTMP);
            sellPriceTMP.fontSize = 20;

            TextMeshProUGUI closeTMP;
            Button closeBtn = CreateButton(panelRT, "CloseButton", new Vector2(0.55f, 0.05f), new Vector2(0.9f, 0.12f), new Color(0.5f, 0.5f, 0.5f), out closeTMP);
            closeTMP.fontSize = 20;
            closeTMP.text = "Fechar";

            // 4. Mágica do SerializedObject: Conecta as variáveis privadas [SerializeField] automaticamente!
            SerializedObject so = new SerializedObject(modalComp);
            so.Update();
            so.FindProperty("titleText").objectReferenceValue = titleTMP;
            so.FindProperty("detailsText").objectReferenceValue = detailsTMP;
            so.FindProperty("upgradeButton").objectReferenceValue = upgradeBtn;
            so.FindProperty("upgradeCostText").objectReferenceValue = upgradeCostTMP;
            so.FindProperty("sellButton").objectReferenceValue = sellBtn;
            so.FindProperty("sellPriceText").objectReferenceValue = sellPriceTMP;
            so.FindProperty("closeButton").objectReferenceValue = closeBtn;
            so.ApplyModifiedProperties();

            // Oculta inicial
            modalGO.SetActive(false);

            // Seleciona o objeto recém-criado
            Selection.activeGameObject = modalGO;
            Debug.Log("ArtifactUpgradeModal gerado com sucesso! Edite-o visualmente e arraste para transformar em um Prefab.");
        }

        // --- Funções Auxiliares de Andaime ---
        private static TextMeshProUGUI CreateText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, int fontSize, string text, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false; // Prevê colisões indesejadas
            return tmp;
        }

        private static Button CreateButton(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color, out TextMeshProUGUI textComp)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image img = go.GetComponent<Image>();
            img.color = color;

            Button btn = go.GetComponent<Button>();
            btn.targetGraphic = img; // Previne falta de feedback

            textComp = CreateText(go.transform, "Text", Vector2.zero, Vector2.one, 24, "Botão", Color.white);
            
            return btn;
        }
    }
}