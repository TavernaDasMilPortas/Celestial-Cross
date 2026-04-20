using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.Services.Core;

namespace CelestialCross.IAP
{
    /// <summary>
    /// Gerencia compras In-App. 
    /// Se Unity IAP não estiver instalado ou configurado, funciona em modo Mock para testes de UI.
    /// </summary>
    public class IAPStoreManager : MonoBehaviour
    {
        public static IAPStoreManager Instance { get; private set; }

        [SerializeField] private IAPStoreConfig storeConfig;
        [SerializeField] private bool useMockMode = true;

        public event Action<string> OnPurchaseComplete;
        public event Action<string, string> OnPurchaseFailed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void InitiatePurchase(string productId)
        {
            Debug.Log($"[IAPStoreManager] Iniciando compra de: {productId}");

            if (useMockMode)
            {
                // Simula um delay de rede
                Invoke(() => CompleteMockPurchase(productId), 1.0f);
            }
            else
            {
                // Aqui entraria a integração real com UnityEngine.Purchasing
                Debug.LogWarning("[IAPStoreManager] Modo real não implementado. Use useMockMode=true para testar.");
            }
        }

        private void CompleteMockPurchase(string productId)
        {
            IAPProductSO product = storeConfig.Products.Find(p => p.ProductID == productId);
            if (product != null)
            {
                ApplyRewards(product);
                OnPurchaseComplete?.Invoke(product.DisplayName);
                Debug.Log($"[IAPStoreManager] Compra Mock concluída: {product.DisplayName}");
            }
            else
            {
                OnPurchaseFailed?.Invoke(productId, "Produto não encontrado na configuração.");
            }
        }

        private void ApplyRewards(IAPProductSO product)
        {
            if (AccountManager.Instance == null || AccountManager.Instance.PlayerAccount == null) return;

            var acc = AccountManager.Instance.PlayerAccount;
            acc.Money += product.MoneyReward;
            acc.StarMaps += product.StarMapsReward;
            acc.Stardust += product.StardustReward;

            AccountManager.Instance.SaveAccount();
        }

        private void Invoke(Action action, float delay)
        {
            StartCoroutine(InvokeRoutine(action, delay));
        }

        private global::System.Collections.IEnumerator InvokeRoutine(Action action, float delay)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }
    }
}
