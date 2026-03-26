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
                touchStart = touch.position;
                isSwiping = true;
                break;

            case TouchPhase.Ended:
                if (isSwiping)
                {
                    EvaluateSwipe(touch.position);
                    isSwiping = false;
                }
                break;

            case TouchPhase.Canceled:
                isSwiping = false;
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
            touchStart = Input.mousePosition;
            isSwiping = true;
        }
        else if (Input.GetMouseButtonUp(0) && isSwiping)
        {
            EvaluateSwipe(Input.mousePosition);
            isSwiping = false;
        }
    }

    // =============================
    // AVALIAÇÃO
    // =============================

    void EvaluateSwipe(Vector2 endPos)
    {
        float deltaX = endPos.x - touchStart.x;

        if (Mathf.Abs(deltaX) < minSwipeDistance) return;

        if (deltaX < 0)
            OnSwipeLeft?.Invoke();
        else
            OnSwipeRight?.Invoke();
    }
}
