using UnityEngine;

namespace CelestialCross.Cloud
{
    [CreateAssetMenu(fileName = "AzureConfig", menuName = "Celestial Cross/Config/Azure Config")]
    public class AzureConfig : ScriptableObject
    {
        [Header("Configurações do Azure Functions")]
        [Tooltip("URL base do Azure Functions (ex: https://meu-app.azurewebsites.net/api)")]
        public string BaseUrl;

        [Tooltip("Chave de autenticação da function (se necessário)")]
        public string FunctionKey;

        [Header("Opções de Rede")]
        public int TimeoutSeconds = 15;
    }
}
