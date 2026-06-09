using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using CelestialCross.UI;

public class CombatUIManager : MonoBehaviour
{
    public static CombatUIManager Instance { get; private set; }

    [SerializeField] private UnitPanelUI unitPanelUI;
    [SerializeField] private ActionBarUI actionBarUI;
    [SerializeField] private CombatForecastUI combatForecastUI;
    [SerializeField] private TurnTimelineUI turnTimelineUI;

    [Header("Turn Display")]
    [SerializeField] private RectTransform turnDisplayContainer;
    [SerializeField] private TextMeshProUGUI currentTurnUnitNameText;
    [SerializeField] private Image turnBackgroundImage;
    [SerializeField] private Color allyColor = Color.cyan;
    [SerializeField] private Color enemyColor = Color.red;

    [Header("Animation Settings")]
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private Vector2 exitOffset = new Vector2(0f, 50f);

    private CanvasGroup displayCanvasGroup;
    private SoftShadow backgroundShadow;
    private Coroutine transitionCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (turnDisplayContainer != null)
        {
            displayCanvasGroup = turnDisplayContainer.GetComponent<CanvasGroup>();
            if (displayCanvasGroup == null)
                displayCanvasGroup = turnDisplayContainer.gameObject.AddComponent<CanvasGroup>();
        }

        if (turnBackgroundImage != null)
            backgroundShadow = turnBackgroundImage.GetComponent<SoftShadow>();
    }

    private void Start()
    {
        TurnManager.OnTurnStarted += HandleTurnStarted;
        TurnManager.OnTurnEnded += HandleTurnEnded;
        TurnManager.OnQueueChanged += HandleQueueChanged;

        UnitHoverDetector.OnHoverStarted += HandleUnitHoverStarted;
        UnitHoverDetector.OnHoverEnded += HandleUnitHoverEnded;

        // Sync initial state if combat already started
        if (TurnManager.Instance != null && TurnManager.Instance.CurrentUnit != null)
        {
            HandleQueueChanged(TurnManager.Instance.GetTurnQueue());
            HandleTurnStarted(TurnManager.Instance.CurrentUnit);
        }
    }

    private Unit currentActiveUnit;

    private void OnDestroy()
    {
        TurnManager.OnTurnStarted -= HandleTurnStarted;
        TurnManager.OnTurnEnded -= HandleTurnEnded;
        TurnManager.OnQueueChanged -= HandleQueueChanged;

        UnitHoverDetector.OnHoverStarted -= HandleUnitHoverStarted;
        UnitHoverDetector.OnHoverEnded -= HandleUnitHoverEnded;
        if (currentActiveUnit != null)
            currentActiveUnit.OnActionChanged -= HandleActionChanged;
    }

    private void HandleTurnEnded()
    {
        if (actionBarUI != null) actionBarUI.ClearButtons();
    }

    private void HandleTurnStarted(Unit unit)
    {
        if (currentActiveUnit != null)
            currentActiveUnit.OnActionChanged -= HandleActionChanged;

        currentActiveUnit = unit;
        
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);
        
        transitionCoroutine = StartCoroutine(TransitionTurnUI(unit));

        if (currentActiveUnit != null)
            currentActiveUnit.OnActionChanged += HandleActionChanged;

        // Update player UI if it's player's turn
        bool isPlayerTurn = unit is Pet || unit.CompareTag("Player");
        
        if (isPlayerTurn)
        {
            if (unitPanelUI != null) unitPanelUI.UpdatePanel(unit);
            if (actionBarUI != null) actionBarUI.GenerateButtons(unit);
        }
        else
        {
            if (unitPanelUI != null) unitPanelUI.UpdatePanel(unit);
            if (actionBarUI != null) actionBarUI.ClearButtons();
            if (combatForecastUI != null) combatForecastUI.Hide();
        }
    }

    private IEnumerator TransitionTurnUI(Unit unit)
    {
        if (turnDisplayContainer == null) yield break;

        Vector2 originalPos = turnDisplayContainer.anchoredPosition;
        Vector2 targetExitPos = originalPos + exitOffset; // Alvo para cima

        // --- SAIDA (Sobe como se tirasse o adesivo) ---
        float elapsed = 0;
        while (elapsed < transitionDuration / 2f)
        {
            float t = elapsed / (transitionDuration / 2f);
            float curve = Mathf.SmoothStep(0, 1, t);
            
            turnDisplayContainer.anchoredPosition = Vector2.Lerp(originalPos, targetExitPos, curve);
            if (displayCanvasGroup != null) displayCanvasGroup.alpha = 1f - curve;
            
            // Sombra aumenta para parecer que está descolando
            if (backgroundShadow != null)
                backgroundShadow.effectDistance = new Vector2(5f + curve * 15f, -5f - curve * 15f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // --- TROCA DE DADOS (Enquanto está invisível no topo) ---
        if (currentTurnUnitNameText != null)
        {
            currentTurnUnitNameText.text = unit != null ? unit.DisplayName : "";
            if (unit != null)
            {
                bool isAlly = unit.Team == Team.Player;
                currentTurnUnitNameText.color = isAlly ? allyColor : enemyColor;
            }
        }

        // --- ENTRADA (Vem de cima para baixo como se colasse um novo) ---
        elapsed = 0;
        while (elapsed < transitionDuration / 2f)
        {
            float t = elapsed / (transitionDuration / 2f);
            float curve = Mathf.SmoothStep(0, 1, t);
            
            // Agora interpolamos do TOPO (targetExitPos) de volta para o ORIGINAL (originalPos)
            turnDisplayContainer.anchoredPosition = Vector2.Lerp(targetExitPos, originalPos, curve);
            if (displayCanvasGroup != null) displayCanvasGroup.alpha = curve;
            
            // Sombra diminui para parecer que está sendo colado de volta
            if (backgroundShadow != null)
                backgroundShadow.effectDistance = new Vector2(20f - curve * 15f, -20f + curve * 15f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        turnDisplayContainer.anchoredPosition = originalPos;
        if (displayCanvasGroup != null) displayCanvasGroup.alpha = 1f;
        if (backgroundShadow != null)
            backgroundShadow.effectDistance = new Vector2(5f, -5f);
    }

    private void HandleActionChanged(IUnitAction newAction)
    {
        if (combatForecastUI != null)
            combatForecastUI.SetAction(newAction);
    }

    private void HandleUnitHoverStarted(Unit hoveredUnit)
    {
        if (unitPanelUI != null && hoveredUnit != null)
            unitPanelUI.UpdatePanel(hoveredUnit);
    }

    private void HandleUnitHoverEnded(Unit hoveredUnit)
    {
        // Ao sair do hover, voltamos a exibir a unidade que está jogando no momento
        if (unitPanelUI != null && currentActiveUnit != null)
            unitPanelUI.UpdatePanel(currentActiveUnit);
    }

    private void HandleQueueChanged(IEnumerable<Unit> queue)
    {
        if (turnTimelineUI != null)
            turnTimelineUI.UpdateTimeline(queue);
    }
}
// refresh
