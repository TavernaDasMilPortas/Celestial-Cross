using UnityEngine;
using System.Collections;
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
    public bool autoConfigZoom = true; // Auto ajusta com base no tamanho do mapa
    
    [System.Obsolete("Use initialTilesWidthToSee and initialTilesHeightToSee instead")]
    [HideInInspector]
    public float initialTilesToSee = 4f; // Tamanho do grid que a câmera vai tentar focar no início
    
    [Tooltip("Largura do grid que a câmera vai tentar focar no início (em tiles)")]
    public float initialTilesWidthToSee = 4f;
    
    [System.Obsolete("Use apenas a largura para controle de zoom.")]
    [HideInInspector]
    public float initialTilesHeightToSee = 4f;
    
    [Tooltip("Tamanho fixo do tile (em metros). Deixe o Reference Tile vazio se preferir configurar o tamanho por esse número.")]
    public float tileSizeOverride = 1f; 
    
    [Tooltip("Tile de referência para calcular o tamanho real. Se vazio, usa o TileSizeOverride.")]
    public GameObject referenceTile;
    public float minZoom = 2f;
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
    public float edgePadding = 0f;
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
    bool isTouchDragEligible;
    Vector2 touchDragStartScreenPos;

    // Mouse drag state (para testes no Editor)
    bool isMouseDragging;
    bool isMouseDragEligible;
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
        
#pragma warning disable CS0618
        if (initialTilesWidthToSee == 4f && initialTilesToSee != 4f)
        {
            initialTilesWidthToSee = initialTilesToSee;
        }
