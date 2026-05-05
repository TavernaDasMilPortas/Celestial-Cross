using UnityEditor;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace CelestialCross.Giulia_UI.Editor
{
    public class VictoryUIUtility
    {
        [MenuItem("Celestial Cross/UI Utilities/Auto-Link Victory Modal Components")]
        public static void AutoLinkVictoryComponents()
        {
            var victoryUI = Object.FindFirstObjectByType<VictoryRewardUI>();
            if (victoryUI == null)
            {
                Debug.LogError("[UI Utility] VictoryRewardUI não encontrado na cena.");
                return;
            }

            SerializedObject so = new SerializedObject(victoryUI);
            so.Update();

            var detailsModalProp = so.FindProperty("detailsModal");
            GameObject detailsModal = detailsModalProp.objectReferenceValue as GameObject;

            if (detailsModal == null)
            {
                Debug.LogWarning("[UI Utility] O campo 'Details Modal' está vazio no VictoryRewardUI. Não é possível vincular os sub-componentes.");
            }
            else
            {
                // Título
                LinkComponentIfMissing(so, "modalTitle", detailsModal.transform, "ModalTitle", typeof(TextMeshProUGUI));
                // Descrição
                LinkComponentIfMissing(so, "modalDesc", detailsModal.transform, "ModalDesc", typeof(TextMeshProUGUI));
                // Botão Vender
                LinkComponentIfMissing(so, "modalSellBtn", detailsModal.transform, "Generated_Btn_Sell", typeof(Button), "Btn_Sell");
                // Texto do Botão Vender
                var sellBtnProp = so.FindProperty("modalSellBtn");
                if (sellBtnProp.objectReferenceValue != null)
                {
                    var sellBtnGo = (sellBtnProp.objectReferenceValue as Button).gameObject;
                    LinkComponentIfMissing(so, "modalSellTxt", sellBtnGo.transform, "Text", typeof(TextMeshProUGUI));
                }
                // Botão Fechar
                LinkComponentIfMissing(so, "modalCloseBtn", detailsModal.transform, "Generated_Btn_Close", typeof(Button), "Btn_Close");
            }

            // Linkar Root Container se estiver vazio
            var rootProp = so.FindProperty("rootContainer");
            if (rootProp.objectReferenceValue == null)
            {
                var mainView = victoryUI.transform.Find("MainScrollView");
                if (mainView != null) rootProp.objectReferenceValue = mainView.parent.gameObject;
            }

            so.ApplyModifiedProperties();
            Debug.Log("[UI Utility] Vinculação concluída com sucesso!");
        }

        private static void LinkComponentIfMissing(SerializedObject so, string propertyName, Transform parent, string primaryName, global::System.Type type, string secondaryName = null)
        {
            var prop = so.FindProperty(propertyName);
            if (prop.objectReferenceValue != null) return;

            Transform target = parent.Find(primaryName);
            if (target == null && !string.IsNullOrEmpty(secondaryName))
                target = parent.Find(secondaryName);

            if (target != null)
            {
                prop.objectReferenceValue = target.GetComponent(type);
                Debug.Log($"[UI Utility] Vinculado: {propertyName} -> {target.name}");
            }
            else
            {
                Debug.LogWarning($"[UI Utility] Não foi possível encontrar '{primaryName}' dentro de {parent.name}");
            }
        }
    }
}
