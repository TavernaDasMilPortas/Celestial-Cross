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
        // O Hierarchy tem "slots" da direita para a esquerda.
        // Cada ícone padrão ocupa cerca de 18 pixels de largura.
        // Coluna 0 = Extrema Direita (Geralmente o Olho)
        // Coluna 1 = Ao lado do Olho (Geralmente o Cadeado)
        // Coluna 2 = Terceiro espaço
        // Coluna 3 = Quarto espaço... etc.
        private const int COLUMN_INDEX = 2;
        private const float X_OFFSET_FROM_RIGHT = COLUMN_INDEX * 18f;
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

            // Em vez de alinhar pela direita (que conflita com ícones de componentes variáveis do Hierarchy Designer),
            // vamos calcular a largura do nome do GameObject e desenhar o ícone logo após o texto!
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            float textWidth = labelStyle.CalcSize(new GUIContent(go.name)).x;
            
            // selectionRect.x já contém a indentação correta da árvore.
            // O Unity adiciona um ícone de ~16px antes do texto, mais uns ~4px de margem.
            float startX = selectionRect.x + textWidth + 24f;

            Rect buttonRect = new Rect(
                startX,
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
