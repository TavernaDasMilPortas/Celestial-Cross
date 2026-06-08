using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Scenes.Unit;

namespace CelestialCross.UIBuilders.Editor
{
    public class UIBuilder_PetPanelUpdater : UnityEditor.Editor
    {
        [MenuItem("Celestial Cross/UI Builders/Update/Pet Panel")]
        public static void UpdatePetPanel()
        {
            var panel = Object.FindObjectOfType<UnitDetailPanel_Pet>(true);
            if (panel == null)
            {
                Debug.LogError("UnitDetailPanel_Pet não encontrado na cena. Abra a UnitScene e tente novamente.");
                return;
            }

            var equipped = panel.petEquippedContainer;
            if (equipped == null)
            {
                Debug.LogError("EquippedContainer não referenciado no UnitDetailPanel_Pet.");
                return;
            }

            // Remove o botão verde de seleção original
            var oldBtn = panel.transform.Find("Btn_SelectPet");
            if (oldBtn != null) DestroyImmediate(oldBtn.gameObject);

            // Reorganizar PetSprite e criar botão invisível
            var spriteRT = panel.petSpriteImage.GetComponent<RectTransform>();
            
            // Botão invisível em cima da sprite
            var invisibleBtnGO = equipped.transform.Find("Btn_InvisibleSelectPet")?.gameObject;
            if (invisibleBtnGO == null)
            {
                invisibleBtnGO = new GameObject("Btn_InvisibleSelectPet", typeof(RectTransform), typeof(Image), typeof(Button));
                invisibleBtnGO.transform.SetParent(equipped.transform, false);
            }
            var ibRT = invisibleBtnGO.GetComponent<RectTransform>();
            ibRT.anchorMin = spriteRT.anchorMin;
            ibRT.anchorMax = spriteRT.anchorMax;
            ibRT.offsetMin = spriteRT.offsetMin;
            ibRT.offsetMax = spriteRT.offsetMax;
            
            var ibImage = invisibleBtnGO.GetComponent<Image>();
            ibImage.color = new Color(0, 0, 0, 0); // Invisível

            panel.petImageButton = invisibleBtnGO.GetComponent<Button>();

            // Adicionar sinal de + translúcido no meio do botão
            var plusText = invisibleBtnGO.transform.Find("PlusText")?.gameObject;
            if (plusText == null)
            {
                plusText = new GameObject("PlusText", typeof(RectTransform), typeof(TextMeshProUGUI));
                plusText.transform.SetParent(invisibleBtnGO.transform, false);
            }
            var plusRT = plusText.GetComponent<RectTransform>();
            plusRT.anchorMin = Vector2.zero; plusRT.anchorMax = Vector2.one;
            plusRT.offsetMin = plusRT.offsetMax = Vector2.zero;
            var pTmp = plusText.GetComponent<TextMeshProUGUI>();
            pTmp.text = "+";
            pTmp.fontSize = 60;
            pTmp.alignment = TextAlignmentOptions.Center;
            pTmp.color = new Color(1, 1, 1, 0.3f);

            // Adicionar botão no empty container
            var emptyContainer = panel.petEmptyContainer;
            if (emptyContainer != null)
            {
                var emptyBtn = emptyContainer.GetComponent<Button>();
                if (emptyBtn == null) emptyBtn = emptyContainer.AddComponent<Button>();
                panel.emptySlotButton = emptyBtn;
            }

            // Remover o texto de status antigo que era tudo numa string só
            var oldStats = equipped.transform.Find("PetStats");
            if (oldStats != null) DestroyImmediate(oldStats.gameObject);

            // Criar uma grade para os atributos
            var statsGridGO = equipped.transform.Find("StatsGrid")?.gameObject;
            if (statsGridGO == null)
            {
                statsGridGO = new GameObject("StatsGrid", typeof(RectTransform), typeof(GridLayoutGroup));
                statsGridGO.transform.SetParent(equipped.transform, false);
            }
            var gridRT = statsGridGO.GetComponent<RectTransform>();
            gridRT.anchorMin = new Vector2(0.45f, 0.45f);
            gridRT.anchorMax = new Vector2(0.95f, 0.75f);
            gridRT.offsetMin = gridRT.offsetMax = Vector2.zero;

            var glg = statsGridGO.GetComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(100, 30);
            glg.spacing = new Vector2(10, 5);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 2; // 2 colunas

            // Criar ou atualizar as referências dos textos separados
            panel.hpText = CreateOrUpdateText(statsGridGO.transform, "HPText", "HP: 0");
            panel.atkText = CreateOrUpdateText(statsGridGO.transform, "ATKText", "ATK: 0");
            panel.defText = CreateOrUpdateText(statsGridGO.transform, "DEFText", "DEF: 0");
            panel.spdText = CreateOrUpdateText(statsGridGO.transform, "SPDText", "SPD: 0");
            panel.critChanceText = CreateOrUpdateText(statsGridGO.transform, "CRateText", "CRIT: 0%");
            panel.critDmgText = CreateOrUpdateText(statsGridGO.transform, "CDmgText", "C.DMG: 0%");
            panel.accText = CreateOrUpdateText(statsGridGO.transform, "ACCText", "ACC: 0%");
            panel.resText = CreateOrUpdateText(statsGridGO.transform, "RESText", "RES: 0%");

            // Criar container de estrelas
            var starsContGO = equipped.transform.Find("StarsContainer")?.gameObject;
            if (starsContGO == null)
            {
                starsContGO = new GameObject("StarsContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
                starsContGO.transform.SetParent(equipped.transform, false);
            }
            var stRT = starsContGO.GetComponent<RectTransform>();
            stRT.anchorMin = new Vector2(0.5f, 0.92f); // no topo
            stRT.anchorMax = new Vector2(0.5f, 0.92f);
            stRT.sizeDelta = new Vector2(200, 30);
            stRT.anchoredPosition = new Vector2(0, 0);

            var stHLG = starsContGO.GetComponent<HorizontalLayoutGroup>();
            stHLG.childAlignment = TextAnchor.MiddleCenter;
            stHLG.childControlWidth = false;
            stHLG.childControlHeight = false;
            stHLG.spacing = 5;

            panel.starsContainer = starsContGO.transform;

            // Criar Prefab de estrela padrão
            if (panel.starPrefab == null)
            {
                var starPrefabGO = panel.transform.Find("DefaultStarPrefab")?.gameObject;
                if (starPrefabGO == null)
                {
                    starPrefabGO = new GameObject("DefaultStarPrefab", typeof(RectTransform), typeof(Image));
                    starPrefabGO.transform.SetParent(panel.transform, false);
                    starPrefabGO.SetActive(false);
                    var sRT = starPrefabGO.GetComponent<RectTransform>();
                    sRT.sizeDelta = new Vector2(25, 25);
                    starPrefabGO.GetComponent<Image>().color = Color.yellow;
                }
                panel.starPrefab = starPrefabGO;
            }

            // Garantir que a skillIconImage tenha um Button
            if (panel.skillIconImage != null)
            {
                var skillBtn = panel.skillIconImage.GetComponent<Button>();
                if (skillBtn == null) skillBtn = panel.skillIconImage.gameObject.AddComponent<Button>();
                panel.skillIconButton = skillBtn;
            }

            // Ligar o PetSkillModal automaticamente se existir na cena, senão cria um básico
            if (panel.petSkillModal == null)
            {
                var foundModal = Object.FindObjectOfType<CelestialCross.Scenes.Inventory.PetSkillModal>(true);
                if (foundModal != null)
                {
                    panel.petSkillModal = foundModal;
                }
                else
                {
                    // Criar um PetSkillModal do zero
                    var modalGO = new GameObject("Panel_PetSkillModal", typeof(RectTransform), typeof(Image), typeof(CelestialCross.Scenes.Inventory.PetSkillModal));
                    
                    // Pega o root do Canvas principal
                    var canvas = Object.FindObjectOfType<Canvas>();
                    if (canvas != null) modalGO.transform.SetParent(canvas.transform, false);
                    else modalGO.transform.SetParent(panel.transform.root, false);
                    
                    var mRT = modalGO.GetComponent<RectTransform>();
                    mRT.anchorMin = Vector2.zero; mRT.anchorMax = Vector2.one;
                    mRT.offsetMin = mRT.offsetMax = Vector2.zero;
                    modalGO.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f); // Fundo escuro
                    
                    var pModal = modalGO.GetComponent<CelestialCross.Scenes.Inventory.PetSkillModal>();
                    
                    // Box principal
                    var box = new GameObject("Box", typeof(RectTransform), typeof(Image));
                    box.transform.SetParent(modalGO.transform, false);
                    var bRT = box.GetComponent<RectTransform>();
                    bRT.anchorMin = new Vector2(0.2f, 0.2f); bRT.anchorMax = new Vector2(0.8f, 0.8f);
                    bRT.offsetMin = bRT.offsetMax = Vector2.zero;
                    box.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 1f);
                    
                    // Ícone
                    var iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                    iconGO.transform.SetParent(box.transform, false);
                    var iRT = iconGO.GetComponent<RectTransform>();
                    iRT.anchorMin = new Vector2(0.5f, 0.8f); iRT.anchorMax = new Vector2(0.5f, 0.8f);
                    iRT.sizeDelta = new Vector2(100, 100);
                    iRT.anchoredPosition = new Vector2(0, -60);
                    pModal.skillIconImage = iconGO.GetComponent<Image>();
                    
                    // Nome
                    pModal.skillNameText = CreateOrUpdateText(box.transform, "NameText", "Nome da Habilidade");
                    var nRT = pModal.skillNameText.GetComponent<RectTransform>();
                    nRT.anchorMin = new Vector2(0.1f, 0.6f); nRT.anchorMax = new Vector2(0.9f, 0.7f);
                    nRT.offsetMin = nRT.offsetMax = Vector2.zero;
                    pModal.skillNameText.alignment = TextAlignmentOptions.Center;
                    pModal.skillNameText.fontSize = 24;
                    
                    // Descrição
                    pModal.skillDescriptionText = CreateOrUpdateText(box.transform, "DescText", "Descrição longa da habilidade...");
                    var dRT = pModal.skillDescriptionText.GetComponent<RectTransform>();
                    dRT.anchorMin = new Vector2(0.1f, 0.2f); dRT.anchorMax = new Vector2(0.9f, 0.55f);
                    dRT.offsetMin = dRT.offsetMax = Vector2.zero;
                    pModal.skillDescriptionText.alignment = TextAlignmentOptions.TopLeft;
                    pModal.skillDescriptionText.enableWordWrapping = true;
                    pModal.skillDescriptionText.fontSize = 18;
                    
                    // Botão Fechar
                    var btnGO = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
                    btnGO.transform.SetParent(box.transform, false);
                    var cRT = btnGO.GetComponent<RectTransform>();
                    cRT.anchorMin = new Vector2(0.4f, 0.05f); cRT.anchorMax = new Vector2(0.6f, 0.15f);
                    cRT.offsetMin = cRT.offsetMax = Vector2.zero;
                    btnGO.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f, 1f);
                    var btnTxt = CreateOrUpdateText(btnGO.transform, "Text", "Fechar");
                    var btTxtRT = btnTxt.GetComponent<RectTransform>();
                    btTxtRT.anchorMin = Vector2.zero; btTxtRT.anchorMax = Vector2.one;
                    btTxtRT.offsetMin = btTxtRT.offsetMax = Vector2.zero;
                    btnTxt.alignment = TextAlignmentOptions.Center;
                    pModal.closeButton = btnGO.GetComponent<Button>();
                    
                    modalGO.SetActive(false); // Fica escondido até chamarem o Show()
                    panel.petSkillModal = pModal;
                }
            }

            // Garantir que a cena seja salva
            EditorUtility.SetDirty(panel);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(panel.gameObject.scene);
            
            Debug.Log("UIBuilder_PetPanelUpdater: Painel do Pet atualizado na cena com sucesso!");
        }

        private static TextMeshProUGUI CreateOrUpdateText(Transform parent, string name, string defaultText)
        {
            var go = parent.Find(name)?.gameObject;
            if (go == null)
            {
                go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
                go.transform.SetParent(parent, false);
            }
            var txt = go.GetComponent<TextMeshProUGUI>();
            txt.text = defaultText;
            txt.fontSize = 16;
            txt.color = Color.white;
            txt.alignment = TextAlignmentOptions.Left;
            return txt;
        }
    }
}
