using UnityEngine;
using TMPro;
using System.Collections;

public class DamageNumberUI : MonoBehaviour
{
    [SerializeField] private TMP_Text textMesh;
    [SerializeField] private float duration = 1f;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 1f, 1, 1.2f);

    private bool isWorldSpace = true;
    private bool followTarget = false;
    private Transform targetTransform;
    private Vector3 spawnOffset;
    private Vector3 jitterOffset;
    private Vector3 lastWorldPos;

    public void Setup(int amount, Color color, string prefix, bool worldSpace = true)
    {
        this.followTarget = false;
        this.isWorldSpace = worldSpace;
        
        if (textMesh == null) textMesh = GetComponentInChildren<TMP_Text>();
        
        if (textMesh != null)
        {
            textMesh.text = $"{prefix}{amount}";
            textMesh.color = color;
        }

        StartCoroutine(Animate());
    }

    public void Setup(Transform target, Vector3 offset, Vector3 jitter, int amount, Color color, string prefix)
    {
        this.targetTransform = target;
        this.spawnOffset = offset;
        this.jitterOffset = jitter;
        if (target != null)
        {
            this.lastWorldPos = target.position;
        }
        this.followTarget = true;
        this.isWorldSpace = false;

        if (textMesh == null) textMesh = GetComponentInChildren<TMP_Text>();

        if (textMesh != null)
        {
            textMesh.text = $"{prefix}{amount}";
            textMesh.color = color;
        }

        // Posicionamento imediato para evitar glitch visual de 1 frame
        RectTransform rect = GetComponent<RectTransform>();
        if (rect != null)
        {
            Vector3 current3DPos = lastWorldPos + spawnOffset + jitterOffset;
            Vector3 canvasWorldPos;
            bool positionFound = false;

            if (RenderTextureInputManager.Instance != null && RenderTextureInputManager.Instance.WorldToCanvasWorldPoint(current3DPos, out canvasWorldPos))
            {
                positionFound = true;
            }
            else if (Camera.main != null && transform.parent != null)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(current3DPos);
                if (screenPos.z >= 0)
                {
                    RectTransform parentRect = transform.parent as RectTransform;
                    Canvas canvas = GetComponentInParent<Canvas>();
                    Camera uiCamera = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;

                    if (parentRect != null && RectTransformUtility.ScreenPointToWorldPointInRectangle(parentRect, screenPos, uiCamera, out Vector3 worldPoint))
                    {
                        canvasWorldPos = worldPoint;
                        positionFound = true;
                    }
                }
            }

            if (positionFound)
            {
                rect.position = canvasWorldPos;
            }
        }

        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        float elapsed = 0f;
        RectTransform rect = GetComponent<RectTransform>();
        Vector2 startAnchoredPos = (rect != null && !isWorldSpace) ? rect.anchoredPosition : Vector2.zero;
        Vector3 startWorldPos = transform.position;
        Vector3 initialScale = transform.localScale;
        Color startColor = textMesh != null ? textMesh.color : Color.white;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            if (followTarget && rect != null)
            {
                if (targetTransform != null)
                {
                    lastWorldPos = targetTransform.position;
                }

                // Calcula a posição 3D do alvo atualizada, somando a subida da animação e o jitter
                Vector3 current3DPos = lastWorldPos + spawnOffset + jitterOffset + (Vector3.up * (elapsed * moveSpeed));
                Vector3 canvasWorldPos;
                bool positionFound = false;

                // 1. Tenta converter usando o RenderTextureInputManager (caso esteja ativo)
                if (RenderTextureInputManager.Instance != null && RenderTextureInputManager.Instance.WorldToCanvasWorldPoint(current3DPos, out canvasWorldPos))
                {
                    positionFound = true;
                }
                // 2. Se não estiver ativo, tenta projetar usando a Câmera Principal diretamente no Canvas da UI
                else if (Camera.main != null && transform.parent != null)
                {
                    Vector3 screenPos = Camera.main.WorldToScreenPoint(current3DPos);
                    if (screenPos.z >= 0)
                    {
                        RectTransform parentRect = transform.parent as RectTransform;
                        Canvas canvas = GetComponentInParent<Canvas>();
                        Camera uiCamera = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;

                        if (parentRect != null && RectTransformUtility.ScreenPointToWorldPointInRectangle(parentRect, screenPos, uiCamera, out Vector3 worldPoint))
                        {
                            canvasWorldPos = worldPoint;
                            positionFound = true;
                        }
                    }
                }

                if (positionFound)
                {
                    rect.position = canvasWorldPos;
                }
            }
            else
            {
                // Movimento clássico para cima
                if (!isWorldSpace && rect != null)
                {
                    rect.anchoredPosition = startAnchoredPos + Vector2.up * (elapsed * moveSpeed * 100f);
                }
                else
                {
                    transform.position = startWorldPos + Vector3.up * (elapsed * moveSpeed);
                }
            }

            // Escala (pop-in respeitando a escala inicial do prefab)
            float scaleValue = scaleCurve.Evaluate(t);
            transform.localScale = initialScale * scaleValue;

            // Fade out
            if (textMesh != null)
            {
                Color c = startColor;
                c.a = 1f - t;
                textMesh.color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    private void LateUpdate()
    {
        // Garante que o texto sempre encare a câmera (Billboard) apenas em World Space
        if (isWorldSpace && Camera.main != null)
        {
            transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
                             Camera.main.transform.rotation * Vector3.up);
        }
    }
}

