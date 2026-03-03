using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    public enum CameraMode
    {
        FollowUnit,
        Free
    }

    // =========================
    // REFS
    // =========================

    [Header("Refs")]
    public Camera cam;
    public CameraBounds bounds;

    // =========================
    // VIEW CONFIG
    // =========================

    [Header("View Offsets")]
    public float heightOffset = 12f;   // altura real da câmera
    public float depthOffset = 12f;    // recuo visual (forward)

    [Header("Rotation")]
    public Vector3 cameraRotation = new(45f, 45f, 0f);

    // =========================
    // MOVEMENT
    // =========================

    [Header("Movement")]
    public float dragSpeed = 0.01f;
    public float followSpeed = 10f;

    [Header("Follow Scaling")]
    public float followZoomMultiplier = 0.8f;
    public float minFollowFactor = 0.6f;
    public float maxFollowFactor = 2.2f;

    // =========================
    // ZOOM
    // =========================

    [Header("Zoom")]
    public float zoomSpeed = 0.1f;
    public float minZoom = 5f;
    public float maxZoom = 12f;

    // =========================
    // STATE
    // =========================

    [Header("State")]
    public CameraMode cameraMode = CameraMode.FollowUnit;

    Vector3 targetProjectedPoint;
    float targetZoom;

    Unit followTarget;

    Plane mapPlane = new(Vector3.up, Vector3.zero);

    // =========================
    // UNITY
    // =========================

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (cam == null)
            cam = Camera.main;

        cam.orthographic = true;
        targetZoom = cam.orthographicSize;

        transform.rotation = Quaternion.Euler(cameraRotation);

        targetProjectedPoint = ProjectCameraToPlane(transform.position);

        cam.nearClipPlane = Mathf.Max(cam.nearClipPlane, 0.3f);
    }

    void Update()
    {
        switch (cameraMode)
        {
            case CameraMode.Free:
                HandleDrag();
                HandleZoom();
                break;

            case CameraMode.FollowUnit:
                HandleFollow();
                break;
        }

        ApplyClamp();
        ApplyMovement();
    }

    // =========================
    // MODES
    // =========================

    void HandleFollow()
    {
        if (followTarget == null)
            return;

        targetProjectedPoint = followTarget.transform.position;
    }

    // =========================
    // INPUT (FREE MODE)
    // =========================

    void HandleDrag()
    {
        if (Input.touchCount != 1)
            return;

        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Moved)
            return;

        Vector2 delta = touch.deltaPosition;

        Vector3 right = cam.transform.right;
        Vector3 forward = cam.transform.forward;

        right.y = 0f;
        forward.y = 0f;

        right.Normalize();
        forward.Normalize();

        Vector3 move =
            (-right * delta.x + -forward * delta.y) * dragSpeed;

        targetProjectedPoint += move;
    }

    void HandleZoom()
    {
        if (Input.touchCount != 2)
            return;

        Touch t0 = Input.GetTouch(0);
        Touch t1 = Input.GetTouch(1);

        Vector2 prev0 = t0.position - t0.deltaPosition;
        Vector2 prev1 = t1.position - t1.deltaPosition;

        float prevDist = Vector2.Distance(prev0, prev1);
        float currDist = Vector2.Distance(t0.position, t1.position);

        float delta = currDist - prevDist;

        targetZoom -= delta * zoomSpeed;
        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
    }

    // =========================
    // APPLY
    // =========================

    void ApplyClamp()
    {
        if (bounds == null)
            return;

        targetProjectedPoint =
            bounds.ClampProjectedPoint(targetProjectedPoint);
    }

    void ApplyMovement()
    {
        float zoomFactor = Mathf.Clamp(
            cam.orthographicSize * followZoomMultiplier,
            minFollowFactor,
            maxFollowFactor
        );

        float speed = followSpeed * zoomFactor;

        Vector3 desiredCameraPos =
            targetProjectedPoint
            + Vector3.up * heightOffset
            - cam.transform.forward * depthOffset;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredCameraPos,
            Time.deltaTime * speed
        );

        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize,
            targetZoom,
            Time.deltaTime * speed
        );
    }

    // =========================
    // UTIL
    // =========================

    Vector3 ProjectCameraToPlane(Vector3 camPos)
    {
        Ray ray = new Ray(camPos, -cam.transform.up);
        if (mapPlane.Raycast(ray, out float dist))
            return ray.GetPoint(dist);

        return camPos;
    }

    // =========================
    // PUBLIC API
    // =========================

    public void Follow(Unit unit)
    {
        followTarget = unit;
        cameraMode = CameraMode.FollowUnit;
    }

    public void EnableFreeCamera(bool enable)
    {
        cameraMode = enable ? CameraMode.Free : CameraMode.FollowUnit;
    }

    // =========================
    // DEBUG
    // =========================

void OnDrawGizmos()
{
    if (!Application.isPlaying)
        return;

    // ponto desejado (antes do clamp)
    Gizmos.color = Color.blue;
    Gizmos.DrawSphere(targetProjectedPoint, 0.25f);

    if (bounds != null)
    {
        Vector3 clamped =
            bounds.ClampProjectedPoint(targetProjectedPoint);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(clamped, 0.3f);
    }
}

}
