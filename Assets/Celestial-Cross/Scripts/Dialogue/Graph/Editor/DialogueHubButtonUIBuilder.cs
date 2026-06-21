using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Dialogue.Runtime;

namespace CelestialCross.Dialogue.Editor
{
    public class DialogueHubButtonUIBuilder : EditorWindow
    {
        [MenuItem("Celestial Cross/3. UI Builders/1. Screens/Add Dialogue Hub Button")]
        public static void BuildHubButton()
        {
            // 1. Encontrar o Canvas de Diálogo
            GameObject canvasGO = GameObject.Find("DialogueCanvas");
            if (canvasGO == null)
            {
                // Se não achou com o nome padrão, tenta pegar qualquer Canvas na cena
                Canvas anyCanvas = FindObjectOfType<Canvas>();
                if (anyCanvas != null)
                {
                    canvasGO = anyCanvas.gameObject;
                }
                else
                {
                    Debug.LogError("[DialogueHubButtonUIBuilder] Canvas não encontrado! Por favor, crie um Canvas ou rode o 'Dialogue UI Scene' builder primeiro.");
                    return;
                }
            }

            // Verifica se já existe para evitar duplicatas
            Transform existingBtn = canvasGO.transform.Find("ReturnToHubButton");
            if (existingBtn != null)
            {
                Debug.LogWarning("[DialogueHubButtonUIBuilder] O botão 'ReturnToHubButton' já existe na cena.");
                Selection.activeGameObject = existingBtn.gameObject;
                return;
            }

            // 2. Criar Objeto do Botão
            GameObject btnGO = new GameObject("ReturnToHubButton");
            btnGO.transform.SetParent(canvasGO.transform, false);
            RectTransform btnRT = btnGO.AddComponent<RectTransform>();
            
            // Posicionar no canto inferior direito
            btnRT.anchorMin = new Vector2(1, 0);
            btnRT.anchorMax = new Vector2(1, 0);
            btnRT.pivot = new Vector2(1, 0);
            btnRT.anchoredPosition = new Vector2(-50, 50);
            btnRT.sizeDelta = new Vector2(250, 80);

            // Estilo visual (fundo)
            Image btnImg = btnGO.AddComponent<Image>();
            btnImg.color = new Color(0.15f, 0.5f, 0.85f, 1f); // Azul bonito
            
            Button btn = btnGO.AddComponent<Button>();

            // 3. Criar Texto do Botão
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(btnGO.transform, false);
            TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = "Return to Hub";
            tmp.fontSize = 28;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            
            RectTransform textRT = textGO.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;
            textRT.offsetMin = new Vector2(10, 10);
            textRT.offsetMax = new Vector2(-10, -10);

            // 4. Adicionar o componente que gerencia a lógica de voltar ao hub
            DialogueReturnToHub returnScript = btnGO.AddComponent<DialogueReturnToHub>();
            
            // Linkar a referência do botão no script automaticamente
            SerializedObject so = new SerializedObject(returnScript);
            so.FindProperty("returnButton").objectReferenceValue = btn;
            so.ApplyModifiedProperties();

            // 5. Ocultar o botão por padrão (o script fará isso no Start, mas é bom no Editor também)
            btnGO.SetActive(false);

            Selection.activeGameObject = btnGO;
            Debug.Log("[DialogueHubButtonUIBuilder] Botão 'Return to Hub' criado e configurado com sucesso!");
        }
    }
}
