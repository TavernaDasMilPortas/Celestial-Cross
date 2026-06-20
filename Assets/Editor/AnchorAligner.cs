using UnityEngine;
using UnityEditor;

public class AnchorAligner : MonoBehaviour
{
    [MenuItem("Tools/UI/Fix Anchors to Middle-Center (Keep Position)")]
    public static void FixAnchors()
    {
        // Pega o objeto selecionado na Hierarchy (o usuário deve selecionar o CenterBase)
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("Por favor, selecione o CenterBase (ou outro objeto) na Hierarchy primeiro!");
            return;
        }

        int fixedCount = 0;

        foreach (GameObject obj in selectedObjects)
        {
            RectTransform parentRect = obj.GetComponent<RectTransform>();
            if (parentRect != null)
            {
                // Analisa todos os filhos diretos do objeto selecionado
                foreach (Transform child in parentRect)
                {
                    RectTransform childRect = child.GetComponent<RectTransform>();
                    if (childRect != null)
                    {
                        // Verifica se as âncoras não estão no Middle-Center
                        if (childRect.anchorMin != new Vector2(0.5f, 0.5f) || childRect.anchorMax != new Vector2(0.5f, 0.5f))
                        {
                            // Salva a ação para poder dar Ctrl+Z depois, caso algo dê errado
                            Undo.RecordObject(childRect, "Fix Anchors to Middle-Center");

                            // Salva a posição exata e o tamanho atual na tela antes de mexer nas âncoras
                            Vector3 currentWorldPosition = childRect.position;
                            Vector2 currentSize = childRect.rect.size;

                            // Muda as âncoras para o exato meio (Middle-Center)
                            childRect.anchorMin = new Vector2(0.5f, 0.5f);
                            childRect.anchorMax = new Vector2(0.5f, 0.5f);

                            // Restaura o tamanho original que ele tinha antes de perder o Stretch
                            childRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currentSize.x);
                            childRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, currentSize.y);

                            // Restaura a posição exata na tela
                            childRect.position = currentWorldPosition;
                            
                            // Marca como modificado para a Unity salvar
                            EditorUtility.SetDirty(childRect);
                            fixedCount++;
                        }
                    }
                }
            }
        }

        Debug.Log($"Sucesso! {fixedCount} elementos filhos tiveram suas âncoras arrumadas para o Centro sem sair do lugar!");
    }
}
