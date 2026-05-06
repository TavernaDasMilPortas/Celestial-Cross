using UnityEngine;
using UnityEditor;
using CelestialCross.Data;

namespace CelestialCross.EditorArea
{
    public class ConstellationEditorWindow : EditorWindow
    {
        public ConstellationConfigSO targetConfig;
        private int selectedStarIndex = -1;
        private float zoom = 1.0f;
        private Vector2 scrollPos = Vector2.zero;

        [MenuItem("Celestial Cross/Constellation Designer")]
        public static void ShowWindow()
        {
            var window = GetWindow<ConstellationEditorWindow>("Constellation Designer");
            window.minSize = new Vector2(600, 400);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            targetConfig = (ConstellationConfigSO)EditorGUILayout.ObjectField("Config SO", targetConfig, typeof(ConstellationConfigSO), false);
            GUILayout.FlexibleSpace();
            zoom = EditorGUILayout.Slider("Zoom", zoom, 0.1f, 3.0f);
            if (GUILayout.Button("Reset View", EditorStyles.toolbarButton)) zoom = 1.0f;
            EditorGUILayout.EndHorizontal();

            if (targetConfig == null)
            {
                EditorGUILayout.HelpBox("Selecione um ConstellationConfigSO para começar a editar.", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            
            // Área de Edição Gráfica (Lado Esquerdo)
            DrawEditorArea(new Rect(0, 20, position.width - 250, position.height - 20));

            // Painel de Propriedades (Lado Direito)
            DrawPropertiesPanel(new Rect(position.width - 250, 20, 250, position.height - 20));
            
            EditorGUILayout.EndHorizontal();
            
            if (GUI.changed) Repaint();
        }

        private void DrawPropertiesPanel(Rect area)
        {
            GUILayout.BeginArea(area, EditorStyles.helpBox);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            
            EditorGUILayout.LabelField("Configurações das Estrelas", EditorStyles.boldLabel);
            
            if (targetConfig.stars.Count != 6)
            {
                if (GUILayout.Button("Inicializar 6 Estrelas"))
                {
                    Undo.RecordObject(targetConfig, "Init Stars");
                    targetConfig.EnsureSixStars();
                    EditorUtility.SetDirty(targetConfig);
                }
            }

            for (int i = 0; i < targetConfig.stars.Count; i++)
            {
                var star = targetConfig.stars[i];
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                Rect headerRect = EditorGUILayout.BeginHorizontal();
                if (selectedStarIndex == i) EditorGUI.DrawRect(headerRect, new Color(1, 0.9f, 0, 0.1f));
                
                EditorGUILayout.LabelField($"C{i+1}", GUILayout.Width(30));
                star.starName = EditorGUILayout.TextField(star.starName);
                
                if (GUILayout.Button("Ir", GUILayout.Width(30))) selectedStarIndex = i;
                EditorGUILayout.EndHorizontal();

                star.passiveGraph = (Celestial_Cross.Scripts.Abilities.Graph.AbilityGraphSO)EditorGUILayout.ObjectField("Grafo", star.passiveGraph, typeof(Celestial_Cross.Scripts.Abilities.Graph.AbilityGraphSO), false);
                star.customDescription = EditorGUILayout.TextArea(star.customDescription, GUILayout.Height(40));

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawEditorArea(Rect area)
        {
            // Desenhar fundo
            EditorGUI.DrawRect(area, new Color(0.12f, 0.12f, 0.12f, 1f));
            
            Vector2 center = area.center;

            // Desenhar Grade (Grid)
            Handles.BeginGUI();
            Handles.color = new Color(1, 1, 1, 0.05f);
            for (float i = -1000; i <= 1000; i += 50)
            {
                float x = center.x + i * zoom;
                float y = center.y + i * zoom;
                if (x >= area.x && x <= area.xMax) Handles.DrawLine(new Vector2(x, area.y), new Vector2(x, area.yMax));
                if (y >= area.y && y <= area.yMax) Handles.DrawLine(new Vector2(area.x, y), new Vector2(area.xMax, y));
            }
            
            Handles.color = new Color(1, 1, 1, 0.2f);
            Handles.DrawLine(new Vector2(center.x, area.y), new Vector2(center.x, area.yMax));
            Handles.DrawLine(new Vector2(area.x, center.y), new Vector2(area.xMax, center.y));
            Handles.EndGUI();

            Event e = Event.current;

            // Desenhar Linhas
            Handles.BeginGUI();
            Handles.color = Color.cyan;
            var stars = targetConfig.stars;
            
            // Desenhar conexões sequenciais como fallback se não houver custom
            if (targetConfig.connectionIndices == null || targetConfig.connectionIndices.Length < 2)
            {
                for (int i = 0; i < stars.Count - 1; i++)
                {
                    Vector2 p1 = center + new Vector2(stars[i].position.x, -stars[i].position.y) * zoom;
                    Vector2 p2 = center + new Vector2(stars[i+1].position.x, -stars[i+1].position.y) * zoom;
                    Handles.DrawAAPolyLine(3f, p1, p2);
                }
            }
            else
            {
                for (int i = 0; i < targetConfig.connectionIndices.Length; i += 2)
                {
                    int i1 = targetConfig.connectionIndices[i];
                    int i2 = targetConfig.connectionIndices[i+1];
                    if (i1 < stars.Count && i2 < stars.Count)
                    {
                        Vector2 p1 = center + new Vector2(stars[i1].position.x, -stars[i1].position.y) * zoom;
                        Vector2 p2 = center + new Vector2(stars[i2].position.x, -stars[i2].position.y) * zoom;
                        Handles.DrawAAPolyLine(3f, p1, p2);
                    }
                }
            }

            // Desenhar e Arrastar Estrelas
            for (int i = 0; i < stars.Count; i++)
            {
                Vector2 starPos = center + new Vector2(stars[i].position.x, -stars[i].position.y) * zoom;
                Rect starRect = new Rect(starPos.x - 15, starPos.y - 15, 30, 30);

                if (area.Contains(starPos))
                {
                    Handles.color = (selectedStarIndex == i) ? Color.yellow : Color.white;
                    Handles.DrawSolidDisc(starPos, Vector3.forward, 10f * zoom);
                    
                    GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel);
                    labelStyle.normal.textColor = Color.white;
                    GUI.Label(new Rect(starPos.x + 12, starPos.y - 10, 150, 40), $"{stars[i].starName}\nX:{stars[i].position.x:F0} Y:{stars[i].position.y:F0}", labelStyle);

                    if (e.type == EventType.MouseDown && starRect.Contains(e.mousePosition))
                    {
                        selectedStarIndex = i;
                        e.Use();
                    }
                }
            }
            Handles.EndGUI();

            if (selectedStarIndex != -1 && e.type == EventType.MouseDrag)
            {
                Undo.RecordObject(targetConfig, "Move Star");
                stars[selectedStarIndex].position += new Vector2(e.delta.x, -e.delta.y) / zoom;
                EditorUtility.SetDirty(targetConfig);
                e.Use();
            }

            if (e.type == EventType.MouseUp)
            {
                selectedStarIndex = -1;
            }

            GUI.Label(new Rect(10, position.height - 30, 400, 30), "Designer de Constelações v2.0", EditorStyles.miniBoldLabel);
        }
    }
}

