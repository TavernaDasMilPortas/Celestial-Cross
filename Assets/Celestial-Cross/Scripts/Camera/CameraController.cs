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
    public float heightOffset = 15f;
    public float depthOffset = 5f;

    [Header("Rotation")]
    public Vector3 cameraRotation = new(75f, 0f, 0f);

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

    [Header("Clamping & Framing")]
    public float edgePadding = 1f;
    public float verticalCenterOffset = 1f; // Sobe um pouco o foco dos pés para o corpo

    // =========================
    // UNITY
    // =========================

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        TurnManager.OnTurnStarted += Follow;
    }

    void OnDisable()
    {
        TurnManager.OnTurnStarted -= Follow;
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

        // Centraliza na base + offset vertical para não cortar a cabeça
        targetProjectedPoint = followTarget.transform.position + Vector3.forward * verticalCenterOffset;
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
        if (bounds == null || cam == null || bounds.bottomLeft == null || bounds.topRight == null)
            return;

        float orthoHeight = cam.orthographicSize;
        float orthoWidth = orthoHeight * cam.aspect;

        // Adiciona padding para os personagens "respirarem" nas bordas
        float minX = bounds.bottomLeft.position.x - edgePadding;
        float maxX = bounds.topRight.position.x + edgePadding;
        float minZ = bounds.bottomLeft.position.z - edgePadding;
        float maxZ = bounds.topRight.position.z + edgePadding;

        float clampedX, clampedZ;

        // Se o mapa for menor que a tela no eixo X, centraliza no mapa
        if (maxX - minX < orthoWidth * 2f)
            clampedX = (minX + maxX) / 2f;
        else
            clampedX = Mathf.Clamp(targetProjectedPoint.x, minX + orthoWidth, maxX - orthoWidth);

        // Se o mapa for menor que a tela no eixo Z, centraliza no mapa
        if (maxZ - minZ < orthoHeight * 2f)
            clampedZ = (minZ + maxZ) / 2f;
        else
            clampedZ = Mathf.Clamp(targetProjectedPoint.z, minZ + orthoHeight, maxZ - orthoHeight);

        targetProjectedPoint = new Vector3(clampedX, 0f, clampedZ);
    }

    void ApplyMovement()
    {
        float zoomFactor = Mathf.Clamp(
            cam.orthographicSize * followZoomMultiplier,
            minFollowFactor,
            maxFollowFactor
        );

        float speed = followSpeed * zoomFactor;

        // Simplificação: Para centralizar no orthographic, a posição da câmera deve ser:
        // PosiçãoFocada - forward * distância
        // Usamos depthOffset como essa distância total. heightOffset passa a ser 0 ou um ajuste fino.
        Vector3 desiredCameraPos =
            targetProjectedPoint
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
        Ray ray = new Ray(camPos, cam.transform.forward);
        if (mapPlane.Raycast(ray, out float dist))
            return ray.GetPoint(dist);

        return camPos;
    }

    // =========================
    // PUBLIC API
    // =========================

    public void Follow(Unit unit)
    {
        if (unit == null) return;
        Debug.Log($"[CameraController] Seguindo unidade: {unit.DisplayName}");
        followTarget = unit;
        cameraMode = CameraMode.FollowUnit;

        // Se for a primeira unidade ou o herói, podemos forçar um snap inicial
        if (Time.timeSinceLevelLoad < 2f) 
            SnapToTarget();
    }

    public void SnapToTarget()
    {
        HandleFollow();
        ApplyClamp();
        
        Vector3 desiredCameraPos = targetProjectedPoint - cam.transform.forward * depthOffset;
        transform.position = desiredCameraPos;
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
