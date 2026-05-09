using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TimelineDrawerBuilder
{
    [MenuItem("Celestial Cross/UI/Setup Drawer (Timeline)")]
    public static void SetupTimelineDrawer()
    {
        // 1. Procurar pela TurnTimelineUI na cena ou criar uma
        TurnTimelineUI timelineUI = GameObject.FindObjectOfType<TurnTimelineUI>();
        
        if (timelineUI == null)
        {
            Debug.LogWarning("TurnTimelineUI não encontrado. Selecione o objeto alvo ou ele não existe na cena.");
            return;
        }

        // Registrar no Undo para poder dar Ctrl+Z
        Undo.RecordObject(timelineUI, "Setup Timeline Drawer");

        // 2. Configurar o Drawer Panel
        if (timelineUI.drawerPanel == null)
        {
            timelineUI.drawerPanel = timelineUI.GetComponent<RectTransform>();
            if (timelineUI.drawerPanel == null)
            {
                Debug.LogWarning("TurnTimelineUI precisa estar em um objeto com RectTransform (UI)!");
                return;
            }
        }

        // 3. Procurar ou Criar o Botão (Gaveta)
        if (timelineUI.toggleDrawerButton == null)
        {
            // Busca por um botão nos filhos
            Button btn = timelineUI.GetComponentInChildren<Button>();
            if (btn != null)
            {
                timelineUI.toggleDrawerButton = btn;
            }
            else
            {
                // Cria um novo Image vazio para agir de botão se não tiver nenhum
                GameObject btnObj = new GameObject("ToggleDrawerButton");
                Undo.RegisterCreatedObjectUndo(btnObj, "Create ToggleDrawerButton");
                btnObj.transform.SetParent(timelineUI.drawerPanel, false);
                
                RectTransform btnRect = btnObj.AddComponent<RectTransform>();
                btnRect.anchorMin = new Vector2(0, 0.5f);
                btnRect.anchorMax = new Vector2(0, 0.5f);
                btnRect.pivot = new Vector2(1, 0.5f);
                btnRect.sizeDelta = new Vector2(50, 100);
                btnRect.anchoredPosition = new Vector2(0, 0); // No canto

                Image img = btnObj.AddComponent<Image>();
                img.color = new Color(0, 0, 0, 0.5f);
                
                Button newBtn = btnObj.AddComponent<Button>();
                timelineUI.toggleDrawerButton = newBtn;
            }
        }

        // 4. Configurar Referências de Posição (Âncoras Fantasmas)
        // Devem ficar no PARENT do Drawer para poder comparar a posição independentemente
        Transform parentTransform = timelineUI.drawerPanel.parent;
        
        if (timelineUI.openPositionRef == null)
        {
            timelineUI.openPositionRef = CreateAnchor(parentTransform, "Drawer_OpenPos", timelineUI.drawerPanel.anchoredPosition);
        }

        if (timelineUI.closedPositionRef == null)
        {
            // Cria a posição fechada "escondida" (exemplo: deslocada 300 pixels para a direita)
            timelineUI.closedPositionRef = CreateAnchor(parentTransform, "Drawer_ClosedPos", timelineUI.drawerPanel.anchoredPosition + new Vector2(300, 0));
        }

        // Salva as alterações no Editor
        EditorUtility.SetDirty(timelineUI);
        Debug.Log("Timeline Drawer configurado com sucesso! Verifique as âncoras fantasmas criadas se precisar ajustar o limite do drag.");
    }

    private static RectTransform CreateAnchor(Transform parent, string name, Vector2 anchoredPosition)
    {
        // Se a gente achar pelo nome a gente reutiliza
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            return existing.GetComponent<RectTransform>();
        }

        GameObject anchorObj = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(anchorObj, "Create " + name);
        anchorObj.transform.SetParent(parent, false);

        RectTransform rect = anchorObj.AddComponent<RectTransform>();
        rect.anchoredPosition = anchoredPosition;

        // Opcional: Adicionar um gizmo para enxergar no Scene View?
        // Mas como é um RectTransform, o Scene View do Unity já desenha a bolinha azul
        
        return rect;
    }
}
