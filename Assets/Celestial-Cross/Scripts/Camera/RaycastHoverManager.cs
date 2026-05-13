using UnityEngine;

/// <summary>
/// Substitui o OnMouseEnter/OnMouseExit nativo da Unity que quebra com Render Textures.
/// Baseia-se no RenderTextureInputManager para detectar hovers.
/// </summary>
public class RaycastHoverManager : MonoBehaviour
{
    public static RaycastHoverManager Instance;

    UnitHoverDetector currentHover;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (RenderTextureInputManager.Instance == null)
            return;

        if (!RenderTextureInputManager.Instance.TryGetRay(Input.mousePosition, out Ray ray))
        {
            if (currentHover != null)
            {
                currentHover.ManualMouseExit();
                currentHover = null;
            }
            return;
        }
        
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            UnitHoverDetector detector = hit.collider.GetComponent<UnitHoverDetector>();
            
            // Se trocamos de alvo apontado (saiu de um, entrou no outro ou num vazio)
            if (detector != currentHover)
            {
                if (currentHover != null)
                {
                    currentHover.ManualMouseExit();
                }

                currentHover = detector;
                
                if (currentHover != null)
                {
                    currentHover.ManualMouseEnter();
                }
            }
        }
        else
        {
            // Apontando pro nada
            if (currentHover != null)
            {
                currentHover.ManualMouseExit();
                currentHover = null;
            }
        }
    }
}
