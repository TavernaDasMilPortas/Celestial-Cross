using UnityEngine;
using System;

/// <summary>
/// Detecta gestos de swipe horizontal (touch e mouse).
/// Dispara eventos OnSwipeLeft / OnSwipeRight para navegação entre abas.
/// </summary>
public class SwipeDetector : MonoBehaviour
{
    [Header("Configuração")]
    [Tooltip("Distância mínima em pixels para considerar um swipe")]
    public float minSwipeDistance = 80f;

    public event Action OnSwipeLeft;
    public event Action OnSwipeRight;
    public bool IsGestureOverRenderTarget { get; private set; }
    public bool IsSwipeInProgress { get; private set; }
    public bool WasSwipeConsumed { get; private set; }

    private Vector2 touchStart;
    private bool isSwiping;

    void Update()
    {
        #if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseSwipe();
        #else
        HandleTouchSwipe();
        #endif
    }

    // =============================
    // TOUCH (Mobile)
    // =============================

    void HandleTouchSwipe()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                IsGestureOverRenderTarget = RenderTextureInputManager.Instance == null
                    || !RenderTextureInputManager.Instance.IsRaycastTargetReady()
                    || RenderTextureInputManager.Instance.IsScreenPointOverExclusiveRenderTarget(touch.position);

                if (!IsGestureOverRenderTarget)
                {
                    Debug.Log($"[SwipeDetector] Ignorado: touch began fora do RawImage em {touch.position}");
                    isSwiping = false;
                    IsSwipeInProgress = false;
                    WasSwipeConsumed = false;
                    break;
                }

                touchStart = touch.position;
                isSwiping = true;
                IsSwipeInProgress = true;
                WasSwipeConsumed = false;
                break;

            case TouchPhase.Ended:
                if (isSwiping)
                {
                    WasSwipeConsumed = EvaluateSwipe(touch.position);
                    isSwiping = false;
                    IsSwipeInProgress = false;
                }
                break;

            case TouchPhase.Canceled:
                isSwiping = false;
                IsSwipeInProgress = false;
                WasSwipeConsumed = false;
                break;
        }
    }

    // =============================
    // MOUSE (Editor / Desktop)
    // =============================

    void HandleMouseSwipe()
    {
        if (Input.GetMouseButtonDown(0))
        {
            IsGestureOverRenderTarget = RenderTextureInputManager.Instance == null
                || !RenderTextureInputManager.Instance.IsRaycastTargetReady()
                || RenderTextureInputManager.Instance.IsScreenPointOverExclusiveRenderTarget(Input.mousePosition);

            if (!IsGestureOverRenderTarget)
            {
                Debug.Log($"[SwipeDetector] Ignorado: mouse down fora do RawImage em {Input.mousePosition}");
                isSwiping = false;
                IsSwipeInProgress = false;
                WasSwipeConsumed = false;
                return;
            }

            touchStart = Input.mousePosition;
            isSwiping = true;
            IsSwipeInProgress = true;
            WasSwipeConsumed = false;
        }
        else if (Input.GetMouseButtonUp(0) && isSwiping)
        {
            WasSwipeConsumed = EvaluateSwipe(Input.mousePosition);
            isSwiping = false;
            IsSwipeInProgress = false;
        }
    }

    // =============================
    // AVALIAÇÃO
    // =============================

    bool EvaluateSwipe(Vector2 endPos)
    {
        float deltaX = endPos.x - touchStart.x;

        if (Mathf.Abs(deltaX) < minSwipeDistance) return false;

        if (deltaX < 0)
            OnSwipeLeft?.Invoke();
        else
            OnSwipeRight?.Invoke();

        return true;
    }
}
