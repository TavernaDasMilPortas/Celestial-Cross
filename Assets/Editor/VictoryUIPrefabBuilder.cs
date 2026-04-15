using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace CelestialCross.EditorScripts
{
    public class VictoryUIPrefabBuilder : EditorWindow
    {
        [MenuItem("Celestial Cross/UI Builders/Generate Victory UI Templates")]
        public static void GenerateTemplates()
        {
            // Busca um Canvas na cena, ou cria um se não existir
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasGo = new GameObject("Canvas");
                canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGo.AddComponent<CanvasScaler>();
                canvasGo.AddComponent<GraphicRaycaster>();
                Undo.RegisterCreatedObjectUndo(canvasGo, "Create Canvas");
            }

            // ==========================================
            // 1. Template do Botão de Recompensa (Quadrado)
            // ==========================================
            GameObject btnRoot = new GameObject("ArtifactPet_Button_Template", typeof(RectTransform), typeof(Image), typeof(Button));
            btnRoot.transform.SetParent(canvas.transform, false);
            RectTransform btnRt = btnRoot.GetComponent<RectTransform>();
            btnRt.sizeDelta = new Vector2(150, 150); // Formato quadrado
            btnRoot.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f); // Fundo escuro

            // Ícone da Recompensa
            GameObject iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(btnRoot.transform, false);
            RectTransform iconRt = iconGo.GetComponent<RectTransform>();
            // Espaço maior no topo
            iconRt.anchorMin = new Vector2(0.1f, 0.25f); 
            iconRt.anchorMax = new Vector2(0.9f, 0.95f);
            iconRt.offsetMin = iconRt.offsetMax = Vector2.zero;
            iconGo.GetComponent<Image>().preserveAspect = true;

            // Texto do Nome do Item embaixo
            GameObject txtGo = new GameObject("Text (TMP)", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtGo.transform.SetParent(btnRoot.transform, false);
            RectTransform txtRt = txtGo.GetComponent<RectTransform>();
            // Espaço abaixo do ícone
            txtRt.anchorMin = new Vector2(0f, 0f);
            txtRt.anchorMax = new Vector2(1f, 0.25f);
            txtRt.offsetMin = txtRt.offsetMax = Vector2.zero;
            TextMeshProUGUI tmp = txtGo.GetComponent<TextMeshProUGUI>();
            tmp.text = "Nome do Item";
            tmp.alignment = TextAlignmentOptions.Bottom;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 10;
            tmp.fontSizeMax = 24;

            // ==========================================
            // 2. Template do Painel de Especificações Modal
            // ==========================================
            GameObject modalRoot = new GameObject("VictoryModal_Template", typeof(RectTransform), typeof(Image));
            modalRoot.transform.SetParent(canvas.transform, false);
            RectTransform modalRt = modalRoot.GetComponent<RectTransform>();
            modalRt.sizeDelta = new Vector2(400, 600);
            modalRoot.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.05f, 0.95f); // Fundo Preto Transparente

            // Ícone centralizado no topo do detalhe
            GameObject mIconGo = new GameObject("ModalIcon", typeof(RectTransform), typeof(Image));
            mIconGo.transform.SetParent(modalRoot.transform, false);
            RectTransform mIconRt = mIconGo.GetComponent<RectTransform>();
            mIconRt.anchorMin = new Vector2(0.25f, 0.5f);
            mIconRt.anchorMax = new Vector2(0.75f, 0.95f);
            mIconRt.offsetMin = mIconRt.offsetMax = Vector2.zero;
            mIconGo.GetComponent<Image>().preserveAspect = true;

            // Título do modal logo abaixo da imagem
            GameObject mTitleGo = new GameObject("ModalTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            mTitleGo.transform.SetParent(modalRoot.transform, false);
            RectTransform mTitleRt = mTitleGo.GetComponent<RectTransform>();
            mTitleRt.anchorMin = new Vector2(0.05f, 0.4f);
            mTitleRt.anchorMax = new Vector2(0.95f, 0.48f);
            mTitleRt.offsetMin = mTitleRt.offsetMax = Vector2.zero;
            TextMeshProUGUI mTitle = mTitleGo.GetComponent<TextMeshProUGUI>();
            mTitle.text = "Nome do Item Grande";
            mTitle.alignment = TextAlignmentOptions.Center;
            mTitle.fontSize = 28;
            mTitle.fontStyle = FontStyles.Bold;

            // Descrição (infos e status)
            GameObject mDescGo = new GameObject("ModalDesc", typeof(RectTransform), typeof(TextMeshProUGUI));
            mDescGo.transform.SetParent(modalRoot.transform, false);
            RectTransform mDescRt = mDescGo.GetComponent<RectTransform>();
            mDescRt.anchorMin = new Vector2(0.05f, 0.05f);
            mDescRt.anchorMax = new Vector2(0.95f, 0.38f);
            mDescRt.offsetMin = mDescRt.offsetMax = Vector2.zero;
            TextMeshProUGUI mDesc = mDescGo.GetComponent<TextMeshProUGUI>();
            mDesc.text = "Especificações detalhadas do item.\nStatus 1...\nStatus 2...";
            mDesc.alignment = TextAlignmentOptions.Top;
            mDesc.fontSize = 20;

            // Registra para permitir "Ctrl+Z" (Desfazer)
            Undo.RegisterCreatedObjectUndo(btnRoot, "Create Button Template");
            Undo.RegisterCreatedObjectUndo(modalRoot, "Create Modal Template");

            Selection.activeGameObject = modalRoot;
            Debug.Log("[Victory UI Builder] Templates UI gerados com sucesso na cena! Arraste-os para uma pasta para torná-los Prefabs.");
        }
    }
}