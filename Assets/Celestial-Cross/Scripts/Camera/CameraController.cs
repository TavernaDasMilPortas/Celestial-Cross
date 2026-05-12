using UnityEngine;
using System.Collections.Generic;

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
    // DRAG STATE
    // =========================

    /// <summary>
    /// Distância mínima (em pixels) que o dedo/mouse precisa arrastar antes de iniciar o drag.
    /// Evita conflito com toques de seleção de unidades.
    /// </summary>
    [Header("Drag")]
    public float dragThresholdPixels = 15f;

    // Touch drag state
    bool isTouchDragging;
    Vector2 touchDragStartScreenPos;

    // Mouse drag state (para testes no Editor)
    bool isMouseDragging;
    Vector3 mouseDragStartScreenPos;

    // Direções de arrasto no plano XZ, calculadas uma vez na Start
    Vector3 dragRight;
    Vector3 dragForward;

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
        originalZoom = targetZoom;

        cam.nearClipPlane = Mathf.Max(cam.nearClipPlane, 0.3f);

        // Pre-calcula as direções de arrasto no plano XZ.
        // Usa a rotação Y da câmera para definir "right" e "forward" no ground plane,
        // ignorando o pitch (ângulo X) que causa problemas quando é muito inclinado (ex: 75°).
        float yaw = cameraRotation.y * Mathf.Deg2Rad;
        dragRight   = new Vector3(Mathf.Cos(yaw), 0f, -Mathf.Sin(yaw));
        dragForward = new Vector3(Mathf.Sin(yaw), 0f,  Mathf.Cos(yaw));
    }

    void Update()
    {
        HandleKeyboard();

        // Always check drags. If drag happened, it will set CameraMode.Free automatically
        HandleDrag();
        HandleMouseDrag();

        switch (cameraMode)
        {
            case CameraMode.Free:
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
    // INPUT (FREE/DRAG)
    // =========================

    void HandleDrag()
    {
        if (Input.touchCount != 1)
        {
            isTouchDragging = false;
            return;
        }

        Touch touch = Input.GetTouch(0);

        // Início do toque: registra posição e aguarda threshold
        if (touch.phase == TouchPhase.Began)
        {
            isTouchDragging = false;
            touchDragStartScreenPos = touch.position;
            return;
        }

        // Fim do toque: reseta estado
        if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        {
            isTouchDragging = false;
            return;
        }

        if (touch.phase != TouchPhase.Moved)
            return;

        // Verifica threshold antes de considerar como drag
        if (!isTouchDragging)
        {
            float distFromStart = Vector2.Distance(touch.position, touchDragStartScreenPos);
            if (distFromStart < dragThresholdPixels)
                return;

            isTouchDragging = true;

            if (cameraMode == CameraMode.FollowUnit)
                EnableFreeCamera(true);
        }

        Vector2 delta = touch.deltaPosition;
        if (delta.sqrMagnitude < 0.1f) return;

        // Usa as direções pré-calculadas no plano XZ (independente do pitch da câmera)
        Vector3 move =
            (-dragRight * delta.x + -dragForward * delta.y) * dragSpeed;

        targetProjectedPoint += move;
    }

    void HandleMouseDrag()
    {
        // Qualquer botão do mouse pode arrastar a câmera (esquerdo, direito, meio).
        // O threshold de pixels evita conflito com cliques de seleção.
        bool anyButton = Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2);

        if (!anyButton)
        {
            isMouseDragging = false;
            return;
        }

        // Início do arrasto: registra posição inicial
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            isMouseDragging = false;
            mouseDragStartScreenPos = Input.mousePosition;
            return;
        }

        // Verifica threshold antes de iniciar o arrasto de fato
        if (!isMouseDragging)
        {
            float distFromStart = Vector3.Distance(Input.mousePosition, mouseDragStartScreenPos);
            if (distFromStart < dragThresholdPixels)
                return;

            isMouseDragging = true;

            if (cameraMode == CameraMode.FollowUnit)
                EnableFreeCamera(true);
        }

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        if (Mathf.Abs(mouseX) < 0.001f && Mathf.Abs(mouseY) < 0.001f)
            return;

        // Sensibilidade do mouse baseada no zoom para ser consistente
        float sensitivity = cam.orthographicSize * 0.1f;

        // Usa as direções pré-calculadas no plano XZ
        Vector3 move = (-dragRight * mouseX + -dragForward * mouseY) * sensitivity;

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

        float minMapX = bounds.bottomLeft.position.x;
        float maxMapX = bounds.topRight.position.x;
        float minMapZ = bounds.bottomLeft.position.z;
        float maxMapZ = bounds.topRight.position.z;

        float mapWidth = maxMapX - minMapX;
        float mapDepth = maxMapZ - minMapZ;

        // Calcula metade da viewport visível em unidades de mundo.
        float halfHeight = cam.orthographicSize;
        float halfWidth = cam.orthographicSize * cam.aspect;

        float pitchRad = cameraRotation.x * Mathf.Deg2Rad;
        float sinPitch = Mathf.Sin(pitchRad);
        float extentZ = (sinPitch > 0.01f) ? halfHeight / sinPitch : halfHeight;
        float extentX = halfWidth;

        // Calcula os limites para o ponto focal (targetProjectedPoint)
        // Se o mapa for menor que a viewport, o 'limite' deve ser o centro do mapa
        float minX, maxX, minZ, maxZ;

        if (mapWidth > extentX * 2f) {
            minX = minMapX + extentX - edgePadding;
            maxX = maxMapX - extentX + edgePadding;
        } else {
            minX = maxX = (minMapX + maxMapX) * 0.5f;
        }

        if (mapDepth > extentZ * 2f) {
            minZ = minMapZ + extentZ - edgePadding;
            maxZ = maxMapZ - extentZ + edgePadding;
        } else {
            minZ = maxZ = (minMapZ + maxMapZ) * 0.5f;
        }

        float clampedX = Mathf.Clamp(targetProjectedPoint.x, minX, maxX);
        float clampedZ = Mathf.Clamp(targetProjectedPoint.z, minZ, maxZ);

        targetProjectedPoint = new Vector3(clampedX, 0f, clampedZ);
        
        // Opcional: Impedir que o targetZoom seja tão grande que ultrapasse muito o mapa
        float maxPossibleZoomX = (mapWidth * 0.5f) / cam.aspect;
        float maxPossibleZoomZ = (mapDepth * 0.5f) * sinPitch;
        float zoomLimit = Mathf.Max(maxPossibleZoomX, maxPossibleZoomZ, maxZoom);
        
        targetZoom = Mathf.Clamp(targetZoom, minZoom, zoomLimit);
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

    public void FrameGridPositions(IEnumerable<Vector2Int> gridPositions)
    {
        if (gridPositions == null) return;

        bool hasBounds = false;
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);

        foreach (var pos in gridPositions)
        {
            Vector3 worldPos = GridMap.Instance.GridToWorld(pos);
            if (!hasBounds)
            {
                bounds = new Bounds(worldPos, Vector3.zero);
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(worldPos);
            }
        }

        if (hasBounds)
        {
            targetProjectedPoint = bounds.center;
            
            float maxExtent = Mathf.Max(bounds.size.x, bounds.size.z);
            float paddedExtent = maxExtent * 0.5f + edgePadding * 2f;
            
            targetZoom = Mathf.Clamp(paddedExtent, minZoom, maxZoom * 1.25f);
            cameraMode = CameraMode.Free;
        }
    }

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

        // Foca na unidade dona da ação
        if (followTarget != null)
        {
            targetProjectedPoint = followTarget.transform.position + Vector3.forward * verticalCenterOffset;
        }

        // Só salva o estado original se não estivermos já focados em uma ação
        if (targetedAction == null)
        {
            originalZoom = targetZoom;
            originalMode = cameraMode;
        }

        targetedAction = action;

        // Se o alcance for grande, precisamos de uma visão mais ampla ou livre
        if (action.Range >= 4)
        {
            targetZoom = Mathf.Clamp(Mathf.Max(targetZoom, 10f), minZoom, maxZoom); 
            
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
