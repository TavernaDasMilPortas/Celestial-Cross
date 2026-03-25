using UnityEngine;
using UnityEngine.UI;

public class CameraModeButton : MonoBehaviour
{
    [Header("UI")]
    public Image icon;

    [Header("Opacity")]
    [Range(0f, 1f)]
    public float activeAlpha = 1f;

    [Range(0f, 1f)]
    public float inactiveAlpha = 0.4f;

    void Start()
    {
        RefreshVisual();
    }

    // =========================
    // UI CALLBACK
    // =========================

    public void ToggleFreeCamera()
    {
        if (CameraController.Instance == null)
            return;

        bool enableFree =
            CameraController.Instance.cameraMode
            != CameraController.CameraMode.Free;

        CameraController.Instance.EnableFreeCamera(enableFree);
        RefreshVisual();
    }

    // =========================
    // VISUAL
    // =========================

    void RefreshVisual()
    {
        if (icon == null || CameraController.Instance == null)
            return;

        bool isFree =
            CameraController.Instance.cameraMode
            == CameraController.CameraMode.Free;

        Color c = icon.color;
        c.a = isFree ? activeAlpha : inactiveAlpha;
        icon.color = c;
    }
}
