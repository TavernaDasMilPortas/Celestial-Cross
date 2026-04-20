using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CelestialCross.Dialogue.Editor
{
    public class DialogueUIBuilder : EditorWindow
    {
        [MenuItem("Celestial Cross/UI/Build Dialogue UI Scene")]
        public static void BuildUI()
        {
            // 1. Criar Canvas se não existir
            GameObject canvasGO = GameObject.Find("DialogueCanvas");
            if (canvasGO == null)
            {
                canvasGO = new GameObject("DialogueCanvas");
                Canvas canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<CanvasScaler>();
                canvasGO.AddComponent<GraphicRaycaster>();
            }

            // 2. Criar Painel Principal
            GameObject mainPanel = new GameObject("MainDialoguePanel");
            mainPanel.transform.SetParent(canvasGO.transform, false);
            RectTransform mainRT = mainPanel.AddComponent<RectTransform>();
            mainRT.anchorMin = new Vector2(0.5f, 0);
            mainRT.anchorMax = new Vector2(0.5f, 0);
            mainRT.pivot = new Vector2(0.5f, 0);
            mainRT.sizeDelta = new Vector2(800, 250);
            mainRT.anchoredPosition = new Vector2(0, 50);
            mainPanel.AddComponent<Image>().color = new Color(0, 0, 0, 0.8f);

            // 3. Criar Nome do Personagem
            GameObject nameGO = new GameObject("SpeakerName");
            nameGO.transform.SetParent(mainPanel.transform, false);
            TMP_Text nameText = nameGO.AddComponent<TextMeshProUGUI>();
            nameText.fontSize = 24;
            nameText.text = "Character Name";
            RectTransform nameRT = nameGO.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 1);
            nameRT.anchorMax = new Vector2(0, 1);
            nameRT.pivot = new Vector2(0, 0);
            nameRT.anchoredPosition = new Vector2(20, 5);
            nameRT.sizeDelta = new Vector2(200, 50);

            // 4. Criar Texto do Diálogo
            GameObject textGO = new GameObject("DialogueText");
            textGO.transform.SetParent(mainPanel.transform, false);
            TMP_Text diagText = textGO.AddComponent<TextMeshProUGUI>();
            diagText.fontSize = 32;
            diagText.text = "Sample dialogue text goes here...";
            RectTransform textRT = textGO.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = new Vector2(-40, -40);

            // 5. Criar Retrato do Personagem (Portrait)
            GameObject portraitGO = new GameObject("CharacterPortrait");
            portraitGO.transform.SetParent(mainPanel.transform, false);
            Image portraitImg = portraitGO.AddComponent<Image>();
            RectTransform portraitRT = portraitGO.GetComponent<RectTransform>();
            portraitRT.anchorMin = new Vector2(0, 0.5f);
            portraitRT.anchorMax = new Vector2(0, 0.5f);
            portraitRT.pivot = new Vector2(1, 0.5f);
            portraitRT.anchoredPosition = new Vector2(-20, 0);
            portraitRT.sizeDelta = new Vector2(200, 200);
            portraitGO.SetActive(false);

            // 6. Criar Indicador de Continuação
            GameObject continueGO = new GameObject("ContinueIndicator");
            continueGO.transform.SetParent(mainPanel.transform, false);
            Image continueImg = continueGO.AddComponent<Image>();
            continueImg.color = Color.yellow;
            RectTransform continueRT = continueGO.GetComponent<RectTransform>();
            continueRT.anchorMin = new Vector2(1, 0);
            continueRT.anchorMax = new Vector2(1, 0);
            continueRT.anchoredPosition = new Vector2(-30, 30);
            continueRT.sizeDelta = new Vector2(20, 20);
            continueGO.SetActive(false);

            // 7. Criar Container de Escolhas
            GameObject choicesPanel = new GameObject("ChoicesPanel");
            choicesPanel.transform.SetParent(canvasGO.transform, false);
            RectTransform choicesRT = choicesPanel.AddComponent<RectTransform>();
            choicesRT.anchorMin = new Vector2(0.5f, 0.5f);
            choicesRT.anchorMax = new Vector2(0.5f, 0.5f);
            choicesRT.sizeDelta = new Vector2(400, 300);
            choicesPanel.AddComponent<VerticalLayoutGroup>().childControlHeight = true;
            choicesPanel.SetActive(false);

            // 8. Tentar carregar Prefab de Botão (Opcional)
            Button choiceBtnPrefab = AssetDatabase.LoadAssetAtPath<Button>("Assets/Celestial-Cross/Prefabs/UI/DialogueChoiceButton.prefab");

            // 9. Criar Gerenciador Global
            GameObject managerGO = new GameObject("DialogueManager (System)");
            var manager = managerGO.AddComponent<CelestialCross.Dialogue.Manager.DialogueManager>();
            var ui = managerGO.AddComponent<CelestialCross.Dialogue.Manager.DialogueUI>();

            // Linkar Referências automaticamente
            SerializedObject soUI = new SerializedObject(ui);
            soUI.FindProperty("mainDialoguePanel").objectReferenceValue = mainPanel;
            soUI.FindProperty("choicesPanel").objectReferenceValue = choicesPanel;
            soUI.FindProperty("speakerNameText").objectReferenceValue = nameText;
            soUI.FindProperty("dialogueText").objectReferenceValue = diagText;
            soUI.FindProperty("characterPortrait").objectReferenceValue = portraitImg;
            soUI.FindProperty("continueIndicator").objectReferenceValue = continueGO;
            soUI.FindProperty("choicesContainer").objectReferenceValue = choicesPanel.transform;
            if (choiceBtnPrefab != null) soUI.FindProperty("choiceButtonPrefab").objectReferenceValue = choiceBtnPrefab;
            soUI.ApplyModifiedProperties();

            SerializedObject soManager = new SerializedObject(manager);
            soManager.FindProperty("dialogueUI").objectReferenceValue = ui;
            soManager.ApplyModifiedProperties();

            Selection.activeGameObject = managerGO;
            Debug.Log("Dialogue UI Scene built successfully!");
        }
    }
}
