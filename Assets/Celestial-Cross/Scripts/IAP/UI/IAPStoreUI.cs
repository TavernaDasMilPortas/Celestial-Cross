using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CelestialCross.IAP.UI
{
    public class IAPStoreUI : MonoBehaviour
    {
        [SerializeField] private IAPStoreConfig config;
        [SerializeField] private Transform container;
        [SerializeField] private Button productButtonPrefab;

        private void Start()
        {
            if (config == null) return;
            BuildUI();
        }

        public void BuildUI()
        {
            foreach (Transform child in container) Destroy(child.gameObject);

            foreach (var product in config.Products)
            {
                Button btn = Instantiate(productButtonPrefab, container);
                
                // Configura textos (assumindo que o prefab tenha TMP_Text)
                var texts = btn.GetComponentsInChildren<TMP_Text>();
                foreach (var t in texts)
                {
                    if (t.name.Contains("Name")) t.text = product.DisplayName;
                    if (t.name.Contains("Price")) t.text = $"$ {product.Price:F2}";
                }

                btn.onClick.AddListener(() => {
                    IAPStoreManager.Instance.InitiatePurchase(product.ProductID);
                });
            }
        }

        private void OnEnable()
        {
            if (IAPStoreManager.Instance != null)
                IAPStoreManager.Instance.OnPurchaseComplete += HandlePurchaseComplete;
        }

        private void OnDisable()
        {
            if (IAPStoreManager.Instance != null)
                IAPStoreManager.Instance.OnPurchaseComplete -= HandlePurchaseComplete;
        }

        private void HandlePurchaseComplete(string name)
        {
            // Atualiza a UI do Shopping se ela estiver visível
            var shopUI = FindObjectOfType<CelestialCross.Gacha.UI.ShopSceneUI>();
            if (shopUI != null) shopUI.RefreshUI();
            
            Debug.Log($"UI: Compra de {name} bem-sucedida!");
        }
    }
}
