#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Celestial_Cross.Scripts.Abilities.Graph;
using Celestial_Cross.Scripts.Abilities.Graph.Editor;
using Celestial_Cross.Scripts.Units.Enemy;
using Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree;
using Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Editor;
using CelestialCross.Data;

namespace CelestialCross.Editor
{
    public class EnemyCreationWizard : EditorWindow
    {
        public enum WizardStep
        {
            Identity,
            Stats,
            Abilities,
            Visuals,
            Review
        }

        public enum WizardViewMode
        {
            WizardSteps,
            GraphEditor,
            BTGraphEditor
        }

        // --- Wizard State ---
        private WizardStep currentStep = WizardStep.Identity;
        private WizardViewMode currentViewMode = WizardViewMode.WizardSteps;
        private Vector2 scrollPosition;

        // Existing Unit Modification
        private UnitData loadedUnitData;

        // Step 1: Identity
        private string unitName = "NewEnemy";
        private string displayName = "New Enemy";
        private UnitRole role = UnitRole.Attacker;
        private UnitClass unitClass = UnitClass.Warrior;
        private Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.BehaviorTreeSO defaultBehaviorTree;

        // Step 2: Stats (Enemies only have flat stats, no leveling scaling difference)
        private CombatStats enemyStats = new CombatStats(50, 15, 5, 10, 5, 0, 50, 0);
        private int maxAP = 1;

        // Step 3: Abilities
        private List<AbilityGraphSO> associatedGraphs = new List<AbilityGraphSO>();
        private AbilityGraphSO graphDetailsToEdit; // Inline ability editing reference
        
        // Form fields to add new Ability Graphs
        private string newAbilityName = "New Ability";
        private string newAbilityDescription = "Ability description here...";
        private Sprite newAbilityIcon = null;
        private int newAbilityRange = 1;
        private AbilityType newAbilityType = AbilityType.Active;



        // Step 4: Visuals
        private Sprite unitIcon;
        private Sprite unitSprite;
        private AnimationClip idleAnim;
        private AnimationClip combatIdleAnim;

        // --- UI Toolkit Containers (Hybrid Model) ---
        private IMGUIContainer wizardStepsContainer;
        private VisualElement graphEditorContainer;
        private AbilityGraphView embeddedGraphView;
        private AbilityGraphSO currentEditingGraph;
        private Label graphEditorTitleLabel;

        private VisualElement btGraphEditorContainer;
        private BTGraphView embeddedBTGraphView;
        private BehaviorTreeSO currentEditingBTGraph;
        private Label btGraphEditorTitleLabel;

        // --- Styles & UI Cache ---
        private GUIStyle headerStyle;
        private GUIStyle stepHeaderStyle;
        private GUIStyle footerStyle;
        private GUIStyle cardStyle;
        private GUIStyle subtitleStyle;
        private GUIStyle richTextStyle;

        [MenuItem("Celestial Cross/1. Editors/Wizards/Enemy Creation Wizard")]
        public static void OpenWindow()
        {
            var window = GetWindow<EnemyCreationWizard>("Enemy Wizard");
            window.minSize = new Vector2(600, 650);
            window.Show();
        }

        private void OnEnable()
        {
            // Reset state
            currentStep = WizardStep.Identity;
            currentViewMode = WizardViewMode.WizardSteps;

            // Clear visual tree
            rootVisualElement.Clear();

            // 1. Create IMGUI container for the wizard steps
            wizardStepsContainer = new IMGUIContainer(DrawIMGUIWizardContent)
            {
                style = { flexGrow = 1 }
            };
            rootVisualElement.Add(wizardStepsContainer);

            // 2. Prepare Graph Editor container (hidden initially)
            graphEditorContainer = new VisualElement
            {
                style = { flexGrow = 1, display = DisplayStyle.None }
            };
            rootVisualElement.Add(graphEditorContainer);

            // 3. Prepare BT Graph Editor container (hidden initially)
            btGraphEditorContainer = new VisualElement
            {
                style = { flexGrow = 1, display = DisplayStyle.None }
            };
            rootVisualElement.Add(btGraphEditorContainer);

            BuildEmbeddedGraphEditorUI();
            BuildEmbeddedBTGraphEditorUI();
        }

