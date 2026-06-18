using UnityEngine;
using Celestial_Cross.Scripts.Units.Enemy;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Gerencia os indicadores visuais (setas) direcionados aos inimigos que estão fora da tela.
/// As setas aparecem de forma responsiva temporariamente no início de cada turno e após 10 segundos de inatividade.
/// Este objeto deve ser filho de um Canvas na cena.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class EnemyPointerManager : MonoBehaviour
{
    public static EnemyPointerManager Instance { get; private set; }

    [Header("Configurações de UI")]
    [Tooltip("Prefab da seta indicadora (deve conter um componente Button e Image).")]
    [SerializeField] private GameObject arrowPrefab;

    [Tooltip("Padding da borda da tela em pixels para posicionar as setas.")]
    [SerializeField] private float screenPadding = 45f;

    [Tooltip("Ângulo de rotação padrão da imagem da seta. Ajuste se o sprite apontar para cima ou para outra direção.")]
    [SerializeField] private float arrowGraphicRotationOffset = 0f;

    [Header("Configurações de Tempo")]
    [Tooltip("Tempo em segundos sem interação para ativar as setas.")]
    [SerializeField] private float inactivityThreshold = 10f;

    [Tooltip("Tempo em segundos que as setas ficam piscando/visíveis na tela.")]
    [SerializeField] private float flashDuration = 3f;

    [Tooltip("Tempo em segundos que a câmera fica focada no inimigo ao clicar na seta.")]
    [SerializeField] private float clickFocusDuration = 1.5f;

    [Tooltip("Nível de zoom da câmera ao focar em um inimigo via clique.")]
    [SerializeField] private float clickFocusZoom = 4.5f;

    private float inactivityTimer = 0f;
    private bool isFlashingActive = false;
    private float currentFlashTimeRemaining = 0f;

    private RectTransform rectTransform;
    private Dictionary<EnemyUnit, GameObject> activePointers = new Dictionary<EnemyUnit, GameObject>();
    private Coroutine activeFocusCoroutine;

    private Canvas parentCanvas;
    private Camera canvasCamera;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"[EnemyPointerManager] Instância duplicada detectada em '{gameObject.name}'. Destruindo.");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        rectTransform = GetComponent<RectTransform>();

        // Busca o Canvas pai para obter a câmera correta (necessário para Screen Space - Camera)
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            canvasCamera = parentCanvas.worldCamera;
            Debug.Log($"[EnemyPointerManager] Awake OK. Canvas='{parentCanvas.name}', RenderMode={parentCanvas.renderMode}, Camera={(canvasCamera != null ? canvasCamera.name : "null")}, RectSize={rectTransform.rect.size}");
        }
        else
        {
            Debug.LogError("[EnemyPointerManager] ERRO: Nenhum Canvas pai encontrado! As setas NÃO serão renderizadas.");
        }

        if (arrowPrefab == null)
        {
            Debug.LogError("[EnemyPointerManager] ERRO: arrowPrefab não atribuído no Inspector! As setas NÃO serão criadas.");
        }
        else
        {
            Debug.Log($"[EnemyPointerManager] arrowPrefab configurado: '{arrowPrefab.name}'");
        }
    }

    private void OnEnable()
    {
        TurnManager.OnTurnStarted += HandleTurnStarted;
        Debug.Log("[EnemyPointerManager] OnEnable: inscrito no TurnManager.OnTurnStarted.");
    }

    private void OnDisable()
    {
        TurnManager.OnTurnStarted -= HandleTurnStarted;
        Debug.Log("[EnemyPointerManager] OnDisable: desinscrito do TurnManager.OnTurnStarted.");
    }

    private void Update()
    {
        // Monitora interações para resetar o timer de inatividade
        DetectUserActivity();

        // Gerenciamento do estado temporário de exibição (flash)
        if (isFlashingActive)
        {
            currentFlashTimeRemaining -= Time.deltaTime;
            if (currentFlashTimeRemaining <= 0f)
            {
                StopFlashing();
            }
        }
    }

    private void LateUpdate()
    {
        // Só atualiza os indicadores se estiver na duração ativa do piscar (flash)
        if (isFlashingActive)
        {
            UpdatePointers();
        }
        else
        {
            HideAllPointers();
        }
    }

    /// <summary>
    /// Detecta ações do jogador (toque, clique ou arrasto de câmera) e reseta o timer.
    /// </summary>
    private void DetectUserActivity()
    {
        bool hasInput = Input.anyKey || 
                        Input.GetMouseButton(0) || 
                        Input.GetMouseButton(1) || 
                        Input.GetMouseButton(2) || 
                        Input.touchCount > 0;

        if (hasInput || (CameraController.Instance != null && CameraController.Instance.IsDragging))
        {
            inactivityTimer = 0f;
        }
        else
        {
            inactivityTimer += Time.deltaTime;
            if (inactivityTimer >= inactivityThreshold)
            {
                inactivityTimer = 0f;
                StartFlashing();
            }
        }
    }

    /// <summary>
    /// Evento disparado pelo TurnManager no início de cada turno.
    /// </summary>
    private void HandleTurnStarted(Unit activeUnit)
    {
        Debug.Log($"[EnemyPointerManager] HandleTurnStarted disparado! Unidade ativa: {(activeUnit != null ? activeUnit.DisplayName : "null")}");
        inactivityTimer = 0f;
        StartFlashing();
    }

    /// <summary>
    /// Inicia o período de exibição piscante das setas.
    /// </summary>
    public void StartFlashing()
    {
        Debug.Log($"[EnemyPointerManager] StartFlashing chamado. flashDuration={flashDuration}s");
        isFlashingActive = true;
        currentFlashTimeRemaining = flashDuration;
    }

    /// <summary>
    /// Finaliza o período de exibição e remove as setas visíveis.
    /// </summary>
    public void StopFlashing()
    {
        isFlashingActive = false;
        HideAllPointers();
    }

    /// <summary>
    /// Atualiza as posições e rotações das setas apontando para inimigos vivos fora da tela.
    /// </summary>
    private void UpdatePointers()
    {
        if (Camera.main == null)
        {
            Debug.LogWarning("[EnemyPointerManager] UpdatePointers: Camera.main é null!");
            return;
        }
        if (arrowPrefab == null)
        {
            Debug.LogWarning("[EnemyPointerManager] UpdatePointers: arrowPrefab é null!");
            return;
        }

        // Coleta todos os inimigos vivos na cena
        EnemyUnit[] currentEnemies = Object.FindObjectsByType<EnemyUnit>(FindObjectsSortMode.None);

        if (currentEnemies.Length == 0)
        {
            // É normal não haver inimigos na cena dependendo da fase ou do fluxo (ex: todos morreram)
            return;
        }

        // Remove indicadores de inimigos que morreram ou foram destruídos
        var deadEnemies = activePointers.Keys.Where(e => e == null || !e.gameObject.activeInHierarchy || !currentEnemies.Contains(e)).ToList();
        foreach (var dead in deadEnemies)
        {
            if (activePointers.TryGetValue(dead, out GameObject arrow))
            {
                Destroy(arrow);
            }
            activePointers.Remove(dead);
        }

        Vector2 centerCamera = new Vector2(Camera.main.pixelWidth * 0.5f, Camera.main.pixelHeight * 0.5f);
        Rect rect = rectTransform.rect;

        foreach (var enemy in currentEnemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;

            Vector3 worldPos = enemy.transform.position;
            Vector3 camPixelPos = Camera.main.WorldToScreenPoint(worldPos);

            // Verifica se o inimigo está dentro dos limites da visão real da câmera
            bool isOnScreen = camPixelPos.z > 0 &&
                              camPixelPos.x >= 0 && camPixelPos.x <= Camera.main.pixelWidth &&
                              camPixelPos.y >= 0 && camPixelPos.y <= Camera.main.pixelHeight;

            if (isOnScreen)
            {
                // Inimigo visível na tela: oculta a seta correspondente
                if (activePointers.TryGetValue(enemy, out GameObject arrow))
                {
                    arrow.SetActive(false);
                }
            }
            else
            {
                // Inimigo fora da tela: cria ou ativa a seta indicadora
                if (!activePointers.TryGetValue(enemy, out GameObject arrow) || arrow == null)
                {
                    arrow = Instantiate(arrowPrefab, transform);
                    
                    // Adiciona o ouvinte de clique
                    Button button = arrow.GetComponent<Button>();
                    if (button != null)
                    {
                        EnemyUnit localEnemy = enemy; // Escopo de fechamento
                        button.onClick.AddListener(() => OnPointerClicked(localEnemy));
                    }

                    activePointers[enemy] = arrow;
                }

                arrow.SetActive(true);

                // Se o inimigo está atrás da câmera, inverte a projeção para apontar corretamente
                Vector2 projPos = new Vector2(camPixelPos.x, camPixelPos.y);
                if (camPixelPos.z < 0)
                {
                    projPos *= -1f;
                }

                // Vetor direção a partir do centro da câmera
                Vector2 dir = (projPos - centerCamera).normalized;

                // Margens e limites com padding baseados no Rect local (suporta canvas dinâmicos/RTs)
                float xMin = rect.xMin + screenPadding;
                float xMax = rect.xMax - screenPadding;
                float yMin = rect.yMin + screenPadding;
                float yMax = rect.yMax - screenPadding;

                // Equação de interseção com as bordas do Rect
                float tX = float.MaxValue;
                if (dir.x > 0) tX = (xMax - rect.center.x) / dir.x;
                else if (dir.x < 0) tX = (xMin - rect.center.x) / dir.x;

                float tY = float.MaxValue;
                if (dir.y > 0) tY = (yMax - rect.center.y) / dir.y;
                else if (dir.y < 0) tY = (yMin - rect.center.y) / dir.y;

                float t = Mathf.Min(tX, tY);
                Vector2 localPoint = rect.center + dir * t;

                // Aplica posição
                RectTransform arrowRect = arrow.GetComponent<RectTransform>();
                if (arrowRect != null)
                {
                    arrowRect.anchoredPosition = localPoint;

                    // Rotaciona a seta para apontar para o inimigo
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    arrowRect.localRotation = Quaternion.Euler(0f, 0f, angle + arrowGraphicRotationOffset);
                }

                // Efeito visual de pulso (piscar)
                Image image = arrow.GetComponent<Image>();
                if (image != null)
                {
                    // Onda senoidal para alternar a opacidade
                    float alpha = 0.5f + Mathf.PingPong(Time.time * 5f, 0.5f);
                    Color c = image.color;
                    c.a = alpha;
                    image.color = c;
                }
            }
        }
    }

    /// <summary>
    /// Oculta os indicadores ativos sem destruí-los.
    /// </summary>
    private void HideAllPointers()
    {
        foreach (var pair in activePointers)
        {
            if (pair.Value != null)
            {
                pair.Value.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Lógica chamada ao clicar em um ponteiro/seta.
    /// </summary>
    private void OnPointerClicked(EnemyUnit enemy)
    {
        if (enemy == null || CameraController.Instance == null) return;

        if (activeFocusCoroutine != null)
        {
            StopCoroutine(activeFocusCoroutine);
        }

        activeFocusCoroutine = StartCoroutine(CoFocusOnEnemy(enemy));
    }

    /// <summary>
    /// Corrotina que desloca a câmera até o inimigo clicado e depois retorna.
    /// </summary>
    private IEnumerator CoFocusOnEnemy(EnemyUnit enemy)
    {
        CameraController camCtrl = CameraController.Instance;
        
        // Armazena o estado original da câmera
        Vector3 startProjectedPoint = camCtrl.TargetProjectedPoint;
        float startZoom = camCtrl.TargetZoom;
        CameraController.CameraMode originalMode = camCtrl.cameraMode;

        // Força câmera livre durante a animação
        camCtrl.EnableFreeCamera(true);

        // Desloca e aplica zoom no inimigo
        camCtrl.TargetProjectedPoint = enemy.transform.position;
        camCtrl.TargetZoom = clickFocusZoom;

        // Tempo de espera no foco (pode ser interrompido por arrasto de tela)
        float elapsed = 0f;
        while (elapsed < clickFocusDuration)
        {
            if (camCtrl.IsDragging)
            {
                Debug.Log("[EnemyPointerManager] Foco no inimigo interrompido por arrasto de tela do jogador.");
                activeFocusCoroutine = null;
                yield break;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Retorna suavemente ao ponto original
        camCtrl.TargetProjectedPoint = startProjectedPoint;
        camCtrl.TargetZoom = startZoom;

        // Espera o tempo de transição de retorno acabar
        yield return new WaitForSeconds(0.5f);

        // Restaura o modo de câmera original caso o jogador não tenha arrastado durante a volta
        if (!camCtrl.IsDragging)
        {
            camCtrl.EnableFreeCamera(originalMode == CameraController.CameraMode.Free);
        }

        activeFocusCoroutine = null;
    }
}
