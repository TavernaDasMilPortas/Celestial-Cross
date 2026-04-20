using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Dialogue.Data;

namespace CelestialCross.Dialogue.Editor
{
    public class HubDiaryUIBuilder
    {
        [MenuItem("Celestial Cross/Dialogue/Update Hub with Diary Catalog")]
        public static void UpdateHub()
        {
            // 1. Achar o HubSceneController
            HubSceneController controller = Object.FindFirstObjectByType<HubSceneController>();
            if (controller == null)
            {
                Debug.LogError("[DiaryBuilder] HubSceneController não encontrado na cena!");
                return;
            }

            // 2. Achar o Canvas
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[DiaryBuilder] Canvas não encontrado!");
                return;
            }

            // 3. Achar o Container de Categorias (onde ficam os botões principais)
            // No HubSceneController temos mainCategoriesContainer
            Transform catContainer = controller.transform; // Fallback
            
            SerializedObject so = new SerializedObject(controller);
            SerializedProperty propMainPanel = so.FindProperty("mainPanel");
            SerializedProperty propDungeonsPanel = so.FindProperty("dungeonsPanel");
            
            GameObject mainPanel = propMainPanel.objectReferenceValue as GameObject;
            if (mainPanel == null)
            {
                Debug.LogError("[DiaryBuilder] mainPanel não encontrado no HubSceneController!");
                return;
            }

            // --- 4. CRIAR O PAINEL DE DIÁRIOS (Replica o de Dungeons) ---
            GameObject dungeonsPanel = propDungeonsPanel.objectReferenceValue as GameObject;
            GameObject diaryPanel = Object.Instantiate(dungeonsPanel, mainPanel.transform.parent);
            diaryPanel.name = "DiaryPanel";
            diaryPanel.SetActive(false);
            Undo.RegisterCreatedObjectUndo(diaryPanel, "Create Diary Panel");

            // Limpar conteúdo do Instantiate (remover botões antigos que vieram no ScrollView)
            Transform diaryContent = diaryPanel.transform.Find("DungeonsScrollView/Viewport/Content");
            if (diaryContent == null) diaryContent = diaryPanel.transform.Find("Viewport/Content"); // Tenta variações de nome se mudarem
            
            // Se não achar pelo path fixo, tenta recursivo
            if (diaryContent == null)
            {
                var scrolls = diaryPanel.GetComponentsInChildren<ScrollRect>();
                if (scrolls.Length > 0) diaryContent = scrolls[0].content;
            }

            if (diaryContent != null)
            {
                diaryContent.gameObject.name = "DiaryContent";
                foreach (Transform child in diaryContent) Object.DestroyImmediate(child.gameObject);
            }

            // Ajustar Título
            TMP_Text diaryTitle = diaryPanel.GetComponentInChildren<TMP_Text>();
            if (diaryTitle != null) {
                diaryTitle.text = "Diários de Aventura";
                diaryTitle.gameObject.name = "Txt_DiaryTitle";
            }

            // Ajustar Botão Voltar do Painel
            Button btnBack = diaryPanel.transform.Find("Btn_Back")?.GetComponent<Button>();
            if (btnBack == null) btnBack = diaryPanel.GetComponentInChildren<Button>(); // Pega o primeiro se não achar pelo nome

            // --- 5. CONFIGURAR O CONTROLLER ---
            so.Update();
            so.FindProperty("diaryPanel").objectReferenceValue = diaryPanel;
            so.FindProperty("diaryContainer").objectReferenceValue = diaryContent;
            so.FindProperty("diaryPanelTitle").objectReferenceValue = diaryTitle;
            so.FindProperty("btnBackFromDiary").objectReferenceValue = btnBack;
            so.ApplyModifiedProperties();

            Debug.Log("[DiaryBuilder] Hub Scene atualizado com sucesso! Agora configure a Categoria de Diário no Inspector ddo HubSceneController.");
        }
    }
}