        private void BuildEmbeddedGraphEditorUI()
        {
            // Toolbar
            var toolbar = new Toolbar();
            
            var backButton = new Button(ExitGraphEditorMode) { text = "◀ Voltar para o Wizard" };
            var saveButton = new Button(SaveActiveGraph) { text = "Salvar Grafo" };
            
            graphEditorTitleLabel = new Label(" Editando Habilidade: None")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, marginLeft = 12, marginRight = 12, unityTextAlign = TextAnchor.MiddleLeft }
            };

            toolbar.Add(backButton);
            toolbar.Add(saveButton);
            toolbar.Add(graphEditorTitleLabel);
            
            graphEditorContainer.Add(toolbar);

            // GraphView
            embeddedGraphView = new AbilityGraphView(this)
            {
                name = "Ability Graph"
            };
            embeddedGraphView.style.flexGrow = 1;
            
            graphEditorContainer.Add(embeddedGraphView);
        }

        private void BuildEmbeddedBTGraphEditorUI()
        {
            var toolbar = new Toolbar();
            var backButton = new Button(ExitBTGraphEditorMode) { text = "◀ Voltar para o Wizard" };
            var saveButton = new Button(SaveActiveBTGraph) { text = "Salvar Behavior Tree" };
            
            btGraphEditorTitleLabel = new Label(" Editando Behavior Tree: None")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, marginLeft = 12, marginRight = 12, unityTextAlign = TextAnchor.MiddleLeft }
            };

            toolbar.Add(backButton);
            toolbar.Add(saveButton);
            toolbar.Add(btGraphEditorTitleLabel);
            
            btGraphEditorContainer.Add(toolbar);

            embeddedBTGraphView = new BTGraphView(this)
            {
                name = "BT Graph"
            };
            embeddedBTGraphView.style.flexGrow = 1;
            
            btGraphEditorContainer.Add(embeddedBTGraphView);
        }

        private void EnterGraphEditorMode(AbilityGraphSO graph)
        {
            if (graph == null) return;
            
            currentEditingGraph = graph;
            currentViewMode = WizardViewMode.GraphEditor;

            // Update title label
            graphEditorTitleLabel.text = $" Editando Habilidade: {graph.abilityName} ";

            // Hide main wizard steps UI
            wizardStepsContainer.style.display = DisplayStyle.None;

            // Show embedded graph editor
            graphEditorContainer.style.display = DisplayStyle.Flex;

            // Load graph data
            embeddedGraphView.SetGraphAsset(graph);
            GraphSaveUtility.GetInstance(embeddedGraphView).LoadGraph(graph);

            // Focus on graph view
            embeddedGraphView.Focus();
        }

        private void EnterBTGraphEditorMode(BehaviorTreeSO graph)
        {
            if (graph == null) return;
            
            currentEditingBTGraph = graph;
            currentViewMode = WizardViewMode.BTGraphEditor;

            btGraphEditorTitleLabel.text = $" Editando BT: {graph.treeName} ";

            wizardStepsContainer.style.display = DisplayStyle.None;
            graphEditorContainer.style.display = DisplayStyle.None;
            btGraphEditorContainer.style.display = DisplayStyle.Flex;

            embeddedBTGraphView.SetGraphAsset(graph);
            BTSaveUtility.GetInstance(embeddedBTGraphView).LoadGraph(graph);

            embeddedBTGraphView.Focus();
        }

        private void ExitGraphEditorMode()
        {
            // Auto-save the graph when leaving
            SaveActiveGraph();

            currentViewMode = WizardViewMode.WizardSteps;

            // Hide graph editor
            graphEditorContainer.style.display = DisplayStyle.None;

            // Show main wizard steps UI
            wizardStepsContainer.style.display = DisplayStyle.Flex;

            currentEditingGraph = null;
            Repaint();
        }

        private void ExitBTGraphEditorMode()
        {
            SaveActiveBTGraph();

            currentViewMode = WizardViewMode.WizardSteps;
            btGraphEditorContainer.style.display = DisplayStyle.None;
            wizardStepsContainer.style.display = DisplayStyle.Flex;

            currentEditingBTGraph = null;
            Repaint();
        }

        private void SaveActiveGraph()
        {
            if (currentEditingGraph != null && embeddedGraphView != null)
            {
                GraphSaveUtility.GetInstance(embeddedGraphView).SaveGraph(currentEditingGraph);
                ShowNotification(new GUIContent("Grafo salvo com sucesso!"));
            }
        }

        private void SaveActiveBTGraph()
        {
            if (currentEditingBTGraph != null && embeddedBTGraphView != null)
            {
                BTSaveUtility.GetInstance(embeddedBTGraphView).SaveGraph(currentEditingBTGraph);
                ShowNotification(new GUIContent("Behavior Tree salva com sucesso!"));
            }
        }

        private void SaveAllAssociatedGraphs()
        {
            foreach (var graph in associatedGraphs)
            {
                if (graph != null)
                {
                    EditorUtility.SetDirty(graph);
                }
            }
            AssetDatabase.SaveAssets();
        }

        private void DrawIMGUIWizardContent()
        {
            InitStyles();

            DrawHeaderBar();
            DrawBreadcrumbs();

            GUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandWidth(true), GUILayout.Height(position.height - 130));

            switch (currentStep)
            {
                case WizardStep.Identity:
                    DrawIdentityStep();
                    break;
                case WizardStep.Stats:
                    DrawStatsStep();
                    break;
                case WizardStep.Abilities:
                    DrawAbilitiesStep();
                    break;

                case WizardStep.Visuals:
                    DrawVisualsStep();
                    break;
                case WizardStep.Review:
                    DrawReviewStep();
                    break;
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            DrawFooterBar();
        }

        private void InitStyles()
        {
            if (headerStyle != null) return;

            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 15,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.4f, 0.4f) }
            };

            stepHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 13,
                normal = { textColor = new Color(1f, 0.5f, 0.5f) }
            };

            subtitleStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 10,
                fontStyle = FontStyle.Italic,
                normal = { textColor = Color.gray }
            };

            cardStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(10, 10, 10, 10),
                margin = new RectOffset(0, 0, 5, 5)
            };

            footerStyle = new GUIStyle(EditorStyles.toolbar)
            {
                fixedHeight = 40
            };

            richTextStyle = new GUIStyle(EditorStyles.label)
            {
                richText = true
            };
        }

        private void DrawHeaderBar()
        {
            Rect headerRect = GUILayoutUtility.GetRect(position.width, 35);
            EditorGUI.DrawRect(headerRect, new Color(0.2f, 0.1f, 0.1f));
            GUI.Label(headerRect, "CELESTIAL CROSS - ENEMY CREATION SUITE", headerStyle);
        }

        private void DrawBreadcrumbs()
        {
            Rect barRect = GUILayoutUtility.GetRect(position.width, 25);
            EditorGUI.DrawRect(barRect, new Color(0.12f, 0.1f, 0.1f));

            string[] stepNames = { "Identity & AI", "Stats", "Abilities", "Visuals", "Review" };
            float segmentWidth = position.width / stepNames.Length;

            for (int i = 0; i < stepNames.Length; i++)
            {
                Rect segRect = new Rect(barRect.x + i * segmentWidth, barRect.y, segmentWidth, barRect.height);
                bool isActive = (int)currentStep == i;
                bool isPassed = (int)currentStep > i;

                Color textColor = isActive ? new Color(1f, 0.5f, 0.5f) : (isPassed ? Color.white : Color.gray);
                GUIStyle btnStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = isActive ? FontStyle.Bold : FontStyle.Normal,
                    normal = { textColor = textColor }
                };

                if (isActive)
                {
                    EditorGUI.DrawRect(segRect, new Color(0.3f, 0.15f, 0.15f));
                }

                EditorGUIUtility.AddCursorRect(segRect, MouseCursor.Link);
                GUI.Label(segRect, $"{i + 1}. {stepNames[i]}", btnStyle);

                Event e = Event.current;
                if (e.type == EventType.MouseDown && e.button == 0 && segRect.Contains(e.mousePosition))
                {
                    if (currentStep == WizardStep.Identity && i > 0) EnsureFoldersExist();
                    SaveAllAssociatedGraphs();
                    currentStep = (WizardStep)i;
                    e.Use();
                    GUI.FocusControl(null);
                }

                if (i < stepNames.Length - 1)
                {
                    Handles.BeginGUI();
                    Handles.color = new Color(0.3f, 0.2f, 0.2f);
                    Handles.DrawLine(new Vector2(segRect.xMax, segRect.y), new Vector2(segRect.xMax, segRect.yMax));
                    Handles.EndGUI();
                }
            }
        }

        private void DrawFooterBar()
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal(footerStyle);

            if (currentStep > WizardStep.Identity)
            {
                if (GUILayout.Button("◀ Voltar (Back)", GUILayout.Width(120), GUILayout.Height(30)))
                {
                    SaveAllAssociatedGraphs();
                    currentStep--;
                    GUI.FocusControl(null);
                }
            }
            else GUILayout.Space(120);

            GUILayout.FlexibleSpace();
            GUILayout.Label($"Passo {(int)currentStep + 1} de 5", EditorStyles.centeredGreyMiniLabel);
            GUILayout.FlexibleSpace();

            if (currentStep < WizardStep.Review)
            {
                if (GUILayout.Button("Avançar (Next) ▶", GUILayout.Width(120), GUILayout.Height(30)))
                {
                    if (currentStep == WizardStep.Identity) EnsureFoldersExist();
                    SaveAllAssociatedGraphs();
                    currentStep++;
                    GUI.FocusControl(null);
                }
            }
            else GUILayout.Space(120);

            GUILayout.EndHorizontal();
        }

        // ==========================================
        // STEP 1: IDENTITY
        // ==========================================
        private void DrawIdentityStep()
        {
            GUILayout.Label("Passo 1: Identidade e IA do Inimigo", stepHeaderStyle);
            GUILayout.Label("Define nome, papéis táticos e a inteligência (Behavior Tree) do inimigo.", subtitleStyle);
            GUILayout.Space(10);

            GUILayout.Label("Modificar Inimigo Existente (Opcional):", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal(cardStyle);
            loadedUnitData = (UnitData)EditorGUILayout.ObjectField("Asset (UnitData)", loadedUnitData, typeof(UnitData), false);
            if (GUILayout.Button("Carregar (Load)", GUILayout.Width(100), GUILayout.Height(20)))
            {
                if (loadedUnitData != null) LoadUnitFromData(loadedUnitData);
                else EditorUtility.DisplayDialog("Aviso", "Selecione um asset de UnitData antes de carregar.", "OK");
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(15);

            GUILayout.Label("Informações Gerais:", EditorStyles.boldLabel);
            GUILayout.BeginVertical(cardStyle);

            string nextUnitName = EditorGUILayout.TextField("ID/Nome do Asset", unitName);
            if (nextUnitName != unitName)
            {
                string oldName = unitName;
                unitName = nextUnitName;
                RenameUnitFolder(oldName, unitName);
            }

            displayName = EditorGUILayout.TextField("Nome de Exibição", displayName);
            GUILayout.Space(5);
            
            role = (UnitRole)EditorGUILayout.EnumPopup("Função Tática", role);
            unitClass = (UnitClass)EditorGUILayout.EnumPopup("Classe", unitClass);

            GUILayout.EndVertical();

            GUILayout.Space(10);
            GUILayout.Label("Configuração de Inteligência (Behavior Tree):", EditorStyles.boldLabel);
            GUILayout.BeginVertical(cardStyle);
            
            GUILayout.BeginHorizontal();
            defaultBehaviorTree = (Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.BehaviorTreeSO)EditorGUILayout.ObjectField("Behavior Tree Padrão", defaultBehaviorTree, typeof(Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.BehaviorTreeSO), false);
            
            GUI.enabled = defaultBehaviorTree != null;
            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.4f);
            if (GUILayout.Button("Edit Nodes", GUILayout.Width(90), GUILayout.Height(18)))
            {
                EnterBTGraphEditorMode(defaultBehaviorTree);
            }
            GUI.backgroundColor = Color.white;
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            GUILayout.Label("Arraste uma BehaviorTree existente, ou edite os nós clicando em 'Edit Nodes'.", EditorStyles.miniLabel);
            GUILayout.EndVertical();

            GUILayout.Space(10);
            GUILayout.Label("Caminho de Destino (Visualização):", EditorStyles.boldLabel);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(GetUnitFolderPath(unitName), EditorStyles.wordWrappedLabel);
            GUILayout.EndVertical();
        }

        private void LoadUnitFromData(UnitData source)
        {
            unitName = source.name;
            displayName = source.displayName;
            role = source.role;
            unitClass = source.unitClass;
            enemyStats = source.baseStats; // Enemies use flat stats
            maxAP = source.maxAP;
            // Step 3
            associatedGraphs = new List<AbilityGraphSO>(source.abilityGraphs);
            


            // Step 4
            unitIcon = source.icon;
            unitSprite = source.sprite;
            idleAnim = source.idleAnim;
            combatIdleAnim = source.combatIdleAnim;
            defaultBehaviorTree = source.defaultBehaviorTree;

            GUI.FocusControl(null);
            ShowNotification(new GUIContent($"Configurações de '{source.name}' carregadas!"));
        }

        private string GetUnitFolderPath(string name)
        {
            return $"Assets/Celestial-Cross/Prefabs/Units/Enemies/{name}";
        }

        private void RenameUnitFolder(string oldName, string newName)
        {
            if (string.IsNullOrWhiteSpace(oldName) || string.IsNullOrWhiteSpace(newName)) return;
            string oldPath = GetUnitFolderPath(oldName);
            string newPath = GetUnitFolderPath(newName);

            if (Directory.Exists(oldPath) && oldPath != newPath)
            {
                string parentDir = Path.GetDirectoryName(newPath);
                if (!Directory.Exists(parentDir)) Directory.CreateDirectory(parentDir);

                string error = AssetDatabase.MoveAsset(oldPath, newPath);
                if (!string.IsNullOrEmpty(error)) Debug.LogError($"[EnemyCreationWizard] Erro ao renomear: {error}");
                else AssetDatabase.Refresh();
            }
        }

        private void EnsureFoldersExist()
        {
            if (string.IsNullOrWhiteSpace(unitName)) return;
            string unitFolder = GetUnitFolderPath(unitName);
            string dataFolder = $"{unitFolder}/Data";
            string abilitiesFolder = $"{unitFolder}/Abilities";

            if (!Directory.Exists(unitFolder)) Directory.CreateDirectory(unitFolder);
            if (!Directory.Exists(dataFolder)) Directory.CreateDirectory(dataFolder);
            if (!Directory.Exists(abilitiesFolder)) Directory.CreateDirectory(abilitiesFolder);
            AssetDatabase.Refresh();
        }

        // ==========================================
        // STEP 2: STATS
        // ==========================================
        private void DrawStatsStep()
        {
            GUILayout.Label("Passo 2: Atributos de Combate do Inimigo", stepHeaderStyle);
            GUILayout.Label("Configure os atributos brutos que este inimigo possuirá (inimigos não utilizam sistema de nível base/max).", subtitleStyle);
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Presets Rápidos:", GUILayout.Width(100));
            if (GUILayout.Button("Mob Frágil")) SetStatsPreset(50, 15, 5, 10, 5, 0);
            if (GUILayout.Button("Elite Bruiser")) SetStatsPreset(300, 45, 30, 8, 10, 20);
            if (GUILayout.Button("Boss")) SetStatsPreset(1500, 80, 50, 15, 20, 50);
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            GUILayout.BeginVertical(cardStyle);

            DrawStatIntRow("Health (Vida)", ref enemyStats.health, 1, 99999);
            DrawStatIntRow("Max AP (Ações)", ref maxAP, 1, 99);
            DrawStatIntRow("Attack (Ataque)", ref enemyStats.attack, 0, 9999);
            DrawStatIntRow("Defense (Defesa)", ref enemyStats.defense, 0, 9999);
            DrawStatIntRow("Speed (Velocidade)", ref enemyStats.speed, -100, 500);
            DrawStatIntRow("Crit Chance % (Crítico)", ref enemyStats.criticalChance, 0, 100);
            DrawStatIntRow("Crit Damage % (Dano Crit)", ref enemyStats.criticalDamage, 0, 500);
            DrawStatIntRow("Accuracy % (Precisão)", ref enemyStats.effectAccuracy, 0, 100);
            DrawStatIntRow("Resistance % (Resistência)", ref enemyStats.effectResistance, 0, 100);

            GUILayout.EndVertical();
        }

        private void SetStatsPreset(int hp, int atk, int def, int spd, int crit, int acc)
        {
            enemyStats = new CombatStats(hp, atk, def, spd, crit, acc, 50, 0);
            maxAP = 1;
        }

        private void DrawStatIntRow(string label, ref int val, int minLimit, int maxLimit)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(180));
            val = EditorGUILayout.IntField(val, GUILayout.Width(140));
            val = Mathf.Clamp(val, minLimit, maxLimit);
            GUILayout.EndHorizontal();
        }

        // ==========================================
        // STEP 3: ABILITIES
        // ==========================================
        private void DrawAbilitiesStep()
        {
            GUILayout.Label("Passo 3: Habilidades (Ability Graphs)", stepHeaderStyle);
            GUILayout.Label("Gerencie a lista de habilidades do inimigo. A IA lê diretamente estas habilidades.", subtitleStyle);
            GUILayout.Space(10);

            if (graphDetailsToEdit != null)
            {
                GUILayout.Label($"Editar Detalhes Visuais: {graphDetailsToEdit.abilityName}", EditorStyles.boldLabel);
                GUILayout.BeginVertical(cardStyle);
                graphDetailsToEdit.abilityName = EditorGUILayout.TextField("Nome da Habilidade", graphDetailsToEdit.abilityName);
                graphDetailsToEdit.abilityDescription = EditorGUILayout.TextArea(graphDetailsToEdit.abilityDescription, GUILayout.Height(40));
                graphDetailsToEdit.abilityIcon = (Sprite)EditorGUILayout.ObjectField("Ícone da Habilidade", graphDetailsToEdit.abilityIcon, typeof(Sprite), false);
                graphDetailsToEdit.displayRange = EditorGUILayout.IntField("Alcance (Display Range)", graphDetailsToEdit.displayRange);
                GUILayout.Space(8);
                GUILayout.BeginHorizontal();
                GUI.backgroundColor = new Color(0.3f, 0.8f, 0.4f);
                if (GUILayout.Button("Salvar Alterações Visuais", GUILayout.Height(25)))
                {
                    EditorUtility.SetDirty(graphDetailsToEdit);
                    AssetDatabase.SaveAssets();
                    graphDetailsToEdit = null;
                    GUI.FocusControl(null);
                    ShowNotification(new GUIContent("Alterações visuais salvas no disco!"));
                }
                GUI.backgroundColor = Color.white;
                if (GUILayout.Button("Cancelar", GUILayout.Height(25))) graphDetailsToEdit = null;
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.Space(15);
            }

            GUILayout.Label("1. Associar Ability Graphs Existentes:", EditorStyles.boldLabel);
            GUILayout.BeginVertical(cardStyle);
            var newSelectedGraph = (AbilityGraphSO)EditorGUILayout.ObjectField("Selecionar e Adicionar", null, typeof(AbilityGraphSO), false);
            if (newSelectedGraph != null && !associatedGraphs.Contains(newSelectedGraph)) associatedGraphs.Add(newSelectedGraph);
            GUILayout.EndVertical();
            GUILayout.Space(10);

            GUILayout.Label("2. Criar Nova Ability Graph:", EditorStyles.boldLabel);
            GUILayout.BeginVertical(cardStyle);
            newAbilityName = EditorGUILayout.TextField("Nome da Habilidade", newAbilityName);
            newAbilityDescription = EditorGUILayout.TextArea(newAbilityDescription, GUILayout.Height(40));
            newAbilityIcon = (Sprite)EditorGUILayout.ObjectField("Ícone da Habilidade", newAbilityIcon, typeof(Sprite), false);
            newAbilityRange = EditorGUILayout.IntField("Alcance (Display Range)", newAbilityRange);
            newAbilityType = (AbilityType)EditorGUILayout.EnumPopup("Tipo de Habilidade", newAbilityType);
            GUILayout.Space(5);
            if (GUILayout.Button("+ Criar, Salvar no Disco e Adicionar", GUILayout.Height(28))) CreateAndAddGraphOnDisk();
            GUILayout.EndVertical();
            GUILayout.Space(10);

            GUILayout.Label("Habilidades Vinculadas ao Inimigo:", EditorStyles.boldLabel);
            if (associatedGraphs.Count == 0) EditorGUILayout.HelpBox("Nenhuma habilidade. Inimigo passará os turnos sem agir se a BT exigir ataques.", MessageType.Warning);
            else
            {
                for (int i = 0; i < associatedGraphs.Count; i++)
                {
                    var graph = associatedGraphs[i];
                    if (graph == null) continue;
                    GUILayout.BeginHorizontal(cardStyle);
                    if (graph.abilityIcon != null) GUILayout.Label(graph.abilityIcon.texture, GUILayout.Width(32), GUILayout.Height(32));
                    else GUILayout.Box("Icon", GUILayout.Width(32), GUILayout.Height(32));
                    GUILayout.BeginVertical();
                    GUILayout.Label(graph.abilityName, EditorStyles.boldLabel);
                    GUILayout.Label($"Tipo: {graph.GetAbilityType()} | Range: {graph.displayRange}", EditorStyles.miniLabel);
                    GUILayout.EndVertical();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Editar Visual", GUILayout.Width(95), GUILayout.Height(26))) { graphDetailsToEdit = graph; GUI.FocusControl(null); }
                    GUI.backgroundColor = new Color(0.3f, 0.8f, 0.4f);
                    if (GUILayout.Button("Edit Nodes", GUILayout.Width(90), GUILayout.Height(26))) EnterGraphEditorMode(graph);
                    GUI.backgroundColor = Color.white;
                    if (GUILayout.Button("Remover", GUILayout.Width(70), GUILayout.Height(26))) { associatedGraphs.RemoveAt(i); i--; }
                    GUILayout.EndHorizontal();
                }
            }
        }

        private void CreateAndAddGraphOnDisk()
        {
            if (string.IsNullOrWhiteSpace(newAbilityName)) return;
            EnsureFoldersExist();
            AbilityGraphSO newGraph = CreateInstance<AbilityGraphSO>();
            newGraph.abilityName = newAbilityName;
            newGraph.abilityDescription = newAbilityDescription;
            newGraph.abilityIcon = newAbilityIcon;
            newGraph.displayRange = newAbilityRange;
            
            var startNode = new AbilityNodeData
            {
                Guid = Guid.NewGuid().ToString(),
                NodeType = "StartNode",
                NodeTitle = "Start",
                Position = new Vector2(100, 200),
                JsonData = $"{{\"type\":{(int)newAbilityType},\"isBuff\":false}}"
            };
            newGraph.NodeData.Add(startNode);

            string abilitiesFolder = $"{GetUnitFolderPath(unitName)}/Abilities";
            string graphPath = AssetDatabase.GenerateUniqueAssetPath($"{abilitiesFolder}/Ability_{unitName}_{newAbilityName.Replace(" ", "")}.asset");
            AssetDatabase.CreateAsset(newGraph, graphPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            associatedGraphs.Add(newGraph);
            
            newAbilityName = "New Ability";
            newAbilityDescription = "Ability description here...";
            newAbilityIcon = null;
            newAbilityRange = 1;
            newAbilityType = AbilityType.Active;
            GUI.FocusControl(null);
            ShowNotification(new GUIContent("Grafo criado e salvo no disco!"));
        }



        // ==========================================
        // STEP 4: VISUALS
        // ==========================================
        private void DrawVisualsStep()
        {
            GUILayout.Label("Passo 4: Recursos Visuais e Animações", stepHeaderStyle);
            GUILayout.Label("Associe o sprite base e animações para a geração do UnitData.", subtitleStyle);
            GUILayout.Space(10);
            GUILayout.BeginVertical(cardStyle);
            unitIcon = (Sprite)EditorGUILayout.ObjectField("Ícone da Unidade (Sprite)", unitIcon, typeof(Sprite), false);
            unitSprite = (Sprite)EditorGUILayout.ObjectField("Sprite de Combate (Inimigos/Aliados)", unitSprite, typeof(Sprite), false);
            GUILayout.Space(8);
            GUILayout.Label("Animações Base:", EditorStyles.boldLabel);
            idleAnim = (AnimationClip)EditorGUILayout.ObjectField("Idle Animation Clip", idleAnim, typeof(AnimationClip), false);
            combatIdleAnim = (AnimationClip)EditorGUILayout.ObjectField("Combat Idle Animation Clip", combatIdleAnim, typeof(AnimationClip), false);
            GUILayout.EndVertical();
        }

        // ==========================================
        // STEP 5: REVIEW & GENERATION
        // ==========================================
        private void DrawReviewStep()
        {
            GUILayout.Label("Passo 5: Revisão e Geração de Assets", stepHeaderStyle);
            GUILayout.Label("Revise todas as configurações antes de criar os assets do Inimigo.", subtitleStyle);
            GUILayout.Space(10);

            GUILayout.BeginVertical(cardStyle);
            GUILayout.Label("RESUMO DAS CONFIGURAÇÕES:", EditorStyles.boldLabel);
            GUILayout.Space(5);
            GUILayout.Label($"<b>ID/Nome:</b> {unitName}", richTextStyle);
            GUILayout.Label($"<b>Nome de Exibição:</b> {displayName}", richTextStyle);
            GUILayout.Label($"<b>Tipo:</b> Inimigo | <b>Função:</b> {role} | <b>Classe:</b> {unitClass}", richTextStyle);
            GUILayout.Label($"<b>Stats:</b> {enemyStats.health} HP / {maxAP} AP / {enemyStats.attack} ATK / {enemyStats.defense} DEF / {enemyStats.speed} SPD", richTextStyle);
            GUILayout.Label($"<b>Behavior Tree:</b> {(defaultBehaviorTree != null ? defaultBehaviorTree.name : "NENHUMA (Aviso)")}", richTextStyle);
            GUILayout.Label($"<b>Habilidades Vinculadas:</b> {associatedGraphs.Count}", richTextStyle);
            GUILayout.Label($"<b>Modo de Salvamento:</b> {(loadedUnitData != null ? "<color=yellow>Atualizar Inimigo Existente</color>" : "<color=green>Criar Novo Inimigo</color>")}", richTextStyle);
            GUILayout.EndVertical();
            GUILayout.Space(15);

            List<string> errors = new List<string>();
            List<string> warnings = new List<string>();

            if (string.IsNullOrWhiteSpace(unitName)) errors.Add("- ID/Nome do Asset está vazio.");
            if (defaultBehaviorTree == null) warnings.Add("- Nenhuma Behavior Tree configurada (IA não saberá agir).");
            if (associatedGraphs.Count == 0) warnings.Add("- Nenhuma habilidade vinculada.");
            if (unitIcon == null) warnings.Add("- Nenhum Ícone (usado como Sprite base).");

            if (errors.Count > 0)
            {
                GUILayout.Label("CORRIJA OS SEGUINTES ERROS ANTES DE GERAR:", EditorStyles.boldLabel);
                foreach (var err in errors) EditorGUILayout.HelpBox(err, MessageType.Error);
            }
            if (warnings.Count > 0)
            {
                GUILayout.Label("ALERTAS/AVISOS:", EditorStyles.boldLabel);
                foreach (var warn in warnings) EditorGUILayout.HelpBox(warn, MessageType.Warning);
            }

            GUILayout.Space(20);
            GUI.enabled = (errors.Count == 0);
            GUI.backgroundColor = loadedUnitData != null ? new Color(0.9f, 0.6f, 0.2f) : new Color(1f, 0.5f, 0.5f);
            string buttonText = loadedUnitData != null ? "Salvar Alterações no Inimigo" : "Criar Novo Inimigo";
            if (GUILayout.Button(buttonText, GUILayout.Height(45))) GenerateEnemyAssets();
            GUI.backgroundColor = Color.white;
            GUI.enabled = true;
        }

        private void GenerateEnemyAssets()
        {
            string unitFolder = GetUnitFolderPath(unitName);
            string dataFolder = $"{unitFolder}/Data";


            try
            {
                EnsureFoldersExist();
                SaveAllAssociatedGraphs();

                UnitData unitData = loadedUnitData;
                bool isNewUnit = (unitData == null);

                if (isNewUnit) unitData = CreateInstance<UnitData>();

                unitData.displayName = displayName;
                unitData.role = role;
                unitData.unitClass = unitClass;
                unitData.icon = unitIcon;
                unitData.sprite = unitSprite;
                unitData.idleAnim = idleAnim;
                unitData.combatIdleAnim = combatIdleAnim;
                unitData.baseStats = enemyStats;
                unitData.maxAP = maxAP;
                unitData.maxStats = enemyStats; // Flat stats for enemies
                unitData.abilityGraphs = associatedGraphs;
                unitData.defaultBehaviorTree = defaultBehaviorTree;

                if (isNewUnit)
                {
                    string unitDataPath = AssetDatabase.GenerateUniqueAssetPath($"{dataFolder}/{unitName}.asset");
                    AssetDatabase.CreateAsset(unitData, unitDataPath);
                }
                else EditorUtility.SetDirty(unitData);

                string unitGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(unitData));



                EditorUtility.SetDirty(unitData);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Selection.activeObject = unitData;

                string msg = isNewUnit 
                    ? $"Inimigo '{displayName}' foi criado com sucesso na pasta:\n{unitFolder}"
                    : $"Os dados do inimigo '{displayName}' foram atualizados com sucesso!";
                
                EditorUtility.DisplayDialog("Sucesso!", msg, "OK");
                Close();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EnemyCreationWizard] Ocorreu um erro ao gerar o inimigo: {ex.Message}");
                EditorUtility.DisplayDialog("Erro", $"Falha ao gerar os assets da unidade. Detalhes no console.", "OK");
            }
        }
    }
}
#endif
