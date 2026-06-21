using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using CelestialCross.Dialogue.Graph;
using CelestialCross.Dialogue.Graph.Editor;
using DG.Tweening;
using DG.DOTweenEditor;
using UnityEditor.Experimental.GraphView;

namespace CelestialCross.Dialogue.Editor
{
    public class DialogueCreationWizard : EditorWindow
    {
        public enum WizardStep
        {
            BasicSetup,
            GraphEditor,
            SceneryManager,
            Review
        }

        private WizardStep currentStep = WizardStep.BasicSetup;
        private Vector2 scrollPosition;

        // Step 1
        private string graphName = "NewDialogue";
        private DialogueGraph loadedGraph;
        private DialogueGraph currentEditingGraph;

        // Step 2: Embedded Graph Editor
        private IMGUIContainer wizardStepsContainer;
        private VisualElement graphEditorContainer;
        private DialogueGraphView embeddedGraphView;
        
        // Step 3: Scenery Manager
        private List<DialogueScenery> sceneryList = new List<DialogueScenery>();
        private DialogueScenery currentEditingScenery;
        private DialogueScenery externalSceneryToLoad;
        private Vector2 sceneryScroll;
        
        // Mapping
        private Dictionary<string, string> nodeSceneryMap = new Dictionary<string, string>(); // Node GUID -> Scenery ID

        // Preview system
        private GameObject previewCanvas;
        private GameObject previewRoot;
        private DialogueSceneryController previewController;

        [MenuItem("Celestial Cross/1. Editors/Wizards/Dialogue Creation Wizard")]
        public static void OpenWindow()
        {
            var window = GetWindow<DialogueCreationWizard>("Dialogue Wizard");
            window.minSize = new Vector2(800, 600);
            window.Show();
        }

        private void OnEnable()
        {
            currentStep = WizardStep.BasicSetup;
            rootVisualElement.Clear();

            var topBar = new IMGUIContainer(() => {
                DrawHeaderBar();
                DrawBreadcrumbs();
            });
            rootVisualElement.Add(topBar);

            wizardStepsContainer = new IMGUIContainer(DrawIMGUIWizardContent)
            {
                style = { flexGrow = 1 }
            };
            rootVisualElement.Add(wizardStepsContainer);

            graphEditorContainer = new VisualElement
            {
                style = { flexGrow = 1, display = DisplayStyle.None }
            };
            rootVisualElement.Add(graphEditorContainer);

            BuildEmbeddedGraphEditorUI();
        }

        private void OnDisable()
        {
            DestroyPreviewCanvas();
        }

        private void BuildEmbeddedGraphEditorUI()
        {
            var toolbar = new Toolbar();
            var backButton = new UnityEngine.UIElements.Button(() => SwitchToStep(WizardStep.BasicSetup)) { text = "◀ Ir para Configuração" };
            var nextButton = new UnityEngine.UIElements.Button(() => SwitchToStep(WizardStep.SceneryManager)) { text = "Ir para Cenários ▶" };
            var saveButton = new UnityEngine.UIElements.Button(SaveActiveGraph) { text = "Salvar Grafo" };
            
            toolbar.Add(backButton);
            toolbar.Add(nextButton);
            toolbar.Add(saveButton);
            graphEditorContainer.Add(toolbar);

            embeddedGraphView = new DialogueGraphView()
            {
                name = "Dialogue Graph"
            };
            embeddedGraphView.style.flexGrow = 1;
            
            var blackboard = new Blackboard(embeddedGraphView);
            blackboard.Add(new BlackboardSection { title = "Exposed Variables" });
            blackboard.addItemRequested = _view => { embeddedGraphView.AddPropertyToBlackboard(new ExposedProperty()); };
            blackboard.editTextRequested = (view1, element, newValue) =>
            {
                var oldPropertyName = ((BlackboardField)element).text;
                if (embeddedGraphView.ExposedProperties.Any(x => x.propertyName == newValue)) return;
                var propertyIndex = embeddedGraphView.ExposedProperties.FindIndex(x => x.propertyName == oldPropertyName);
                embeddedGraphView.ExposedProperties[propertyIndex].propertyName = newValue;
                ((BlackboardField)element).text = newValue;
            };
            blackboard.SetPosition(new Rect(10, 30, 200, 300));
            embeddedGraphView.Add(blackboard);
            embeddedGraphView.Blackboard = blackboard;

            graphEditorContainer.Add(embeddedGraphView);
        }

