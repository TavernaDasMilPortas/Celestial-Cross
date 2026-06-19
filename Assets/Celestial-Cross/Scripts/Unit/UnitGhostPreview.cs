using UnityEngine;

public class UnitGhostPreview : MonoBehaviour
{
    private SpriteRenderer ghostRenderer;
    private SpriteRenderer originalRenderer;
    private bool isInitialized = false;

    public void Initialize(Unit unit)
    {
        if (isInitialized) return;

        var visualCtrl = unit.GetComponentInChildren<UnitVisualController>();
        if (visualCtrl != null)
        {
            originalRenderer = visualCtrl.GetComponent<SpriteRenderer>();
        }
        else
        {
            originalRenderer = unit.GetComponentInChildren<SpriteRenderer>();
        }
        
        if (originalRenderer == null)
        {
            Debug.LogWarning($"[UnitGhostPreview] Não foi possível encontrar SpriteRenderer na unidade {unit.name}");
            return;
        }

        GameObject ghostObj = new GameObject($"{unit.name}_GhostPreview");
        ghostObj.transform.SetParent(unit.transform);
        ghostObj.transform.localPosition = Vector3.zero;

        ghostRenderer = ghostObj.AddComponent<SpriteRenderer>();
        ghostRenderer.sprite = originalRenderer.sprite;
        ghostRenderer.sortingLayerID = originalRenderer.sortingLayerID;
        ghostRenderer.sortingOrder = 20; // Garantir que fica acima das setas (que usam 10)
        
        // O fantasma que vai pro destino será o "bonitão" (opacidade total)
        Color color = originalRenderer.color;
        color.a = 1.0f;
        ghostRenderer.color = color;
        
        ghostRenderer.flipX = originalRenderer.flipX;
        ghostRenderer.flipY = originalRenderer.flipY;
        ghostRenderer.transform.rotation = UnityEngine.Quaternion.Euler(90f, 0f, 0f); // Rotação 90 graus no X pedida
        
        ghostObj.SetActive(false);
        isInitialized = true;
    }

    public void ShowAt(Vector3 worldPosition, bool flipX = false)
    {
        if (!isInitialized || ghostRenderer == null) return;
        
        if (originalRenderer != null)
        {
            ghostRenderer.sprite = originalRenderer.sprite;
            
            // Define a opacidade do ghost pela configuração do PathVisualizer
            Color ghostColor = originalRenderer.color;
            ghostColor.a = PathVisualizer.Instance != null ? PathVisualizer.Instance.ghostOpacity : 0.5f;
            ghostRenderer.color = ghostColor;
        }
        
        ghostRenderer.transform.position = worldPosition;
        ghostRenderer.flipX = flipX;
        ghostRenderer.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (!isInitialized || ghostRenderer == null) return;
        ghostRenderer.gameObject.SetActive(false);
    }
    
    private void OnDestroy()
    {
        if (ghostRenderer != null)
        {
            Destroy(ghostRenderer.gameObject);
        }
    }
}
