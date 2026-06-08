using UnityEditor;
using UnityEngine;
using TMPro;
using CelestialCross.Giulia_UI;

namespace CelestialCross.UIBuilders.Editor
{
    public class UIBuilder_VictoryRewardUpdater : UnityEditor.Editor
    {
        [MenuItem("Celestial Cross/UI Builders/Update/Victory Modal")]
        public static void UpdateVictoryModal()
        {
            var victoryUI = Object.FindFirstObjectByType<VictoryRewardUI>();
            if (victoryUI == null)
            {
                Debug.LogError("[UI Builder] VictoryRewardUI não encontrado na cena! Para criar do zero, use 'Celestial Cross/3. UI Builders/2. Modals/Victory Modal Complete'.");
                return;
            }

            SerializedObject so = new SerializedObject(victoryUI);
            so.Update();

            // Linkar UnitCatalog caso falte (necessário para mostrar as unidades ganhas)
            var unitCatalogProp = so.FindProperty("unitCatalog");
            if (unitCatalogProp != null && unitCatalogProp.objectReferenceValue == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:UnitCatalog");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    unitCatalogProp.objectReferenceValue = AssetDatabase.LoadAssetAtPath<Object>(path);
                    Debug.Log($"[UI Builder] Vinculado automaticamente: UnitCatalog -> {path}");
                }
            }

            so.ApplyModifiedProperties();

            // Garantir que o ModalTitle possua RichText habilitado (Para suportar a formatação de <size=50%>)
            var titleTxtProp = so.FindProperty("modalTitle");
            if (titleTxtProp != null && titleTxtProp.objectReferenceValue != null)
            {
                var tmp = titleTxtProp.objectReferenceValue as TextMeshProUGUI;
                if (tmp != null && !tmp.richText)
                {
                    tmp.richText = true;
                    EditorUtility.SetDirty(tmp);
                }
            }
            
            // O título principal fica em MainScrollView/Viewport/Content/ModalTitle
            var rootProp = so.FindProperty("rootContainer");
            if (rootProp != null && rootProp.objectReferenceValue != null)
            {
                GameObject root = rootProp.objectReferenceValue as GameObject;
                if (root != null)
                {
                    var mainTitle = root.transform.Find("MainScrollView/Viewport/Content/ModalTitle")?.GetComponent<TextMeshProUGUI>();
                    if (mainTitle != null)
                    {
                        mainTitle.richText = true;
                        mainTitle.enableWordWrapping = true;
                        EditorUtility.SetDirty(mainTitle);
                    }
                }
            }

            EditorUtility.SetDirty(victoryUI);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(victoryUI.gameObject.scene);

            Debug.Log("[UI Builder] Victory Modal atualizado com suporte a Itens Genéricos, Unidades e Formatação de Fase!");
        }
    }
}