        private void DrawIMGUIWizardContent()
        {
            GUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandWidth(true), GUILayout.Height(position.height - 110));

            switch (currentStep)
            {
                case WizardStep.BasicSetup: DrawBasicSetup(); break;
                case WizardStep.GraphEditor: /* Handled by UI Toolkit */ break;
                case WizardStep.SceneryManager: DrawSceneryManager(); break;
                case WizardStep.Review: DrawReview(); break;
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            DrawFooterBar();
        }

        // ================= HEADER & FOOTER =================

        private void DrawHeaderBar()
        {
            Rect headerRect = GUILayoutUtility.GetRect(position.width, 30);
            EditorGUI.DrawRect(headerRect, new Color(0.15f, 0.15f, 0.18f));
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } };
            GUI.Label(headerRect, "DIALOGUE CREATION WIZARD", headerStyle);
        }

        private void DrawBreadcrumbs()
        {
            Rect barRect = GUILayoutUtility.GetRect(position.width, 25);
            EditorGUI.DrawRect(barRect, new Color(0.1f, 0.1f, 0.12f));

            string[] stepNames = { "1. Basic Setup", "2. Graph Editor", "3. Scenery Manager", "4. Review" };
            float segmentWidth = position.width / stepNames.Length;

            for (int i = 0; i < stepNames.Length; i++)
            {
                Rect segRect = new Rect(barRect.x + i * segmentWidth, barRect.y, segmentWidth, barRect.height);
                bool isActive = (int)currentStep == i;
                if (isActive) EditorGUI.DrawRect(segRect, new Color(0.2f, 0.2f, 0.25f));

                GUIStyle btnStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = isActive ? new Color(0.3f, 0.7f, 1f) : Color.gray }
                };
                
                if (GUI.Button(segRect, stepNames[i], btnStyle))
                {
                    SwitchToStep((WizardStep)i);
                }
            }
        }

        private void SwitchToStep(WizardStep newStep)
        {
            if (newStep == currentStep) return;

            // Sair do passo atual
            if (currentStep == WizardStep.GraphEditor)
            {
                SaveActiveGraph();
                UpdateNodeSceneryMapFromGraph();
                graphEditorContainer.style.display = DisplayStyle.None;
                wizardStepsContainer.style.display = DisplayStyle.Flex;
            }
            else if (currentStep == WizardStep.BasicSetup && newStep > WizardStep.BasicSetup)
            {
                EnsureGraphCreated();
            }

            // Entrar no novo passo
            currentStep = newStep;

            if (currentStep == WizardStep.GraphEditor)
            {
                wizardStepsContainer.style.display = DisplayStyle.None;
                graphEditorContainer.style.display = DisplayStyle.Flex;
                embeddedGraphView.ClearGraph();
                if (currentEditingGraph != null)
                {
                    GraphSaveUtility.GetInstance(embeddedGraphView).LoadGraph(currentEditingGraph);
                }
            }
        }

        private void DrawFooterBar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Height(30));
            if (currentStep > WizardStep.BasicSetup)
            {
                if (GUILayout.Button("◀ Voltar", GUILayout.Width(100))) { 
                    SwitchToStep(currentStep - 1);
                }
            }
            else GUILayout.Space(100);

            GUILayout.FlexibleSpace();

            if (currentStep < WizardStep.Review)
            {
                if (GUILayout.Button("Avançar ▶", GUILayout.Width(100))) 
                { 
                    SwitchToStep(currentStep + 1);
                }
            }
            else GUILayout.Space(100);

            GUILayout.EndHorizontal();
        }

        // ================= STEP 1: BASIC SETUP =================

        private void DrawBasicSetup()
        {
            GUILayout.Label("Configuração Básica", EditorStyles.boldLabel);
            GUILayout.BeginVertical("helpbox");
            
            loadedGraph = (DialogueGraph)EditorGUILayout.ObjectField("Carregar Existente", loadedGraph, typeof(DialogueGraph), false);
            if (GUILayout.Button("Carregar"))
            {
                if (loadedGraph != null)
                {
                    currentEditingGraph = loadedGraph;
                    graphName = loadedGraph.name;
                    LoadSceneryFromGraph();
                    ShowNotification(new GUIContent("Grafo carregado!"));
                }
            }

            GUILayout.Space(10);
            graphName = EditorGUILayout.TextField("Nome do Novo Grafo", graphName);
            GUILayout.Label($"Save path: Assets/Celestial-Cross/Dialogues/{graphName}.asset", EditorStyles.miniLabel);

            GUILayout.EndVertical();
        }

        private void EnsureGraphCreated()
        {
            if (currentEditingGraph == null)
            {
                string folder = "Assets/Celestial-Cross/Dialogues";
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                string path = $"{folder}/{graphName}.asset";
                currentEditingGraph = ScriptableObject.CreateInstance<DialogueGraph>();
                AssetDatabase.CreateAsset(currentEditingGraph, path);
                AssetDatabase.SaveAssets();
            }
        }

        private void LoadSceneryFromGraph()
        {
            sceneryList.Clear();
            nodeSceneryMap.Clear();
            if (currentEditingGraph.sceneryMappings != null)
            {
                foreach (var map in currentEditingGraph.sceneryMappings)
                {
                    if (map.scenery != null && !sceneryList.Contains(map.scenery))
                        sceneryList.Add(map.scenery);
                }
            }
            // Mapeamentos nos nós serão lidos do graph view depois
        }

        private void SaveActiveGraph()
        {
            if (currentEditingGraph != null)
            {
                GraphSaveUtility.GetInstance(embeddedGraphView).SaveGraph(currentEditingGraph);
            }
        }

        // ================= STEP 3: SCENERY MANAGER =================

        private void DrawSceneryManager()
        {
            GUILayout.BeginHorizontal();

            // Esquerda: Lista de Cenários
            GUILayout.BeginVertical("box", GUILayout.Width(250));
            GUILayout.Label("Cenários", EditorStyles.boldLabel);
            
            GUILayout.BeginHorizontal();
            externalSceneryToLoad = (DialogueScenery)EditorGUILayout.ObjectField(externalSceneryToLoad, typeof(DialogueScenery), false);
            if (GUILayout.Button("Add", GUILayout.Width(40)))
            {
                if (externalSceneryToLoad != null && !sceneryList.Contains(externalSceneryToLoad))
                {
                    sceneryList.Add(externalSceneryToLoad);
                    currentEditingScenery = externalSceneryToLoad;
                    externalSceneryToLoad = null;
                }
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("+ Criar Novo Cenário"))
            {
                var newScenery = ScriptableObject.CreateInstance<DialogueScenery>();
                newScenery.sceneryName = "Novo Cenário";
                sceneryList.Add(newScenery);
                currentEditingScenery = newScenery;
            }

            sceneryScroll = GUILayout.BeginScrollView(sceneryScroll);
            foreach (var scn in sceneryList.ToList())
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(scn.sceneryName)) currentEditingScenery = scn;
                if (GUILayout.Button("X", GUILayout.Width(20))) 
                {
                    sceneryList.Remove(scn);
                    if (currentEditingScenery == scn) currentEditingScenery = null;
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            // Direita: Edição do Cenário Selecionado
            GUILayout.BeginVertical();
            if (currentEditingScenery != null)
            {
                DrawSceneryEditor(currentEditingScenery);
            }
            else
            {
                GUILayout.Label("Selecione um cenário para editar.");
                
                GUILayout.Space(20);
                DrawNodeSceneryMapping();
            }
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        private void DrawSceneryEditor(DialogueScenery scenery)
        {
            scenery.sceneryName = EditorGUILayout.TextField("Nome do Cenário", scenery.sceneryName);
            
            // Preview Controls
            GUILayout.BeginHorizontal("box");
            if (GUILayout.Button("▶ Preview Entrada")) PlayPreview(scenery, DialogueSceneryController.SceneryLoadMode.Animated);
            if (GUILayout.Button("▶ Preview Idle")) PlayPreview(scenery, DialogueSceneryController.SceneryLoadMode.IdleOnly);
            if (GUILayout.Button("▶ Preview Saída")) PlayPreviewExit(scenery);
            if (GUILayout.Button("⏹ Parar Preview")) DestroyPreviewCanvas();
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal("box");
            if (GUILayout.Button("🛠 Entrar Modo de Edição Visual")) EnterVisualEditMode(scenery);
            if (GUILayout.Button("💾 Sincronizar Edições da Cena", GUILayout.Width(200))) SyncVisualEdits(scenery);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            if (GUILayout.Button("+ Adicionar Camada"))
            {
                scenery.layers.Add(new SceneryLayer());
            }

            for (int i = 0; i < scenery.layers.Count; i++)
            {
                var layer = scenery.layers[i];
                GUILayout.BeginVertical("helpbox");
                GUILayout.BeginHorizontal();
                layer.layerName = EditorGUILayout.TextField("Layer Name", layer.layerName);
                if (GUILayout.Button("▲", GUILayout.Width(20)) && i > 0)
                {
                    scenery.layers[i] = scenery.layers[i-1];
                    scenery.layers[i-1] = layer;
                }
                if (GUILayout.Button("▼", GUILayout.Width(20)) && i < scenery.layers.Count - 1)
                {
                    scenery.layers[i] = scenery.layers[i+1];
                    scenery.layers[i+1] = layer;
                }
                if (GUILayout.Button("X", GUILayout.Width(20))) { scenery.layers.RemoveAt(i); break; }
                GUILayout.EndHorizontal();

                layer.sprite = (Sprite)EditorGUILayout.ObjectField("Sprite", layer.sprite, typeof(Sprite), false);
                
                // Anim Entrada
                GUILayout.Label("Animação de Entrada", EditorStyles.boldLabel);
                var newEntryType = (SceneryAnimType)EditorGUILayout.EnumPopup("Tipo", layer.entryAnimation.type);
                if (newEntryType != layer.entryAnimation.type)
                {
                    layer.entryAnimation.type = newEntryType;
                    ApplyAnimationPreset(layer.entryAnimation);
                }

                if (layer.entryAnimation.type != SceneryAnimType.None)
                {
                    layer.entryAnimation.duration = EditorGUILayout.FloatField("Duração", layer.entryAnimation.duration);
                    if (layer.entryAnimation.type == SceneryAnimType.Fade) layer.entryAnimation.fadeFrom = EditorGUILayout.FloatField("Fade From", layer.entryAnimation.fadeFrom);
                    if (layer.entryAnimation.type.ToString().StartsWith("Slide")) layer.entryAnimation.moveFrom = EditorGUILayout.Vector2Field("Move From", layer.entryAnimation.moveFrom);
                    if (layer.entryAnimation.type == SceneryAnimType.ScaleIn) layer.entryAnimation.scaleFrom = EditorGUILayout.Vector3Field("Scale From", layer.entryAnimation.scaleFrom);
                }

                // Anim Idle
                GUILayout.Label("Animação Idle", EditorStyles.boldLabel);
                var newIdleType = (SceneryAnimType)EditorGUILayout.EnumPopup("Tipo", layer.idleAnimation.type);
                if (newIdleType != layer.idleAnimation.type)
                {
                    layer.idleAnimation.type = newIdleType;
                    ApplyAnimationPreset(layer.idleAnimation);
                }

                if (layer.idleAnimation.type != SceneryAnimType.None)
                {
                    layer.idleAnimation.floatSpeed = EditorGUILayout.FloatField("Velocidade (duração)", layer.idleAnimation.floatSpeed);
                    if (layer.idleAnimation.type == SceneryAnimType.Float) layer.idleAnimation.floatAmplitude = EditorGUILayout.FloatField("Amplitude", layer.idleAnimation.floatAmplitude);
                    if (layer.idleAnimation.type == SceneryAnimType.Pulse) layer.idleAnimation.pulseScale = EditorGUILayout.FloatField("Scale Multiplier", layer.idleAnimation.pulseScale);
                    if (layer.idleAnimation.type == SceneryAnimType.Sway) layer.idleAnimation.swayAngle = EditorGUILayout.FloatField("Ângulo", layer.idleAnimation.swayAngle);
                }

                GUILayout.EndVertical();
            }
            
            GUILayout.Space(20);
            if (GUILayout.Button("Voltar para Mapeamento")) currentEditingScenery = null;
        }

        private void UpdateNodeSceneryMapFromGraph()
        {
            var nodes = embeddedGraphView.nodes.ToList().Cast<DialogueNode>().Where(n => n.nodeType == NodeType.Speech).ToList();
            foreach (var n in nodes)
            {
                if (!nodeSceneryMap.ContainsKey(n.guid))
                {
                    nodeSceneryMap[n.guid] = n.sceneryId ?? "";
                }
            }
        }

        private void DrawNodeSceneryMapping()
        {
            GUILayout.Label("Mapeamento de Cenários para Nós", EditorStyles.boldLabel);
            
            var nodes = embeddedGraphView.nodes.ToList().Cast<DialogueNode>().Where(n => n.nodeType == NodeType.Speech).ToList();
            if (nodes.Count == 0) GUILayout.Label("Nenhum nó de fala (Speech) encontrado no grafo.");

            string[] sceneryNames = new string[sceneryList.Count + 1];
            sceneryNames[0] = "Manter Atual / Nenhum";
            for (int i = 0; i < sceneryList.Count; i++) sceneryNames[i + 1] = sceneryList[i].sceneryName;

            foreach (var node in nodes)
            {
                GUILayout.BeginHorizontal("box");
                GUILayout.Label($"{node.speakerName}: {node.dialogueText.Substring(0, Mathf.Min(30, node.dialogueText.Length))}...", GUILayout.Width(250));
                
                string currentId = nodeSceneryMap.ContainsKey(node.guid) ? nodeSceneryMap[node.guid] : "";
                int selectedIdx = 0;
                var scn = sceneryList.FirstOrDefault(s => s.sceneryName == currentId); // asset name
                if (scn != null) selectedIdx = sceneryList.IndexOf(scn) + 1;

                int newIdx = EditorGUILayout.Popup(selectedIdx, sceneryNames);
                if (newIdx == 0) nodeSceneryMap[node.guid] = "";
                else nodeSceneryMap[node.guid] = sceneryList[newIdx - 1].sceneryName;

                GUILayout.EndHorizontal();
            }
        }

        private void ApplyAnimationPreset(SceneryAnimation anim)
        {
            anim.duration = 1f;
            anim.ease = DG.Tweening.Ease.OutQuad;
            switch (anim.type)
            {
                case SceneryAnimType.Fade:
                    anim.fadeFrom = 0f;
                    break;
                case SceneryAnimType.SlideLeft:
                    anim.moveFrom = new Vector2(1080, 0);
                    break;
                case SceneryAnimType.SlideRight:
                    anim.moveFrom = new Vector2(-1080, 0);
                    break;
                case SceneryAnimType.SlideUp:
                    anim.moveFrom = new Vector2(0, -1920);
                    break;
                case SceneryAnimType.SlideDown:
                    anim.moveFrom = new Vector2(0, 1920);
                    break;
                case SceneryAnimType.ScaleIn:
                    anim.scaleFrom = Vector3.zero;
                    break;
                case SceneryAnimType.Float:
                    anim.floatAmplitude = 15f;
                    anim.floatSpeed = 2f;
                    break;
                case SceneryAnimType.Pulse:
                    anim.pulseScale = 1.05f;
                    anim.floatSpeed = 2f;
                    break;
                case SceneryAnimType.Sway:
                    anim.swayAngle = 3f;
                    anim.floatSpeed = 3f;
                    break;
                case SceneryAnimType.SlowRotate:
                    anim.floatSpeed = 30f; // 30 segundos para uma volta completa
                    break;
            }
        }

        // ================= PREVIEW SYSTEM =================

        private void SetupPreviewCanvas()
        {
            if (previewCanvas != null) DestroyImmediate(previewCanvas);
            
            previewCanvas = new GameObject("DialoguePreviewCanvas");
            var canvas = previewCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = previewCanvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);

            previewRoot = new GameObject("PreviewRoot");
            previewRoot.transform.SetParent(previewCanvas.transform, false);
            var rt = previewRoot.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero;

            previewController = previewRoot.AddComponent<DialogueSceneryController>();
            SerializedObject so = new SerializedObject(previewController);
            so.FindProperty("sceneryRoot").objectReferenceValue = rt;
            so.ApplyModifiedProperties();
        }

        private void PlayPreview(DialogueScenery scenery, DialogueSceneryController.SceneryLoadMode mode)
        {
            SetupPreviewCanvas();
            DOTweenEditorPreview.Stop();
            previewController.LoadScenery(scenery, mode);
            
            // Collect all tweens on previewRoot children
            var tweens = DOTween.TweensByTarget(previewRoot, true);
            if (tweens == null)
            {
                foreach(Transform child in previewRoot.transform)
                {
                    var childTweens = DOTween.TweensByTarget(child.gameObject, true);
                    var imgTweens = DOTween.TweensByTarget(child.GetComponent<UnityEngine.UI.Image>(), true);
                    var rtTweens = DOTween.TweensByTarget(child.GetComponent<RectTransform>(), true);
                    
                    if (childTweens != null) foreach(var t in childTweens) DOTweenEditorPreview.PrepareTweenForPreview(t);
                    if (imgTweens != null) foreach(var t in imgTweens) DOTweenEditorPreview.PrepareTweenForPreview(t);
                    if (rtTweens != null) foreach(var t in rtTweens) DOTweenEditorPreview.PrepareTweenForPreview(t);
                }
            }
            DOTweenEditorPreview.Start();
        }

        private void PlayPreviewExit(DialogueScenery scenery)
        {
            if (previewCanvas == null || previewController == null) SetupPreviewCanvas();
            DOTweenEditorPreview.Stop();
            previewController.UnloadScenery();
            // same tween collection
            foreach(Transform child in previewRoot.transform)
            {
                var imgTweens = DOTween.TweensByTarget(child.GetComponent<UnityEngine.UI.Image>(), true);
                var rtTweens = DOTween.TweensByTarget(child.GetComponent<RectTransform>(), true);
                if (imgTweens != null) foreach(var t in imgTweens) DOTweenEditorPreview.PrepareTweenForPreview(t);
                if (rtTweens != null) foreach(var t in rtTweens) DOTweenEditorPreview.PrepareTweenForPreview(t);
            }
            DOTweenEditorPreview.Start();
        }

        private void DestroyPreviewCanvas()
        {
            DOTweenEditorPreview.Stop();
            if (previewCanvas != null) DestroyImmediate(previewCanvas);
        }

        private void EnterVisualEditMode(DialogueScenery scenery)
        {
            SetupPreviewCanvas();
            DOTweenEditorPreview.Stop();
            previewController.LoadScenery(scenery, DialogueSceneryController.SceneryLoadMode.Static);
            
            // Ping the first layer to highlight it in the Hierarchy
            if (previewRoot != null && previewRoot.transform.childCount > 0)
            {
                Selection.activeGameObject = previewRoot.transform.GetChild(0).gameObject;
                EditorGUIUtility.PingObject(Selection.activeGameObject);
                ShowNotification(new GUIContent("Edite no Scene View e clique em 'Sincronizar'!"));
            }
        }

        private void SyncVisualEdits(DialogueScenery scenery)
        {
            if (previewRoot == null || scenery == null) return;
            
            for (int i = 0; i < previewRoot.transform.childCount; i++)
            {
                if (i >= scenery.layers.Count) break;
                
                var child = previewRoot.transform.GetChild(i);
                var rt = child.GetComponent<RectTransform>();
                if (rt != null)
                {
                    scenery.layers[i].anchorMin = rt.anchorMin;
                    scenery.layers[i].anchorMax = rt.anchorMax;
                    scenery.layers[i].pivot = rt.pivot;
                    scenery.layers[i].anchoredPosition = rt.anchoredPosition;
                    scenery.layers[i].sizeDelta = rt.sizeDelta;
                    scenery.layers[i].rotation = rt.localRotation.eulerAngles.z;
                    scenery.layers[i].scale = rt.localScale;
                }
            }
            EditorUtility.SetDirty(scenery);
            ShowNotification(new GUIContent("Posições e Escalas sincronizadas e salvas!"));
        }

        // ================= STEP 4: REVIEW =================

        private void DrawReview()
        {
            GUILayout.Label("Revisão e Salvamento", EditorStyles.boldLabel);
            
            GUILayout.BeginVertical("helpbox");
            GUILayout.Label($"Graph: {graphName}");
            GUILayout.Label($"Nodes: {embeddedGraphView?.nodes.ToList().Count ?? 0}");
            GUILayout.Label($"Sceneries: {sceneryList.Count}");
            GUILayout.EndVertical();

            GUILayout.Space(20);
            if (GUILayout.Button("SALVAR TUDO", GUILayout.Height(40)))
            {
                SaveEverything();
            }
        }

        private void SaveEverything()
        {
            string folder = "Assets/Celestial-Cross/Dialogues";
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            // 1. Save Sceneries
            foreach (var scn in sceneryList)
            {
                if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(scn)))
                {
                    string safeName = scn.sceneryName.Replace(" ", "");
                    string path = $"{folder}/{graphName}_Scenery_{safeName}.asset";
                    AssetDatabase.CreateAsset(scn, path);
                }
                else
                {
                    EditorUtility.SetDirty(scn);
                }
            }
            AssetDatabase.SaveAssets();

            // 2. Update Graph mappings
            currentEditingGraph.sceneryMappings.Clear();
            foreach (var scn in sceneryList)
            {
                currentEditingGraph.sceneryMappings.Add(new SceneryMapping { sceneryId = scn.sceneryName, scenery = scn });
            }

            // 3. Update Node Data in GraphView
            var nodes = embeddedGraphView.nodes.ToList().Cast<DialogueNode>().Where(n => n.nodeType == NodeType.Speech).ToList();
            foreach (var node in nodes)
            {
                if (nodeSceneryMap.ContainsKey(node.guid))
                {
                    node.sceneryId = nodeSceneryMap[node.guid];
                    node.mainContainer.Q<TextField>("scenery-field")?.SetValueWithoutNotify(node.sceneryId);
                }
            }

            // 4. Save Graph
            GraphSaveUtility.GetInstance(embeddedGraphView).SaveGraph(currentEditingGraph);
            
            ShowNotification(new GUIContent("Tudo salvo com sucesso!"));
        }
    }
}