#pragma warning restore CS0618
        
        StartCoroutine(DelayedSetupZoom());

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

    System.Collections.IEnumerator DelayedSetupZoom()
    {
        if (RenderTextureInputManager.Instance != null && RenderTextureInputManager.Instance.createAndAssignTextureOnStart)
        {
            while (!RenderTextureInputManager.Instance.IsInitialized)
            {
                yield return null;
            }
        }
        else
        {
            yield return new WaitForEndOfFrame();
        }

        float calculatedTileSize = ResolveLogicalTileSize();
        float referenceTileSize = ResolveReferenceTileSize();

        Debug.Log($"[Camera] Tamanho métrico do tile lógico usado no zoom: {calculatedTileSize}. Referência visual={(referenceTile != null ? referenceTile.name : "tileSizeOverride")}, tile visual medido={referenceTileSize}");

        if (autoConfigZoom && bounds != null)
        {
            SetupAutoZoom(calculatedTileSize);
        }
    }

    void Update()
    {
        HandleKeyboard();

        // Always check drags. If drag happened, it will set CameraMode.Free automatically
        HandleDrag();
        HandleMouseDrag();

        if (cameraMode == CameraMode.Free && pendingFollowCoroutine != null)
        {
            StopCoroutine(pendingFollowCoroutine);
            pendingFollowCoroutine = null;
        }

        switch (cameraMode)
        {
            case CameraMode.Free:
                // HandleZoom();
                // HandleMouseZoom();
                break;

            case CameraMode.FollowUnit:
                HandleFollow();
                // HandleMouseZoom();
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
            isTouchDragEligible = false;
            return;
        }

        Touch touch = Input.GetTouch(0);

        // Início do toque: registra posição e aguarda threshold
        if (touch.phase == TouchPhase.Began)
        {
            isTouchDragEligible = IsPointerOverRenderTarget(touch.position);
            isTouchDragging = false;

            if (!isTouchDragEligible)
            {
                return;
            }

            touchDragStartScreenPos = touch.position;
            return;
        }

        if (!isTouchDragEligible)
            return;

        // Fim do toque: reseta estado
        if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        {
            isTouchDragging = false;
            isTouchDragEligible = false;
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
            isMouseDragEligible = false;
            return;
        }

        // Início do arrasto: registra posição inicial
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            isMouseDragEligible = IsPointerOverRenderTarget(Input.mousePosition);
            isMouseDragging = false;

            if (!isMouseDragEligible)
            {
                return;
            }

            mouseDragStartScreenPos = Input.mousePosition;
            return;
        }

        if (!isMouseDragEligible)
            return;

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

    bool IsPointerOverRenderTarget(Vector2 screenPos)
    {
        if (RenderTextureInputManager.Instance == null || !RenderTextureInputManager.Instance.IsRaycastTargetReady())
            return false;

        return RenderTextureInputManager.Instance.IsScreenPointOverExclusiveRenderTarget(screenPos);
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

        // Com tileExtensao os limites do mapa consideram a extremidade externa do quadrado do último tile
        float minMapX = bounds.bottomLeft.position.x - bounds.tileExtensao;
        float maxMapX = bounds.topRight.position.x + bounds.tileExtensao;
        float minMapZ = bounds.bottomLeft.position.z - bounds.tileExtensao;
        float maxMapZ = bounds.topRight.position.z + bounds.tileExtensao;

        // Calcula a extenção visível da camera no plano do mapa
        float camHalfHeight = targetZoom / Mathf.Sin(cameraRotation.x * Mathf.Deg2Rad);
        // Quando usamos Render Texture 1:1, a câmera orográfica na targetTexture tem o formato real de `pixelWidth / pixelHeight`. 
        // Idealmente a Render Texture é quadrada, então aspect == 1.
        float currentAspect = cam.targetTexture != null 
                            ? (float)cam.targetTexture.width / cam.targetTexture.height 
                            : cam.aspect;
                            
        float camHalfWidth = targetZoom * currentAspect;
        
        // Garante que o zoom máximo NUNCA seja tão grande que a câmera veja além do mapa.
        float mapWidth = maxMapX - minMapX;
        float mapHeight = maxMapZ - minMapZ;
        
        // Se quisermos que a câmera preencha no máximo o menor eixo do mapa...
        float maxAllowedZoomByWidth = (mapWidth / 2f) / currentAspect;
        float maxAllowedZoomByHeight = (mapHeight / 2f) * Mathf.Sin(cameraRotation.x * Mathf.Deg2Rad);
        
        // Usa o menor dos dois limites lógicos para garantir que ele caiba nos dois eixos (se quiser limitar estrito)
        float maxGlobalZoom = Mathf.Min(maxAllowedZoomByWidth, maxAllowedZoomByHeight);
        
        // Assegura que o targetZoom nunca ultrapassa esse limite de mapa (e aplica sua const de maxZoom)
        float currentAllowedMaxZoom = Mathf.Min(maxZoom, maxGlobalZoom);
        targetZoom = Mathf.Clamp(targetZoom, minZoom, currentAllowedMaxZoom);

        // Recalcular as extensões pós clamp de zoom de segurança
        camHalfHeight = targetZoom / Mathf.Sin(cameraRotation.x * Mathf.Deg2Rad);
        camHalfWidth = targetZoom * currentAspect;

        float minX = minMapX + camHalfWidth - edgePadding;
        float maxX = maxMapX - camHalfWidth + edgePadding;
        float minZ = minMapZ + camHalfHeight - edgePadding;
        float maxZ = maxMapZ - camHalfHeight + edgePadding;

        // Limita normalmente. Com o math novo, minX < maxX SEMPRE debaixo dos bounds normais do mapa.
        if (minX > maxX) minX = maxX = (minMapX + maxMapX) / 2f;
        if (minZ > maxZ) minZ = maxZ = (minMapZ + maxMapZ) / 2f;

        float clampedX = Mathf.Clamp(targetProjectedPoint.x, minX, maxX);
        float clampedZ = Mathf.Clamp(targetProjectedPoint.z, minZ, maxZ);

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

    void SetupAutoZoom(float calculatedTileSize)
    {
        float logicalTileSize = ResolveLogicalTileSize();
        float minMapX = bounds.bottomLeft.position.x - bounds.tileExtensao;
        float maxMapX = bounds.topRight.position.x + bounds.tileExtensao;
        float minMapZ = bounds.bottomLeft.position.z - bounds.tileExtensao;
        float maxMapZ = bounds.topRight.position.z + bounds.tileExtensao;

        float mapWidth = maxMapX - minMapX + (edgePadding * 2);
        float mapHeight = maxMapZ - minMapZ + (edgePadding * 2);

        float currentAspect = cam.targetTexture != null 
                            ? (float)cam.targetTexture.width / cam.targetTexture.height 
                            : cam.aspect;

        float maxAllowedZoomByWidth = (mapWidth / 2f) / currentAspect;
        float maxAllowedZoomByHeight = (mapHeight / 2f) * Mathf.Sin(cameraRotation.x * Mathf.Deg2Rad);

        // O zoom onde a camera abrange o mapa inteiro
        float globalMapZoom = Mathf.Min(maxAllowedZoomByWidth, maxAllowedZoomByHeight);
        
        maxZoom = globalMapZoom;
        minZoom = 1.5f; // Limite mínimo fixo pra evitar que a conta abaixo travasse tudo por ser muito restrita

        // Determina o centro de enquadramento
        float gridTileSize = ResolveLogicalTileSize();
        Vector3 finalCenter = GetBestSpawnFramingCenter(gridTileSize);
        if (finalCenter == Vector3.zero && (GridMap.Instance == null || GridMap.Instance.phaseMap == null))
        {
            finalCenter = new Vector3((minMapX + maxMapX) / 2f, 0f, (minMapZ + maxMapZ) / 2f);
        }

        float finalCenterX = Mathf.Round(finalCenter.x / gridTileSize) * gridTileSize;
        float finalCenterZ = Mathf.Round(finalCenter.z / gridTileSize) * gridTileSize;
        Vector3 centerPoint = new Vector3(finalCenterX, 0f, finalCenterZ);

        // Obtenção da largura desejada em tiles
        float targetWidthTiles = initialTilesWidthToSee;

        // Se o mapa for menor que a largura definida, foca na largura do mapa
        float mapWidthInTiles = (maxMapX - minMapX) / logicalTileSize;
        if (mapWidthInTiles < targetWidthTiles)
        {
            targetWidthTiles = mapWidthInTiles;
        }

        // Calcula a largura desejada no mundo
        float wHalf = (targetWidthTiles * logicalTileSize) / 2f;

        // Representa a largura horizontal do grid (dois pontos ao longo do eixo X do mundo)
        Vector3 leftPoint = centerPoint - wHalf * Vector3.right;
        Vector3 rightPoint = centerPoint + wHalf * Vector3.right;

        // Projeta os cantos de largura para o espaço local da câmera
        Vector3 localLeft = cam.transform.InverseTransformDirection(leftPoint - centerPoint);
        Vector3 localRight = cam.transform.InverseTransformDirection(rightPoint - centerPoint);

        float widthSpan = Mathf.Abs(localRight.x - localLeft.x);

        // O zoom (orthographic size) necessário para a largura caber é (widthSpan / 2) / aspect
        float defaultInitialZoom = (widthSpan / 2f) / currentAspect;

        // Debug pra entendermos o que rolou no cálculo:
        Debug.Log($"[Camera] Mapa: {mapWidth}x{mapHeight} (lógico {mapWidthInTiles:F1} tiles de largura). Zoom Global Máximo é {maxZoom}. A visão de {targetWidthTiles} tiles de largura (tile lógico={logicalTileSize}) mediu Zoom de {defaultInitialZoom}");

        // Começa com a visão configurada (mas garantido de não ultrapassar os limites do mapa inteiro)
        targetZoom = Mathf.Clamp(defaultInitialZoom, minZoom, maxZoom);
        cam.orthographicSize = targetZoom;

        targetProjectedPoint = centerPoint;
        SnapToTarget();

        // Calcula quantos tiles estão realmente sendo mostrados na tela após o clamp
        float shownWidth = (targetZoom * currentAspect * 2f) / logicalTileSize;
        float shownHeight = ((targetZoom / Mathf.Sin(cameraRotation.x * Mathf.Deg2Rad)) * 2f) / logicalTileSize;
        Debug.Log($"[Camera] Visão Resultante: Mostrando {shownWidth:F1} tiles de largura e {shownHeight:F1} tiles de altura/profundidade na tela.");

        // TESTE VISUAL: Apenas calcula e informa quantos tiles ficam dentro da visão inicial.
        StartCoroutine(HighlightVisibleTilesTest(shownWidth, shownHeight, logicalTileSize));
    }

    float ResolveReferenceTileSize()
    {
        float calculatedTileSize = tileSizeOverride;

        if (referenceTile != null)
        {
            Collider col = referenceTile.GetComponentInChildren<Collider>();
            if (col != null)
                calculatedTileSize = col.bounds.size.x;
            else
            {
                Renderer rend = referenceTile.GetComponentInChildren<Renderer>();
                if (rend != null)
                    calculatedTileSize = rend.bounds.size.x;
            }
        }

        return calculatedTileSize;
    }

    float ResolveLogicalTileSize()
    {
        if (GridMap.Instance != null && GridMap.Instance.tileSize > 0f)
            return GridMap.Instance.tileSize;

        return ResolveReferenceTileSize();
    }

    bool TryGetVisibleWorldRect(out Vector3 camCenter, out float halfWidthReal, out float halfHeightReal)
    {
        camCenter = Vector3.zero;
        halfWidthReal = 0f;
        halfHeightReal = 0f;

        if (cam == null)
            return false;

        Vector3[] screenCorners = new Vector3[] {
            new Vector3(0f, 0f, 0f),
            new Vector3(cam.pixelWidth, 0f, 0f),
            new Vector3(0f, cam.pixelHeight, 0f),
            new Vector3(cam.pixelWidth, cam.pixelHeight, 0f)
        };

        Vector3[] worldCorners = new Vector3[4];
        for (int i = 0; i < 4; i++)
        {
            Ray r = cam.ScreenPointToRay(screenCorners[i]);
            if (mapPlane.Raycast(r, out float d))
                worldCorners[i] = r.GetPoint(d);
            else
                worldCorners[i] = targetProjectedPoint;
        }

        float minX = Mathf.Min(worldCorners[0].x, worldCorners[1].x, worldCorners[2].x, worldCorners[3].x);
        float maxX = Mathf.Max(worldCorners[0].x, worldCorners[1].x, worldCorners[2].x, worldCorners[3].x);
        float minZ = Mathf.Min(worldCorners[0].z, worldCorners[1].z, worldCorners[2].z, worldCorners[3].z);
        float maxZ = Mathf.Max(worldCorners[0].z, worldCorners[1].z, worldCorners[2].z, worldCorners[3].z);

        camCenter = new Vector3((minX + maxX) / 2f, 0f, (minZ + maxZ) / 2f);
        halfWidthReal = (maxX - minX) / 2f;
        halfHeightReal = (maxZ - minZ) / 2f;
        return true;
    }

    System.Collections.IEnumerator HighlightVisibleTilesTest(float shownWidth, float shownHeight, float calculatedTileSize)
    {
        yield return new WaitForSeconds(0.5f); // Espera um pouquinho pro mapa construir o visual todo

        if (!TryGetVisibleWorldRect(out Vector3 camCenter, out float halfWidthReal, out float halfHeightReal))
        {
            Debug.LogWarning("[Camera Visual Test] Não foi possível calcular o retângulo visível da câmera.");
            yield break;
        }

        float tileSizeToUse = ResolveLogicalTileSize();
        if (GridMap.Instance != null)
            tileSizeToUse = GridMap.Instance.tileSize;

        int countedTiles = 0;

        if (GridMap.Instance == null || GridMap.Instance.phaseMap == null || bounds == null || bounds.bottomLeft == null || bounds.topRight == null)
        {
            Debug.LogWarning($"[Camera Visual Test] Faltam dependências para a contagem lógica. GridMap={(GridMap.Instance != null ? GridMap.Instance.name : "null")}, PhaseMap={(GridMap.Instance != null && GridMap.Instance.phaseMap != null ? GridMap.Instance.phaseMap.name : "null")}, CameraBounds={(bounds != null ? bounds.name : "null")}");
            yield break;
        }

        int minGridX = Mathf.RoundToInt(bounds.bottomLeft.position.x / tileSizeToUse);
        int maxGridX = Mathf.RoundToInt(bounds.topRight.position.x / tileSizeToUse);
        int minGridZ = Mathf.RoundToInt(bounds.bottomLeft.position.z / tileSizeToUse);
        int maxGridZ = Mathf.RoundToInt(bounds.topRight.position.z / tileSizeToUse);

        float visibleMinX = camCenter.x - halfWidthReal;
        float visibleMaxX = camCenter.x + halfWidthReal;
        float visibleMinZ = camCenter.z - halfHeightReal;
        float visibleMaxZ = camCenter.z + halfHeightReal;

        for (int z = minGridZ; z <= maxGridZ; z++)
        {
            for (int x = minGridX; x <= maxGridX; x++)
            {
                Vector3 tileCenter = new Vector3(x * tileSizeToUse, 0f, z * tileSizeToUse);
                float tileMinX = tileCenter.x - (tileSizeToUse * 0.5f);
                float tileMaxX = tileCenter.x + (tileSizeToUse * 0.5f);
                float tileMinZ = tileCenter.z - (tileSizeToUse * 0.5f);
                float tileMaxZ = tileCenter.z + (tileSizeToUse * 0.5f);

                bool intersectsX = tileMaxX >= visibleMinX && tileMinX <= visibleMaxX;
                bool intersectsZ = tileMaxZ >= visibleMinZ && tileMinZ <= visibleMaxZ;

                if (!intersectsX || !intersectsZ)
                    continue;

                countedTiles++;
            }
        }

        Debug.Log($"[Camera Visual Test] Centro da Câmera (Foco): {camCenter}. TilePrefab={(referenceTile != null ? referenceTile.name : "tileSizeOverride")}. TileSize={tileSizeToUse}. Área Limite X: {halfWidthReal}, Z: {halfHeightReal}. Tiles lógicos contados={countedTiles}.");
    }

    Vector3 GetBestSpawnFramingCenter(float tileSize)
    {
        if (GridMap.Instance == null || GridMap.Instance.phaseMap == null)
            return Vector3.zero;

        bool hasSpawnTiles = false;
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minZ = float.MaxValue;
        float maxZ = float.MinValue;

        foreach (var tile in GridMap.Instance.GetAllTiles())
        {
            if (tile == null || !tile.IsPlayerSpawnZone)
                continue;

            Vector3 worldPos = GridMap.Instance.GridToWorld(tile.GridPosition);
            float half = tileSize * 0.5f;

            minX = Mathf.Min(minX, worldPos.x - half);
            maxX = Mathf.Max(maxX, worldPos.x + half);
            minZ = Mathf.Min(minZ, worldPos.z - half);
            maxZ = Mathf.Max(maxZ, worldPos.z + half);
            hasSpawnTiles = true;
        }

        if (!hasSpawnTiles)
            return Vector3.zero;

        Vector3 spawnCenter = new Vector3((minX + maxX) / 2f, 0f, (minZ + maxZ) / 2f);
        Debug.Log($"[Camera] Enquadramento de spawn calculado. Center={spawnCenter}, BoundsX=[{minX}, {maxX}], BoundsZ=[{minZ}, {maxZ}]");
        return spawnCenter;
    }

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

    private Coroutine pendingFollowCoroutine;

    public void Follow(Unit unit)
    {
        if (unit == null) return;
        Debug.Log($"[CameraController] Solicitação de seguir unidade: {unit.DisplayName}");

        if (pendingFollowCoroutine != null)
        {
            StopCoroutine(pendingFollowCoroutine);
            pendingFollowCoroutine = null;
        }

        pendingFollowCoroutine = StartCoroutine(CoFollowAfterPopups(unit));
    }

    private IEnumerator CoFollowAfterPopups(Unit unit)
    {
        // Espera todos os popups de dano sumirem antes de focar
        if (DamagePopupManager.Instance != null)
        {
            yield return new WaitUntil(() => !DamagePopupManager.Instance.HasActivePopups);
        }

        followTarget = unit;
        cameraMode = CameraMode.FollowUnit;
        pendingFollowCoroutine = null;

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
    // UTIL
    // =========================

    Vector3 ProjectCameraToPlane(Vector3 camPos)
    {
        Ray ray = new Ray(camPos, transform.forward);
        if (mapPlane.Raycast(ray, out float dist))
        {
            return ray.GetPoint(dist);
        }
        return camPos;
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
