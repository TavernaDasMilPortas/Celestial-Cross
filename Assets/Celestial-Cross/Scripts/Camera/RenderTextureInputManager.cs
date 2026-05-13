using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

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
    public bool TryGetRay(Vector2 screenPos, out Ray ray)
    {
        if (renderTargetUI == null || gameCamera == null || gameCamera.targetTexture == null)
        {
            ray = default;
            return false;
        }

        if (!IsScreenPointOverExclusiveRenderTarget(screenPos))
        {
            ray = default;
            return false;
        }

        RectTransform rt = renderTargetUI.rectTransform;
        Canvas canvas = renderTargetUI.canvas;
        Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        
        // Verifica a posição do toque relativa ao Raw Image
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screenPos, uiCamera, out Vector2 localPoint))
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

            ray = gameCamera.ScreenPointToRay(camPixelPos);
            return true;
        }

        ray = default;
        return false;
    }

    public bool IsScreenPointOverRenderTarget(Vector2 screenPos)
    {
        if (renderTargetUI == null)
            return false;

        Canvas canvas = renderTargetUI.canvas;
        Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        return RectTransformUtility.RectangleContainsScreenPoint(renderTargetUI.rectTransform, screenPos, uiCamera);
    }

    public bool IsScreenPointOverExclusiveRenderTarget(Vector2 screenPos)
    {
        if (renderTargetUI == null)
            return false;

        Canvas canvas = renderTargetUI.canvas;
        Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        RectTransform rt = renderTargetUI.rectTransform;

        if (!RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, uiCamera))
            return false;

        if (EventSystem.current == null)
            return true;

        var eventData = new PointerEventData(EventSystem.current)
        {
            position = screenPos
        };

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        if (results.Count == 0)
            return true;

        foreach (var result in results)
        {
            if (result.gameObject == null)
                continue;

            if (result.gameObject == rt.gameObject || result.gameObject.transform.IsChildOf(rt.transform))
                return true;

            // Se o topo for UI decorativa (ex.: moldura, imagem, máscara), não bloqueie o RawImage.
            // Só UI interativa deve impedir o uso da render texture.
            if (!ShouldBlockRenderTargetForObject(result.gameObject))
                continue;

            return false;
        }

        return false;
    }

    bool ShouldBlockRenderTargetForObject(GameObject uiObject)
    {
        if (uiObject == null)
            return false;

        if (uiObject.GetComponent<Selectable>() != null)
            return true;

        // Botões customizados ou controles que não herdam de Selectable podem marcar isso via componente próprio.
        if (uiObject.GetComponent<IPointerClickHandler>() != null)
            return true;

        if (uiObject.GetComponent<IPointerDownHandler>() != null)
            return true;

        if (uiObject.GetComponent<IPointerUpHandler>() != null)
            return true;

        return false;
    }

    public bool IsRaycastTargetReady()
    {
        return renderTargetUI != null && gameCamera != null;
    }
}
