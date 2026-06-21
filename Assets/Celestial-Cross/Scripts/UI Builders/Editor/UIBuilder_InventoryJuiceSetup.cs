using UnityEngine;
using UnityEditor;

namespace CelestialCross.Scenes.Inventory.Editor
{
    public static class UIBuilder_InventoryJuiceSetup
    {
        [MenuItem("Celestial Cross/3. UI Builders/Add Inventory Juice (P5 Style)")]
        public static void AddJuicerToScene()
        {
            var controller = Object.FindObjectOfType<InventorySceneController>();
            if (controller == null)
            {
                Debug.LogWarning("[UIBuilder] InventorySceneController não encontrado na cena ativa.");
                return;
            }

            var canvas = controller.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("[UIBuilder] Não foi possível encontrar um Canvas no InventorySceneController.");
                return;
            }

            var juicer = canvas.gameObject.GetComponent<InventoryJuicer>();
            if (juicer == null)
            {
                juicer = canvas.gameObject.AddComponent<InventoryJuicer>();
                UnityEditor.EditorUtility.SetDirty(canvas.gameObject);
                Debug.Log("[UIBuilder] InventoryJuicer adicionado com sucesso ao Canvas principal!");
            }
            else
            {
                Debug.Log("[UIBuilder] InventoryJuicer já está presente no Canvas.");
            }
        }
    }
}
