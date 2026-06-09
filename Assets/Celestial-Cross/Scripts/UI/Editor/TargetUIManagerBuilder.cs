using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Celestial_Cross.Scripts.UI;

namespace Celestial_Cross.Scripts.UI.Editor
{
    public class TargetUIManagerBuilder
    {
        [MenuItem("Celestial Cross/Setup Target Multiplier UI")]
        public static void BuildTargetUI()
        {
            // 1. Procurar ou criar o UIManager
            TargetMultiplierUIManager manager = Object.FindFirstObjectByType<TargetMultiplierUIManager>();
            GameObject managerObj;

            if (manager == null)
            {
                managerObj = new GameObject("TargetMultiplierUIManager");
                manager = managerObj.AddComponent<TargetMultiplierUIManager>();
                Undo.RegisterCreatedObjectUndo(managerObj, "Create Target UIManager");
            }
            else
            {
                managerObj = manager.gameObject;
            }

            // 2. Procurar ou criar um Canvas principal na cena
            Canvas mainCanvas = Object.FindFirstObjectByType<Canvas>();
            if (mainCanvas == null)
            {
                GameObject canvasObj = new GameObject("MainCanvas");
                mainCanvas = canvasObj.AddComponent<Canvas>();
                mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
                Undo.RegisterCreatedObjectUndo(canvasObj, "Create Main Canvas");
            }

            // 3. Criar o texto de "Remaining Targets" no topo da tela (se já não existir no manager)
            TextMeshProUGUI remainingText = null;
            
            // Procurar se já existe um filho com esse nome no canvas para não duplicar
            Transform existingTextObj = mainCanvas.transform.Find("RemainingTargetsText");
            if (existingTextObj != null)
            {
                remainingText = existingTextObj.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                GameObject textObj = new GameObject("RemainingTargetsText");
                textObj.transform.SetParent(mainCanvas.transform, false);
                remainingText = textObj.AddComponent<TextMeshProUGUI>();
                
                remainingText.text = "Alvos restantes: 0";
                remainingText.fontSize = 36;
                remainingText.alignment = TextAlignmentOptions.Center;
                remainingText.color = Color.white;
                
                // Estilizar com outline suave (se der)
                remainingText.fontStyle = FontStyles.Bold;

                // Ancorar no topo (Top Center)
                RectTransform rt = textObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.anchoredPosition = new Vector2(0, -50);
                rt.sizeDelta = new Vector2(400, 100);

                textObj.SetActive(false); // Fica desativado por padrão
                Undo.RegisterCreatedObjectUndo(textObj, "Create Remaining Targets Text");
            }

            // 4. Criar o Prefab do Multiplicador Flutuante
            // Como não podemos salvar o prefab direto na pasta tão fácil de forma invisível, vamos criar um GameObject
            // na cena e convertê-lo em prefab, ou apenas deixar uma referência de GameObject desativada na cena e usar como prefab!
            // Usar um objeto na cena desativado para o manager clonar é uma forma válida de "Prefab in scene"
            
            GameObject multiplierPrefab = null;
            Transform existingPrefabObj = mainCanvas.transform.Find("TargetMultiplierUI_Template");
            if (existingPrefabObj != null)
            {
                multiplierPrefab = existingPrefabObj.gameObject;
            }
            else
            {
                multiplierPrefab = new GameObject("TargetMultiplierUI_Template");
                multiplierPrefab.transform.SetParent(mainCanvas.transform, false);
                
                var textUI = multiplierPrefab.AddComponent<TextMeshProUGUI>();
                textUI.text = "x2";
                textUI.fontSize = 40;
                textUI.color = new Color(1f, 0.8f, 0.2f, 1f); // Amarelo Dourado
                textUI.alignment = TextAlignmentOptions.Center;
                textUI.fontStyle = FontStyles.Bold;

                RectTransform pRT = multiplierPrefab.GetComponent<RectTransform>();
                pRT.sizeDelta = new Vector2(100, 100);
                // Ancoras não importam tanto porque o script vai reposicionar usando WorldToScreenPoint
                pRT.pivot = new Vector2(0.5f, 0f); // Pivot embaixo

                // Adiciona nosso componente UI
                multiplierPrefab.AddComponent<TargetMultiplierUI>();
                
                multiplierPrefab.SetActive(false); // É um template, fica inativo
                Undo.RegisterCreatedObjectUndo(multiplierPrefab, "Create Multiplier Template");
            }

            // 5. Vincular tudo no Manager
            SerializedObject serializedManager = new SerializedObject(manager);
            serializedManager.Update();
            serializedManager.FindProperty("multiplierTextPrefab").objectReferenceValue = multiplierPrefab;
            serializedManager.FindProperty("remainingTargetsText").objectReferenceValue = remainingText;
            serializedManager.FindProperty("mainScreenCanvas").objectReferenceValue = mainCanvas;
            serializedManager.ApplyModifiedProperties();

            // Seleciona o manager para o usuário ver
            Selection.activeGameObject = managerObj;

            Debug.Log("<color=green>Target Multiplier UI configurado com sucesso na cena atual!</color>");
        }
    }
}
