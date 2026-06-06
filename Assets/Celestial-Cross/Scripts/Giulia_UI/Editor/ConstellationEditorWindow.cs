using UnityEngine;
using UnityEditor;
using CelestialCross.Data;

namespace CelestialCross.EditorArea
{
    public class ConstellationEditorWindow : EditorWindow
    {
        public enum TargetMode { Constellation, BannerPull }
        public enum ScreenReference { Landscape_1920x1080, Portrait_1080x1920, Custom_Area }
        
        public TargetMode currentMode = TargetMode.Constellation;
        public ScreenReference screenRef = ScreenReference.Portrait_1080x1920;
        public Vector2 customAreaSize = new Vector2(1080, 1920);

        public ConstellationConfigSO targetConfig;
        public BannerPullVisualConfigSO bannerConfig;
        
        private int selectedStarIndex = -1;
        private float zoom = 1.0f;
        private Vector2 scrollPos = Vector2.zero;

        [MenuItem("Celestial Cross/1. Editors/Constellation & Banner Designer")]
        public static void ShowWindow()
        {
            var window = GetWindow<ConstellationEditorWindow>("Star Designer");
            window.minSize = new Vector2(600, 400);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            currentMode = (TargetMode)EditorGUILayout.EnumPopup(currentMode, EditorStyles.toolbarDropDown, GUILayout.Width(120));
            
            if (currentMode == TargetMode.Constellation)
            {
                targetConfig = (ConstellationConfigSO)EditorGUILayout.ObjectField("Config SO", targetConfig, typeof(ConstellationConfigSO), false);
            }
            else
            {
                bannerConfig = (BannerPullVisualConfigSO)EditorGUILayout.ObjectField("Banner SO", bannerConfig, typeof(BannerPullVisualConfigSO), false);
                GUILayout.Space(10);
                screenRef = (ScreenReference)EditorGUILayout.EnumPopup(screenRef, EditorStyles.toolbarDropDown, GUILayout.Width(140));
            }
            
            GUILayout.FlexibleSpace();
            zoom = EditorGUILayout.Slider("Zoom", zoom, 0.1f, 3.0f);
            if (GUILayout.Button("Reset View", EditorStyles.toolbarButton)) zoom = 1.0f;
            EditorGUILayout.EndHorizontal();

            if (currentMode == TargetMode.Constellation && targetConfig == null)
            {
                EditorGUILayout.HelpBox("Selecione um ConstellationConfigSO para começar a editar.", MessageType.Info);
                return;
            }
            else if (currentMode == TargetMode.BannerPull && bannerConfig == null)
            {
                EditorGUILayout.HelpBox("Selecione um BannerPullVisualConfigSO para começar a editar.", MessageType.Info);
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
            
            if (currentMode == TargetMode.BannerPull && screenRef == ScreenReference.Custom_Area)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Tamanho da Área Real (Pixels)", EditorStyles.miniBoldLabel);
                customAreaSize = EditorGUILayout.Vector2Field("", customAreaSize);
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }

            if (currentMode == TargetMode.Constellation)
            {
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
            }
            else // TargetMode.BannerPull
            {
                if (bannerConfig.pullPositions.Count != 10)
                {
                    if (GUILayout.Button("Inicializar 10 Posições"))
                    {
                        Undo.RecordObject(bannerConfig, "Init Banner Positions");
                        bannerConfig.EnsureTenStars();
                        EditorUtility.SetDirty(bannerConfig);
                    }
                }

                for (int i = 0; i < bannerConfig.pullPositions.Count; i++)
                {
                    var pullPos = bannerConfig.pullPositions[i];
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    Rect headerRect = EditorGUILayout.BeginHorizontal();
                    if (selectedStarIndex == i) EditorGUI.DrawRect(headerRect, new Color(1, 0.9f, 0, 0.1f));
                    
                    EditorGUILayout.LabelField($"P{i+1}", GUILayout.Width(30));
                    pullPos.positionName = EditorGUILayout.TextField(pullPos.positionName);
                    
                    if (GUILayout.Button("Ir", GUILayout.Width(30))) selectedStarIndex = i;
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(2);
                }
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

            // Desenhar Linhas Guias de Tela (Mobile Reference)
            if (currentMode == TargetMode.BannerPull)
            {
                Handles.BeginGUI();
                Vector2 screenSize = screenRef == ScreenReference.Landscape_1920x1080 ? new Vector2(1920, 1080) : 
                                     (screenRef == ScreenReference.Portrait_1080x1920 ? new Vector2(1080, 1920) : customAreaSize);
                Vector2 scaledSize = screenSize * zoom;
                
                Rect screenRect = new Rect(center.x - scaledSize.x / 2, center.y - scaledSize.y / 2, scaledSize.x, scaledSize.y);
                
                // Dim down the outside of the screen to focus on the active area
                Handles.DrawSolidRectangleWithOutline(screenRect, new Color(0, 0, 0, 0), new Color(0.5f, 0.8f, 1f, 0.5f));
                
                GUIStyle referenceStyle = new GUIStyle(EditorStyles.boldLabel);
                referenceStyle.normal.textColor = new Color(0.5f, 0.8f, 1f, 0.5f);
                referenceStyle.alignment = TextAnchor.UpperLeft;
                GUI.Label(new Rect(screenRect.x + 5, screenRect.y + 5, 200, 20), $"Screen Bounds ({screenSize.x}x{screenSize.y})", referenceStyle);
                Handles.EndGUI();
            }

            Event e = Event.current;

            // Desenhar Linhas
            Handles.BeginGUI();
            Handles.color = Color.cyan;
            
            if (currentMode == TargetMode.Constellation)
            {
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
                
                if (selectedStarIndex != -1 && e.type == EventType.MouseDrag)
                {
                    Undo.RecordObject(targetConfig, "Move Star");
                    stars[selectedStarIndex].position += new Vector2(e.delta.x, -e.delta.y) / zoom;
                    EditorUtility.SetDirty(targetConfig);
                    e.Use();
                }
            }
            else // TargetMode.BannerPull
            {
                var pullPositions = bannerConfig.pullPositions;
                
                if (bannerConfig.connectionIndices != null && bannerConfig.connectionIndices.Length >= 2)
                {
                    for (int i = 0; i < bannerConfig.connectionIndices.Length; i += 2)
                    {
                        int i1 = bannerConfig.connectionIndices[i];
                        int i2 = bannerConfig.connectionIndices[i+1];
                        if (i1 < pullPositions.Count && i2 < pullPositions.Count)
                        {
                            Vector2 p1 = center + new Vector2(pullPositions[i1].position.x, -pullPositions[i1].position.y) * zoom;
                            Vector2 p2 = center + new Vector2(pullPositions[i2].position.x, -pullPositions[i2].position.y) * zoom;
                            Handles.DrawAAPolyLine(3f, p1, p2);
                        }
                    }
                }

                // Desenhar e Arrastar Posições
                for (int i = 0; i < pullPositions.Count; i++)
                {
                    Vector2 starPos = center + new Vector2(pullPositions[i].position.x, -pullPositions[i].position.y) * zoom;
                    Rect starRect = new Rect(starPos.x - 15, starPos.y - 15, 30, 30);

                    if (area.Contains(starPos))
                    {
                        // Diferenciar cor pra banner (vamos usar azul claro / cyan)
                        Handles.color = (selectedStarIndex == i) ? Color.yellow : Color.cyan;
                        Handles.DrawSolidDisc(starPos, Vector3.forward, 10f * zoom);
                        
                        GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel);
                        labelStyle.normal.textColor = Color.white;
                        GUI.Label(new Rect(starPos.x + 12, starPos.y - 10, 150, 40), $"{pullPositions[i].positionName}\nX:{pullPositions[i].position.x:F0} Y:{pullPositions[i].position.y:F0}", labelStyle);

                        if (e.type == EventType.MouseDown && starRect.Contains(e.mousePosition))
                        {
                            selectedStarIndex = i;
                            e.Use();
                        }
                    }
                }
                
                if (selectedStarIndex != -1 && e.type == EventType.MouseDrag)
                {
                    Undo.RecordObject(bannerConfig, "Move Banner Star");
                    pullPositions[selectedStarIndex].position += new Vector2(e.delta.x, -e.delta.y) / zoom;
                    EditorUtility.SetDirty(bannerConfig);
                    e.Use();
                }
            }
            
            Handles.EndGUI();

            if (e.type == EventType.MouseUp)
            {
                selectedStarIndex = -1;
            }

            GUI.Label(new Rect(10, position.height - 30, 400, 30), "Designer de Estrelas Visuais", EditorStyles.miniBoldLabel);
        }
    }
}

