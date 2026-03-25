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
    IUnitAction targetedAction;
    float originalZoom;
    CameraMode originalMode;

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
        HandleKeyboard();

        switch (cameraMode)
        {
            case CameraMode.Free:
                HandleDrag();
                HandleMouseDrag();
                HandleZoom();
                HandleMouseZoom();
                break;

            case CameraMode.FollowUnit:
                HandleFollow();
                HandleMouseZoom();
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

    void HandleMouseDrag()
    {
        if (!Input.GetMouseButton(1)) // Clique direito
            return;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        Vector3 right = cam.transform.right;
        Vector3 forward = cam.transform.forward;

        right.y = 0f;
        forward.y = 0f;
        right.Normalize();
        forward.Normalize();

        // Sensibilidade do mouse baseada no zoom para ser consistente
        float sensitivity = cam.orthographicSize * 0.1f;
        Vector3 move = (-right * mouseX + -forward * mouseY) * sensitivity;

        targetProjectedPoint += move;
    }

    void HandleKeyboard()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            bool isFree = cameraMode == CameraMode.Free;
            EnableFreeCamera(!isFree);
            
            // Se as pessoas usarem a UI, o botão deve atualizar. 
            // Como não temos ref direta fácil sem Find, deixamos a UI se atualizar se houver evento (opcional)
        }
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

        UpdateZoomState(-delta * zoomSpeed);
    }

    void HandleMouseZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            UpdateZoomState(-scroll * 10f * zoomSpeed);
        }
    }

    void UpdateZoomState(float delta)
    {
        targetZoom += delta;
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

        // Limites estritos do mapa (sem padding externo para não mostrar o vazio)
        float minMapX = bounds.bottomLeft.position.x;
        float maxMapX = bounds.topRight.position.x;
        float minMapZ = bounds.bottomLeft.position.z;
        float maxMapZ = bounds.topRight.position.z;

        float clampedX, clampedZ;

        // Se o mapa for menor que a tela no eixo X, centraliza no mapa
        if (maxMapX - minMapX < orthoWidth * 2f)
            clampedX = (minMapX + maxMapX) / 2f;
        else
            clampedX = Mathf.Clamp(targetProjectedPoint.x, minMapX + orthoWidth, maxMapX - orthoWidth);

        // Se o mapa for menor que a tela no eixo Z, centraliza no mapa
        if (maxMapZ - minMapZ < orthoHeight * 2f)
            clampedZ = (minMapZ + maxMapZ) / 2f;
        else
            clampedZ = Mathf.Clamp(targetProjectedPoint.z, minMapZ + orthoHeight, maxMapZ - orthoHeight);

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
        Debug.Log($"[CameraController] Modo alterado para: {cameraMode}");
    }

    public void SetActionFocus(IUnitAction action)
    {
        if (action == null)
        {
            ResetFocus();
            return;
        }

        targetedAction = action;
        
        // Salva estado anterior para poder voltar
        if (cameraMode != CameraMode.Free || targetedAction == null)
        {
            originalZoom = targetZoom;
            originalMode = cameraMode;
        }

        // Se o alcance for grande, precisamos de uma visão mais ampla ou livre
        if (action.Range >= 4)
        {
            targetZoom = Mathf.Max(targetZoom, 10f); // Zoom out mínimo para ver a ação
            
            if (action.Range >= 7)
            {
                cameraMode = CameraMode.Free;
                Debug.Log("[CameraController] Alcance alto detectado. Habilitando Câmera Livre e Zoom Out.");
            }
        }
    }

    public void ResetFocus()
    {
        if (targetedAction == null) return;

        targetedAction = null;
        targetZoom = originalZoom;
        cameraMode = originalMode;
        Debug.Log("[CameraController] Foco da ação resetado.");
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
