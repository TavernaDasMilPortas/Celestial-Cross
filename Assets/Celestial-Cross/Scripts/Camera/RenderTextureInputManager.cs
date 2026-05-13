using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Converte os cliques na tela (Canvas) para raios da câmera interna usando Render Texture.
/// </summary>
public class RenderTextureInputManager : MonoBehaviour
{
    public static RenderTextureInputManager Instance;

    [Header("Configurações")]
    public RawImage renderTargetUI;
    public Camera gameCamera;
    
    [Header("Auto Configuração")]
    public bool createAndAssignTextureOnStart = true;
    public int textureResolutionStr = 1024; // Resolução quadrada

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (createAndAssignTextureOnStart && renderTargetUI != null && gameCamera != null)
        {
            // Cria um Render Texture quadrado (ex: 1024x1024) que se ajustará no aspect nativo
            RenderTexture rt = new RenderTexture(textureResolutionStr, textureResolutionStr, 24);
            rt.name = "GameRenderTexture";
            
            gameCamera.targetTexture = rt;
            renderTargetUI.texture = rt;
        }
    }

    /// <summary>
    /// Converte um ponto da tela para um Ray que sai da câmera que possui a Render Texture.
    /// </summary>
    public Ray GetRay(Vector2 screenPos)
    {
        if (renderTargetUI == null || gameCamera == null || gameCamera.targetTexture == null)
        {
            // Fallback caso não esteja usando a configuração de Render Texture
            Camera cam = gameCamera != null ? gameCamera : Camera.main;
            return cam.ScreenPointToRay(screenPos);
        }

        RectTransform rt = renderTargetUI.rectTransform;
        
        // Verifica a posição do toque relativa ao Raw Image
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screenPos, null, out Vector2 localPoint))
        {
            // Normaliza o ponto para os limites do tamanho (0 a 1 em cada eixo)
            Vector2 normalizedPoint = new Vector2(
                (localPoint.x + rt.rect.width * rt.pivot.x) / rt.rect.width,
                (localPoint.y + rt.rect.height * rt.pivot.y) / rt.rect.height
            );

            // Traduz a coordenada normalizada para resolução de pixels da câmera
            Vector2 camPixelPos = new Vector2(
                normalizedPoint.x * gameCamera.pixelWidth,
                normalizedPoint.y * gameCamera.pixelHeight
            );

            return gameCamera.ScreenPointToRay(camPixelPos);
        }

        return gameCamera.ScreenPointToRay(screenPos);
    }
}
