#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Scenes.Unit;
using CelestialCross.Scenes.Inventory;

namespace CelestialCross.EditorArea
{
    public class UIBuilder_SelectionFlow : EditorWindow
    {
        [MenuItem("Celestial Cross/3. UI Builders/1. Screens/Selection Flow (Unit Scene)")]
        public static void GenerateUnitSceneHelper()
        {
            var helper = Object.FindObjectOfType<UnitSelectionFlowHelper>();
            if (helper == null)
            {
                var go = new GameObject("UnitSelectionFlowHelper");
                helper = go.AddComponent<UnitSelectionFlowHelper>();
                Debug.Log("[UIBuilder] UnitSelectionFlowHelper criado na raiz da cena.");
            }
            else
            {
                Debug.Log("[UIBuilder] UnitSelectionFlowHelper já existe na cena.");
            }
            
            EditorUtility.SetDirty(helper);
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        }

        [MenuItem("Celestial Cross/3. UI Builders/1. Screens/Selection Flow (Inventory Scene)")]
        public static void GenerateInventorySceneHelper()
        {
            var inventoryController = Object.FindObjectOfType<InventorySceneController>();
            if (inventoryController == null)
            {
                Debug.LogError("[UIBuilder] InventorySceneController não encontrado na cena! Certifique-se de estar na InventoryScene.");
                return;
            }

            var helper = Object.FindObjectOfType<InventorySelectionFlowHelper>();
            if (helper == null)
            {
                var go = new GameObject("InventorySelectionFlowHelper");
                helper = go.AddComponent<InventorySelectionFlowHelper>();
                Debug.Log("[UIBuilder] InventorySelectionFlowHelper criado.");
            }

            helper.inventoryController = inventoryController;

            // Encontrar abas
            var artifactTab = Object.FindObjectOfType<ArtifactTabPanel>();
            var petTab = Object.FindObjectOfType<PetTabPanel>();

            helper.artifactTab = artifactTab;
            helper.petTab = petTab;

            if (artifactTab != null)
            {
                EnsureSelectionButtons(artifactTab.gameObject, out Button eqBtn, out Button uneqBtn);
                artifactTab.equipButton = eqBtn;
                artifactTab.unequipButton = uneqBtn;
                EditorUtility.SetDirty(artifactTab);
            }

            if (petTab != null)
            {
                EnsureSelectionButtons(petTab.gameObject, out Button eqBtn, out Button uneqBtn);
                petTab.equipButton = eqBtn;
                petTab.unequipButton = uneqBtn;
                EditorUtility.SetDirty(petTab);
            }

            // Tentar achar o botão de voltar principal
            if (helper.returnBackButton == null)
            {
                // Procurar por botões comuns de voltar (ex: ReturnToHub, BackBtn)
                var allButtons = Object.FindObjectsOfType<Button>(true);
                foreach (var btn in allButtons)
                {
                    if (btn.gameObject.name.ToLower().Contains("return") || btn.gameObject.name.ToLower().Contains("back"))
                    {
                        helper.returnBackButton = btn;
                        break;
                    }
                }
            }

            EditorUtility.SetDirty(helper);
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();

            Debug.Log("[UIBuilder] InventorySelectionFlowHelper configurado com sucesso! Verifique se o 'Return Back Button' foi associado corretamente.");
        }

        private static void EnsureSelectionButtons(GameObject tabParent, out Button equipBtn, out Button unequipBtn)
        {
            equipBtn = null;
            unequipBtn = null;

            var selectionContainer = tabParent.transform.Find("SelectionModeButtons");
            if (selectionContainer == null)
            {
                var go = new GameObject("SelectionModeButtons", typeof(RectTransform));
                go.transform.SetParent(tabParent.transform, false);
                selectionContainer = go.transform;
                var rt = (RectTransform)selectionContainer;
                // Posicionar no rodapé, ao lado das ações normais
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(1, 0);
                rt.pivot = new Vector2(0.5f, 0);
                rt.anchoredPosition = new Vector2(0, 20);
                rt.sizeDelta = new Vector2(-40, 60);
            }

            var eqTransform = selectionContainer.Find("EquipButton");
            if (eqTransform == null)
            {
                var go = CreateStyledButton("EquipButton", "Trocar / Equipar", new Color(0.2f, 0.7f, 0.3f));
                go.transform.SetParent(selectionContainer, false);
                eqTransform = go.transform;
                var rt = (RectTransform)eqTransform;
                rt.anchorMin = new Vector2(1, 0.5f);
                rt.anchorMax = new Vector2(1, 0.5f);
                rt.pivot = new Vector2(1, 0.5f);
                rt.anchoredPosition = new Vector2(-20, 0);
                rt.sizeDelta = new Vector2(200, 50);
                go.SetActive(false);
            }
            equipBtn = eqTransform.GetComponent<Button>();

            var uneqTransform = selectionContainer.Find("UnequipButton");
            if (uneqTransform == null)
            {
                var go = CreateStyledButton("UnequipButton", "Desequipar", new Color(0.8f, 0.3f, 0.3f));
                go.transform.SetParent(selectionContainer, false);
                uneqTransform = go.transform;
                var rt = (RectTransform)uneqTransform;
                rt.anchorMin = new Vector2(1, 0.5f);
                rt.anchorMax = new Vector2(1, 0.5f);
                rt.pivot = new Vector2(1, 0.5f);
                rt.anchoredPosition = new Vector2(-240, 0);
                rt.sizeDelta = new Vector2(200, 50);
                go.SetActive(false);
            }
            unequipBtn = uneqTransform.GetComponent<Button>();
        }

        private static GameObject CreateStyledButton(string name, string textLabel, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.GetComponent<Image>().color = color;
            
            var txtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtGo.transform.SetParent(go.transform, false);
            var txtRt = (RectTransform)txtGo.transform;
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = txtRt.offsetMax = Vector2.zero;
            
            var tmp = txtGo.GetComponent<TextMeshProUGUI>();
            tmp.text = textLabel;
            tmp.color = Color.white;
            tmp.fontSize = 24;
            tmp.alignment = TextAlignmentOptions.Center;
            
            return go;
        }
    }
}
#endif
