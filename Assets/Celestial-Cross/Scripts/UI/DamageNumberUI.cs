using UnityEngine;
using TMPro;
using DG.Tweening;

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

    private Sequence currentSequence;
    private float currentAnimYOffset = 0f;
    private Vector2 startAnchoredPos;

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

        RectTransform rect = GetComponent<RectTransform>();
        if (rect != null && !worldSpace) startAnchoredPos = rect.anchoredPosition;

        StartAnimation();
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
        UpdatePosition(0f);

        StartAnimation();
    }

    private void StartAnimation()
    {
        if (currentSequence != null) currentSequence.Kill();
        
        Vector3 initialScale = Vector3.one; 
        Color startColor = textMesh != null ? textMesh.color : Color.white;
        startColor.a = 1f;
        if (textMesh != null) textMesh.color = startColor;

        transform.localScale = Vector3.zero;
        currentAnimYOffset = 0f;

        currentSequence = DOTween.Sequence();
        
        // 1. Pop-in Elástico e rápido (0.3s)
        currentSequence.Append(transform.DOScale(initialScale * 1.3f, 0.3f).SetEase(Ease.OutBack, 2f));
        currentSequence.Append(transform.DOScale(initialScale, 0.1f));
        
        // 2. Movimento Y fluído durante toda a duração
        currentSequence.Insert(0, DOTween.To(() => currentAnimYOffset, x => currentAnimYOffset = x, moveSpeed, duration).SetEase(Ease.OutQuad));
        
        // 3. Fade out na reta final
        float fadeDuration = duration * 0.4f;
        float fadeStartTime = duration - fadeDuration;
        
        currentSequence.Insert(fadeStartTime, transform.DOScale(Vector3.zero, fadeDuration).SetEase(Ease.InBack));
        if (textMesh != null)
        {
            currentSequence.Insert(fadeStartTime, textMesh.DOFade(0f, fadeDuration));
        }

        // 4. Retornar ao Pool
        currentSequence.OnComplete(() => {
            if (DamagePopupManager.Instance != null) {
                DamagePopupManager.Instance.ReturnToPool(gameObject);
            } else {
                Destroy(gameObject);
            }
        });
    }

    private void Update()
    {
        if (currentSequence != null && currentSequence.IsPlaying())
        {
            UpdatePosition(currentAnimYOffset);
        }
    }

    private void UpdatePosition(float yOffset)
    {
        RectTransform rect = GetComponent<RectTransform>();
        
        if (followTarget && rect != null)
        {
            if (targetTransform != null)
            {
                lastWorldPos = targetTransform.position;
            }

            Vector3 current3DPos = lastWorldPos + spawnOffset + jitterOffset + (Vector3.up * yOffset);
            Vector3 canvasWorldPos = Vector3.zero;
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
            // Movimento clássico para cima sem seguir um alvo
            if (!isWorldSpace && rect != null)
            {
                rect.anchoredPosition = startAnchoredPos + Vector2.up * (yOffset * 100f);
            }
            else
            {
                transform.position = lastWorldPos + spawnOffset + jitterOffset + (Vector3.up * yOffset);
            }
        }
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

