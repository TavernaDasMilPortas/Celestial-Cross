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

    private void Start()
    {
        if (toggleDrawerButton != null)
        {
            toggleDrawerButton.onClick.AddListener(ToggleDrawer);

            EventTrigger trigger = toggleDrawerButton.gameObject.AddComponent<EventTrigger>();
            
            var beginDragEntry = new EventTrigger.Entry { eventID = EventTriggerType.BeginDrag };
            beginDragEntry.callback.AddListener((data) => { OnBeginDragDrawer((PointerEventData)data); });
            trigger.triggers.Add(beginDragEntry);

            var dragEntry = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
            dragEntry.callback.AddListener((data) => { OnDragDrawer((PointerEventData)data); });
            trigger.triggers.Add(dragEntry);

            var endDragEntry = new EventTrigger.Entry { eventID = EventTriggerType.EndDrag };
            endDragEntry.callback.AddListener((data) => { OnEndDragDrawer((PointerEventData)data); });
            trigger.triggers.Add(endDragEntry);
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
            
            Vector3 projectedPos = startPos + Vector3.Project(worldPoint - startPos, dir);
            
            float totalDist = Vector3.Distance(startPos, endPos);
            float projDistFromStart = Vector3.Distance(startPos, projectedPos);
            float projDistFromEnd = Vector3.Distance(endPos, projectedPos);
            
            if (projDistFromStart > totalDist) projectedPos = endPos;
            else if (projDistFromEnd > totalDist) projectedPos = startPos;
            else if (Vector3.Dot(dir, projectedPos - startPos) < 0) projectedPos = startPos;
            
            drawerPanel.position = projectedPos;
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
