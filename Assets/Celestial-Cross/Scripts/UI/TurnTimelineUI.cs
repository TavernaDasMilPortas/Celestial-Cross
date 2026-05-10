using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

public class TurnTimelineUI : MonoBehaviour
{
    [Header("Timeline Setup")]
    [SerializeField] public GameObject portraitPrefab;
    [SerializeField] public Transform container;

    [Header("Drawer Settings (Gaveta)")]
    [Tooltip("O painel inteiro da Timeline que vai se mover")]
    [SerializeField] public RectTransform drawerPanel;
    [Tooltip("O botão usado para puxar ou empurrar a gaveta")]
    [SerializeField] public Button toggleDrawerButton;
    [Tooltip("Um RectTransform vazio (ou ponto) marcando onde é a posição ABERTA da timeline na tela")]
    [SerializeField] public RectTransform openPositionRef;
    [Tooltip("Um RectTransform vazio (ou ponto) marcando onde é a posição FECHADA da timeline")]
    [SerializeField] public RectTransform closedPositionRef;
    [SerializeField] public float slideSpeed = 15f;

    private bool isDrawerOpen = true;
    private bool isDragging = false;
    private List<GameObject> activePortraits = new();

    private void Awake()
    {
        TurnManager.OnRoundStarted += HandleRoundStarted;

        // Começa escondido
        if (drawerPanel != null)
        {
            drawerPanel.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        TurnManager.OnRoundStarted -= HandleRoundStarted;
    }

    private void Start()
    {
        if (toggleDrawerButton != null)
        {
            // Remove o botão para que ele não capture eventos de clique que podem travar o drag
            // No entanto, mantemos ele como objeto visual ou trocamos por uma imagem se necessário.
            // Para garantir o drag, vamos adicionar o EventTrigger ao toggleDrawerButton.
            
            EventTrigger trigger = toggleDrawerButton.gameObject.GetComponent<EventTrigger>();
            if (trigger == null) trigger = toggleDrawerButton.gameObject.AddComponent<EventTrigger>();
            
            // Limpa triggers antigos para evitar duplicatas manuais
            trigger.triggers.Clear();

            var beginDragEntry = new EventTrigger.Entry { eventID = EventTriggerType.BeginDrag };
            beginDragEntry.callback.AddListener((data) => { OnBeginDragDrawer((PointerEventData)data); });
            trigger.triggers.Add(beginDragEntry);

            var dragEntry = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
            dragEntry.callback.AddListener((data) => { OnDragDrawer((PointerEventData)data); });
            trigger.triggers.Add(dragEntry);

            var endDragEntry = new EventTrigger.Entry { eventID = EventTriggerType.EndDrag };
            endDragEntry.callback.AddListener((data) => { OnEndDragDrawer((PointerEventData)data); });
            trigger.triggers.Add(endDragEntry);

            // Importante: Buttons podem bloquear o drag se o clique for detectado primeiro.
            // Para permitir o drag sem o clique atrapalhar, vamos habilitar o botão
            // mas garantir que ele aceite o início do drag.
            toggleDrawerButton.enabled = true; 
        }
    }

    private void HandleRoundStarted(int round)
    {
        if (round == 1 && drawerPanel != null)
        {
            drawerPanel.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        if (isDragging) return;

        if (drawerPanel != null && openPositionRef != null && closedPositionRef != null)
        {
            Vector3 targetPos = isDrawerOpen ? openPositionRef.position : closedPositionRef.position;
            drawerPanel.position = Vector3.Lerp(drawerPanel.position, targetPos, Time.deltaTime * slideSpeed);
        }
    }

    public void ToggleDrawer()
    {
        if (!isDragging) isDrawerOpen = !isDrawerOpen;
    }

    public void OnBeginDragDrawer(PointerEventData data)
    {
        isDragging = true;
    }

    public void OnDragDrawer(PointerEventData data)
    {
        if (drawerPanel == null || openPositionRef == null || closedPositionRef == null) return;
        
        RectTransform canvasRect = drawerPanel.parent as RectTransform;
        if (canvasRect != null && RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, data.position, data.pressEventCamera, out Vector3 worldPoint))
        {
            Vector3 startPos = closedPositionRef.position;
            Vector3 endPos = openPositionRef.position;
            Vector3 dir = (endPos - startPos).normalized;
            
            // Projeta a posição do mouse na linha entre Fechado e Aberto
            Vector3 projectedPos = startPos + Vector3.Project(worldPoint - startPos, dir);
            
            float totalDist = Vector3.Distance(startPos, endPos);
            float currentDistFromStart = Vector3.Dot(projectedPos - startPos, dir);
            
            // Limita a posição para não sair do range [startPos, endPos]
            if (currentDistFromStart < 0) projectedPos = startPos;
            else if (currentDistFromStart > totalDist) projectedPos = endPos;
            
            drawerPanel.position = projectedPos;

            // Atualiza isDrawerOpen em tempo real para o Update não lutar contra o drag
            float distToOpen = Vector3.Distance(drawerPanel.position, openPositionRef.position);
            float distToClosed = Vector3.Distance(drawerPanel.position, closedPositionRef.position);
            isDrawerOpen = distToOpen <= distToClosed;
        }
    }

    public void OnEndDragDrawer(PointerEventData data)
    {
        isDragging = false;
        if (drawerPanel == null || openPositionRef == null || closedPositionRef == null) return;

        float distToOpen = Vector3.Distance(drawerPanel.position, openPositionRef.position);
        float distToClosed = Vector3.Distance(drawerPanel.position, closedPositionRef.position);

        isDrawerOpen = distToOpen <= distToClosed;
    }

    public void UpdateTimeline(IEnumerable<Unit> turnQueue)
    {
        ClearTimeline();

        if (turnQueue == null || portraitPrefab == null || container == null)
            return;

        int turnIndex = 1;
        foreach (var unit in turnQueue)
        {
            GameObject portraitObj = Instantiate(portraitPrefab, container);
            TurnPortraitUI pUI = portraitObj.GetComponent<TurnPortraitUI>();
            if (pUI != null) 
            {
                pUI.Setup(unit, turnIndex);
                turnIndex++;
            }
            
            activePortraits.Add(portraitObj);
        }
    }

    private void ClearTimeline()
    {
        foreach (var p in activePortraits)
            if (p != null) Destroy(p);
        activePortraits.Clear();
    }
}
