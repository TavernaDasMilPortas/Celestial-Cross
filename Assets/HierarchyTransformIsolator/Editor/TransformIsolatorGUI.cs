using UnityEditor;
using UnityEngine;

namespace TransformIsolator.Editor
{
    [InitializeOnLoad]
    public static class TransformIsolatorGUI
    {
        // -------------------------------------------------------------------
        // CONFIGURAÇÃO
        // Ajuste esse valor para mover o botão mais para a esquerda ou direita.
        // Isso ajuda a alinhar perfeitamente com os botões do Hierarchy Designer.
        // -------------------------------------------------------------------
        private const float X_OFFSET_FROM_RIGHT = 18f;
        private const float BUTTON_WIDTH = 16f;

        private static GUIContent linkedIcon;
        private static GUIContent unlinkedIcon;

        static TransformIsolatorGUI()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyItemGUI;
            EditorApplication.delayCall += LoadIcons;
        }

        private static void LoadIcons()
        {
            // Tenta carregar ícones internos do Unity (Link)
            Texture2D linkedTex = EditorGUIUtility.IconContent("d_Linked").image as Texture2D 
                                  ?? EditorGUIUtility.IconContent("Linked").image as Texture2D;
            Texture2D unlinkedTex = EditorGUIUtility.IconContent("d_Unlinked").image as Texture2D 
                                    ?? EditorGUIUtility.IconContent("Unlinked").image as Texture2D;

            linkedIcon = new GUIContent(linkedTex, "Modo de Aplicação Ligado: Transformações no pai afetam os filhos.");
            unlinkedIcon = new GUIContent(unlinkedTex, "Modo Apenas o Pai (Isolado): Transformações no pai NÃO afetam os filhos.");

            // Fallback para texto se não encontrar ícones
            if (linkedTex == null) linkedIcon.text = "L";
            if (unlinkedTex == null) unlinkedIcon.text = "U";
        }

        private static void OnHierarchyItemGUI(int instanceID, Rect selectionRect)
        {
            GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (go == null) return;

            // Define o rect do botão na extrema direita da Hierarchy
            Rect buttonRect = new Rect(
                selectionRect.xMax - X_OFFSET_FROM_RIGHT,
                selectionRect.y,
                BUTTON_WIDTH,
                selectionRect.height
            );

            bool isIsolated = TransformIsolatorCore.IsIsolated(go);
            
            // Oculta o botão se o mouse não estiver em cima e não estiver isolado (opcional para limpar UI)
            // Mas o usuário pediu "junto aos ícones do HD", então vamos sempre mostrar,
            // ou mostrar no hover/seleção. Para simplificar, mostramos sempre que não há ícones visuais para atrapalhar,
            // ou só quando hover/selecionado.
            
            bool isHovering = selectionRect.Contains(Event.current.mousePosition);
            bool isSelected = Selection.Contains(go);

            // Podemos mostrar sempre se o objeto estiver isolado, ou se estiver passando o mouse
            if (isIsolated || isHovering || isSelected)
            {
                // Remover o fundo do botão para ficar clean como o Hierarchy Designer
                GUIStyle buttonStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    padding = new RectOffset(0, 0, 0, 0)
                };

                GUIContent currentIcon = isIsolated ? unlinkedIcon : linkedIcon;
                
                // Desenhar botão
                if (GUI.Button(buttonRect, currentIcon, buttonStyle))
                {
                    TransformIsolatorCore.SetIsolated(go, !isIsolated);
                    Event.current.Use(); // Consome o clique
                }
            }
        }
    }
}
