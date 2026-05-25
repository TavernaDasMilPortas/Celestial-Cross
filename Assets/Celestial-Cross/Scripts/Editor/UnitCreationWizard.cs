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
using Celestial_Cross.Scripts.Abilities.SkillTree;
using Celestial_Cross.Scripts.Abilities.Graph.Editor;
using CelestialCross.Data;

namespace CelestialCross.Editor
{
    public class UnitCreationWizard : EditorWindow
    {
        public enum WizardStep
        {
            Identity,
            Stats,
            Abilities,
            ExtraSystems,
            Visuals,
            Review
        }

        public enum UnitType
        {
            Player,
            Enemy
        }

        public enum WizardViewMode
        {
            WizardSteps,
            GraphEditor
        }

        // --- Wizard State ---
        private WizardStep currentStep = WizardStep.Identity;
        private WizardViewMode currentViewMode = WizardViewMode.WizardSteps;
        private Vector2 scrollPosition;

        // Existing Unit Modification
        private UnitData loadedUnitData;

        // Step 1: Identity
        private string unitName = "NewUnit";
        private string displayName = "New Unit";
        private UnitType unitType = UnitType.Player;
        private UnitRole role = UnitRole.Attacker;
        private UnitClass unitClass = UnitClass.Warrior;

        // Step 2: Stats
        private CombatStats baseStats = new CombatStats(30, 10, 6, 7, 7, 1, 50, 0);
        private CombatStats maxStats = new CombatStats(300, 100, 60, 70, 7, 1, 50, 0);

        // Step 3: Abilities
        private List<AbilityGraphSO> associatedGraphs = new List<AbilityGraphSO>();
        private AbilityGraphSO graphDetailsToEdit; // Inline ability editing reference
        
        // Form fields to add new Ability Graphs
        private string newAbilityName = "New Ability";
        private string newAbilityDescription = "Ability description here...";
        private Sprite newAbilityIcon = null;
        private int newAbilityRange = 1;
        private AbilityType newAbilityType = AbilityType.Active;

        // Step 4: Extra Systems
        private bool generateSkillTree = true;
        private bool generateConstellation = false; // Kept disabled as per request
        
        // Skill tree mapping fields
        private AbilityGraphSO basicAttackGraph;
        private AbilityGraphSO movementSkillGraph;
        private List<AbilityGraphSO> combatSkillGraphs = new List<AbilityGraphSO>();
        private Dictionary<AbilityGraphSO, int> skillSlotAssignments = new Dictionary<AbilityGraphSO, int>();

        // Step 5: Visuals
        private Sprite unitIcon;
        private AnimationClip idleAnim;
        private AnimationClip combatIdleAnim;

        // --- UI Toolkit Containers (Hybrid Model) ---
        private IMGUIContainer wizardStepsContainer;
        private VisualElement graphEditorContainer;
        private AbilityGraphView embeddedGraphView;
        private AbilityGraphSO currentEditingGraph;
        private Label graphEditorTitleLabel;

        // --- Styles & UI Cache ---
        private GUIStyle headerStyle;
        private GUIStyle stepHeaderStyle;
        private GUIStyle footerStyle;
        private GUIStyle cardStyle;
        private GUIStyle subtitleStyle;
        private GUIStyle richTextStyle;

        [MenuItem("Celestial Cross/Editors/Unit Creation Wizard")]
        public static void OpenWindow()
        {
            var window = GetWindow<UnitCreationWizard>("Unit Wizard");
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

            BuildEmbeddedGraphEditorUI();
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

        private void SaveActiveGraph()
        {
            if (currentEditingGraph != null && embeddedGraphView != null)
            {
                GraphSaveUtility.GetInstance(embeddedGraphView).SaveGraph(currentEditingGraph);
                ShowNotification(new GUIContent("Grafo salvo com sucesso!"));
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

            // Draw header bar
            DrawHeaderBar();

            // Draw step indicator breadcrumbs
            DrawBreadcrumbs();

            // Draw content area in a ScrollView
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
                case WizardStep.ExtraSystems:
                    DrawExtraSystemsStep();
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

            // Draw footer navigation bar
            DrawFooterBar();
        }

        private void InitStyles()
        {
            if (headerStyle != null) return;

            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 15,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.85f, 0.85f, 1f) }
            };

            stepHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 13,
                normal = { textColor = new Color(0.3f, 0.7f, 1f) }
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
            EditorGUI.DrawRect(headerRect, new Color(0.15f, 0.15f, 0.18f));
            GUI.Label(headerRect, "CELESTIAL CROSS - UNIT CONFIGURATION SUITE", headerStyle);
        }

        private void DrawBreadcrumbs()
        {
            Rect barRect = GUILayoutUtility.GetRect(position.width, 25);
            EditorGUI.DrawRect(barRect, new Color(0.1f, 0.1f, 0.12f));

            string[] stepNames = { "Identity", "Stats", "Abilities", "Systems", "Visuals", "Review" };
            float segmentWidth = position.width / stepNames.Length;

            for (int i = 0; i < stepNames.Length; i++)
            {
                Rect segRect = new Rect(barRect.x + i * segmentWidth, barRect.y, segmentWidth, barRect.height);
                bool isActive = (int)currentStep == i;
                bool isPassed = (int)currentStep > i;

                Color textColor = isActive ? new Color(0.3f, 0.7f, 1f) : (isPassed ? Color.white : Color.gray);
                GUIStyle btnStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = isActive ? FontStyle.Bold : FontStyle.Normal,
                    normal = { textColor = textColor }
                };

                if (isActive)
                {
                    EditorGUI.DrawRect(segRect, new Color(0.2f, 0.2f, 0.25f));
                }

                // Add hand link cursor on hover to indicate clickability
                EditorGUIUtility.AddCursorRect(segRect, MouseCursor.Link);

                GUI.Label(segRect, $"{i + 1}. {stepNames[i]}", btnStyle);

                // Handle Tab Click
                Event e = Event.current;
                if (e.type == EventType.MouseDown && e.button == 0 && segRect.Contains(e.mousePosition))
                {
                    if (currentStep == WizardStep.Identity && i > 0)
                    {
                        EnsureFoldersExist();
                    }

                    SaveAllAssociatedGraphs();
                    currentStep = (WizardStep)i;
                    e.Use();
                    GUI.FocusControl(null);
                }

                if (i < stepNames.Length - 1)
                {
                    Handles.BeginGUI();
                    Handles.color = new Color(0.2f, 0.2f, 0.2f);
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
                    // Ensure current graphs are saved
                    SaveAllAssociatedGraphs();
                    currentStep--;
                    GUI.FocusControl(null);
                }
            }
            else
            {
                GUILayout.Space(120);
            }

            GUILayout.FlexibleSpace();
            GUILayout.Label($"Passo {(int)currentStep + 1} de 6", EditorStyles.centeredGreyMiniLabel);
            GUILayout.FlexibleSpace();

            if (currentStep < WizardStep.Review)
            {
                if (GUILayout.Button("Avançar (Next) ▶", GUILayout.Width(120), GUILayout.Height(30)))
                {
                    // If moving from step 1 (identity) to 2, make sure folders exist on disk
                    if (currentStep == WizardStep.Identity)
                    {
                        EnsureFoldersExist();
                    }

                    // Ensure current graphs are saved
                    SaveAllAssociatedGraphs();
                    currentStep++;
                    GUI.FocusControl(null);
                }
            }
            else
            {
                GUILayout.Space(120);
            }

            GUILayout.EndHorizontal();
        }

        // ==========================================
        // STEP 1: IDENTITY
        // ==========================================
        private void DrawIdentityStep()
        {
            GUILayout.Label("Passo 1: Identidade Geral da Unidade", stepHeaderStyle);
            GUILayout.Label("Define o nome, classificações básicas ou carregue uma unidade existente para editar.", subtitleStyle);
            GUILayout.Space(10);

            // Existing Unit Loading Section
            GUILayout.Label("Modificar Unidade Existente (Opcional):", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal(cardStyle);
            loadedUnitData = (UnitData)EditorGUILayout.ObjectField("Asset (UnitData)", loadedUnitData, typeof(UnitData), false);
            if (GUILayout.Button("Carregar (Load)", GUILayout.Width(100), GUILayout.Height(20)))
            {
                if (loadedUnitData != null)
                {
                    LoadUnitFromData(loadedUnitData);
                }
                else
                {
                    EditorUtility.DisplayDialog("Aviso", "Selecione um asset de UnitData antes de carregar.", "OK");
                }
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
                RenameUnitFolder(oldName, unitType, unitName, unitType);
            }

            displayName = EditorGUILayout.TextField("Nome de Exibição", displayName);

            GUILayout.Space(5);
            
            UnitType nextUnitType = (UnitType)EditorGUILayout.EnumPopup("Tipo de Unidade", unitType);
            if (nextUnitType != unitType)
            {
                UnitType oldType = unitType;
                unitType = nextUnitType;
                RenameUnitFolder(unitName, oldType, unitName, unitType);
            }

            role = (UnitRole)EditorGUILayout.EnumPopup("Função Tática", role);
            unitClass = (UnitClass)EditorGUILayout.EnumPopup("Classe", unitClass);

            GUILayout.EndVertical();

            // Folder path preview card
            GUILayout.Space(10);
            GUILayout.Label("Caminho de Destino (Visualização):", EditorStyles.boldLabel);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            string destination = GetUnitFolderPath(unitName, unitType);
            GUILayout.Label(destination, EditorStyles.wordWrappedLabel);
            GUILayout.Label("Nota: Renomear o ID ou alterar o tipo moverá automaticamente qualquer asset já gerado no disco.", EditorStyles.miniLabel);
            GUILayout.EndVertical();
        }

        private void LoadUnitFromData(UnitData source)
        {
            unitName = source.name;
            displayName = source.displayName;
            
            string path = AssetDatabase.GetAssetPath(source);
            unitType = path.Contains("Enemies") ? UnitType.Enemy : UnitType.Player;

            role = source.role;
            unitClass = source.unitClass;
            baseStats = source.baseStats;
            maxStats = source.maxStats;
            
            associatedGraphs = new List<AbilityGraphSO>(source.abilityGraphs);
            
            unitIcon = source.icon;
            idleAnim = source.idleAnim;
            combatIdleAnim = source.combatIdleAnim;

            skillSlotAssignments.Clear();
            if (source.skillTreeConfig != null)
            {
                generateSkillTree = true;
                basicAttackGraph = source.skillTreeConfig.basicAttack;
                if (basicAttackGraph != null && !associatedGraphs.Contains(basicAttackGraph))
                {
                    associatedGraphs.Add(basicAttackGraph);
                }

                movementSkillGraph = source.skillTreeConfig.movementSkill;
                if (movementSkillGraph != null && !associatedGraphs.Contains(movementSkillGraph))
                {
                    associatedGraphs.Add(movementSkillGraph);
                }

                combatSkillGraphs = new List<AbilityGraphSO>();

                #pragma warning disable 612, 618
                bool hasNewSlots = (source.skillTreeConfig.slot1Skills != null && source.skillTreeConfig.slot1Skills.Count > 0) ||
                                   (source.skillTreeConfig.slot2Skills != null && source.skillTreeConfig.slot2Skills.Count > 0);

                if (hasNewSlots)
                {
                    if (source.skillTreeConfig.slot1Skills != null)
                    {
                        foreach (var graph in source.skillTreeConfig.slot1Skills)
                        {
                            if (graph == null) continue;
                            if (!associatedGraphs.Contains(graph))
                            {
                                associatedGraphs.Add(graph);
                            }
                            combatSkillGraphs.Add(graph);
                            skillSlotAssignments[graph] = 1;
                        }
                    }
                    if (source.skillTreeConfig.slot2Skills != null)
                    {
                        foreach (var graph in source.skillTreeConfig.slot2Skills)
                        {
                            if (graph == null) continue;
                            if (!associatedGraphs.Contains(graph))
                            {
                                associatedGraphs.Add(graph);
                            }
                            combatSkillGraphs.Add(graph);
                            skillSlotAssignments[graph] = 2;
                        }
                    }
                }
                else if (source.skillTreeConfig.combatSkills != null)
                {
                    foreach (var graph in source.skillTreeConfig.combatSkills)
                    {
                        if (graph == null) continue;
                        if (!associatedGraphs.Contains(graph))
                        {
                            associatedGraphs.Add(graph);
                        }
                        combatSkillGraphs.Add(graph);
                    }
                    int count = 0;
                    foreach (var graph in source.skillTreeConfig.combatSkills)
                    {
                        if (graph == null) continue;
                        int slot = (count < source.skillTreeConfig.combatSkills.Count / 2) ? 1 : 2;
                        skillSlotAssignments[graph] = slot;
                        count++;
                    }
                }
                #pragma warning restore 612, 618
            }
            else
            {
                generateSkillTree = false;
                basicAttackGraph = null;
                movementSkillGraph = null;
                combatSkillGraphs.Clear();
            }

            GUI.FocusControl(null);
            ShowNotification(new GUIContent($"Configurações de '{source.name}' carregadas!"));
        }

        private string GetUnitFolderPath(string name, UnitType type)
        {
            string root = type == UnitType.Player 
                ? "Assets/Celestial-Cross/Prefabs/Units/MagicalGirls" 
                : "Assets/Celestial-Cross/Prefabs/Units/Enemies";
            
            return $"{root}/{name}";
        }

        private void RenameUnitFolder(string oldName, UnitType oldType, string newName, UnitType newType)
        {
            if (string.IsNullOrWhiteSpace(oldName) || string.IsNullOrWhiteSpace(newName)) return;

            string oldPath = GetUnitFolderPath(oldName, oldType);
            string newPath = GetUnitFolderPath(newName, newType);

            if (Directory.Exists(oldPath) && oldPath != newPath)
            {
                string parentDir = Path.GetDirectoryName(newPath);
                if (!Directory.Exists(parentDir))
                {
                    Directory.CreateDirectory(parentDir);
                }

                string error = AssetDatabase.MoveAsset(oldPath, newPath);
                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogError($"[UnitCreationWizard] Erro ao renomear pasta de {oldPath} para {newPath}: {error}");
                }
                else
                {
                    AssetDatabase.Refresh();
                }
            }
        }

        private void EnsureFoldersExist()
        {
            if (string.IsNullOrWhiteSpace(unitName)) return;

            string unitFolder = GetUnitFolderPath(unitName, unitType);
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
            GUILayout.Label("Passo 2: Atributos de Combate (CombatStats)", stepHeaderStyle);
            GUILayout.Label("Configure os atributos no Nível Inicial (Base Stats) e no Nível Máximo (Max Stats) para a progressão linear.", subtitleStyle);
            GUILayout.Space(10);

            // Presets row
            GUILayout.BeginHorizontal();
            GUILayout.Label("Presets Rápidos:", GUILayout.Width(100));
            if (GUILayout.Button("Atacante (Attacker)")) SetStatsPreset(250, 100, 30, 60, 20, 20);
            if (GUILayout.Button("Tanque (Tank)")) SetStatsPreset(500, 45, 75, 25, 5, 10);
            if (GUILayout.Button("Suporte (Support)")) SetStatsPreset(320, 50, 45, 50, 10, 40);
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            // Double columns table
            GUILayout.BeginVertical(cardStyle);

            DrawStatRowHeader();
            DrawStatIntRow("Health (Vida)", ref baseStats.health, ref maxStats.health, 1, 9999);
            DrawStatIntRow("Attack (Ataque)", ref baseStats.attack, ref maxStats.attack, 0, 999);
            DrawStatIntRow("Defense (Defesa)", ref baseStats.defense, ref maxStats.defense, 0, 999);
            DrawStatIntRow("Speed (Velocidade)", ref baseStats.speed, ref maxStats.speed, -100, 500);
            DrawStatIntRow("Crit Chance % (Crítico)", ref baseStats.criticalChance, ref maxStats.criticalChance, 0, 100);
            DrawStatIntRow("Crit Damage % (Dano Crit)", ref baseStats.criticalDamage, ref maxStats.criticalDamage, 0, 500);
            DrawStatIntRow("Accuracy % (Precisão)", ref baseStats.effectAccuracy, ref maxStats.effectAccuracy, 0, 100);
            DrawStatIntRow("Resistance % (Resistência)", ref baseStats.effectResistance, ref maxStats.effectResistance, 0, 100);

            GUILayout.EndVertical();
        }

        private void SetStatsPreset(int hpMax, int atkMax, int defMax, int spdMax, int critMax, int accMax)
        {
            baseStats = new CombatStats(
                Mathf.RoundToInt(hpMax * 0.12f),
                Mathf.RoundToInt(atkMax * 0.12f),
                Mathf.RoundToInt(defMax * 0.12f),
                Mathf.RoundToInt(spdMax * 0.8f),
                Mathf.Clamp(critMax - 10, 0, 100),
                Mathf.Clamp(accMax - 15, 0, 100),
                50,
                0
            );

            maxStats = new CombatStats(hpMax, atkMax, defMax, spdMax, critMax, accMax, 150, 30);
        }

        private void DrawStatRowHeader()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Atributo", EditorStyles.boldLabel, GUILayout.Width(150));
            GUILayout.Label("Base (Lvl 1)", EditorStyles.boldLabel, GUILayout.Width(140));
            GUILayout.Label("Max (Lvl 100)", EditorStyles.boldLabel, GUILayout.Width(140));
            GUILayout.EndHorizontal();
            GUILayout.Space(3);
        }

        private void DrawStatIntRow(string label, ref int baseVal, ref int maxVal, int minLimit, int maxLimit)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(150));
            baseVal = EditorGUILayout.IntField(baseVal, GUILayout.Width(140));
            baseVal = Mathf.Clamp(baseVal, minLimit, maxLimit);
            maxVal = EditorGUILayout.IntField(maxVal, GUILayout.Width(140));
            maxVal = Mathf.Clamp(maxVal, minLimit, maxLimit);
            GUILayout.EndHorizontal();
        }

        // ==========================================
        // STEP 3: ABILITIES
        // ==========================================
        private void DrawAbilitiesStep()
        {
            GUILayout.Label("Passo 3: Habilidades e Ability Graphs", stepHeaderStyle);
            GUILayout.Label("Gerencie a lista de habilidades vinculadas a esta unidade. Novos grafos são salvos no disco imediatamente.", subtitleStyle);
            GUILayout.Space(10);

            // Inline Ability Graph visual modification panel
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
                if (GUILayout.Button("Cancelar", GUILayout.Height(25)))
                {
                    graphDetailsToEdit = null;
                    GUI.FocusControl(null);
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.Space(15);
            }

            // Drag-and-drop / selector of existing graphs
            GUILayout.Label("1. Associar Ability Graphs Existentes:", EditorStyles.boldLabel);
            GUILayout.BeginVertical(cardStyle);
            
            var newSelectedGraph = (AbilityGraphSO)EditorGUILayout.ObjectField("Selecionar e Adicionar", null, typeof(AbilityGraphSO), false);
            if (newSelectedGraph != null)
            {
                if (!associatedGraphs.Contains(newSelectedGraph))
                {
                    associatedGraphs.Add(newSelectedGraph);
                }
            }

            GUILayout.EndVertical();
            GUILayout.Space(10);

            // Creator of new Ability Graphs on disk
            GUILayout.Label("2. Criar Nova Ability Graph:", EditorStyles.boldLabel);
            GUILayout.BeginVertical(cardStyle);

            newAbilityName = EditorGUILayout.TextField("Nome da Habilidade", newAbilityName);
            newAbilityDescription = EditorGUILayout.TextArea(newAbilityDescription, GUILayout.Height(40));
            newAbilityIcon = (Sprite)EditorGUILayout.ObjectField("Ícone da Habilidade", newAbilityIcon, typeof(Sprite), false);
            newAbilityRange = EditorGUILayout.IntField("Alcance (Display Range)", newAbilityRange);
            newAbilityType = (AbilityType)EditorGUILayout.EnumPopup("Tipo de Habilidade", newAbilityType);

            GUILayout.Space(5);
            if (GUILayout.Button("+ Criar, Salvar no Disco e Adicionar", GUILayout.Height(28)))
            {
                CreateAndAddGraphOnDisk();
            }

            GUILayout.EndVertical();
            GUILayout.Space(10);

            // Current associated graphs list
            GUILayout.Label("Habilidades Vinculadas a esta Unidade:", EditorStyles.boldLabel);
            if (associatedGraphs.Count == 0)
            {
                EditorGUILayout.HelpBox("Nenhuma habilidade adicionada ainda. Crie um novo grafo acima ou associe um existente.", MessageType.Warning);
            }
            else
            {
                for (int i = 0; i < associatedGraphs.Count; i++)
                {
                    var graph = associatedGraphs[i];
                    if (graph == null) continue;

                    GUILayout.BeginHorizontal(cardStyle);
                    
                    // Mini preview icon
                    if (graph.abilityIcon != null)
                    {
                        GUILayout.Label(graph.abilityIcon.texture, GUILayout.Width(32), GUILayout.Height(32));
                    }
                    else
                    {
                        GUILayout.Box("Icon", GUILayout.Width(32), GUILayout.Height(32));
                    }

                    GUILayout.BeginVertical();
                    GUILayout.Label(graph.abilityName, EditorStyles.boldLabel);
                    GUILayout.Label($"Tipo: {graph.GetAbilityType()} | Range: {graph.displayRange}", EditorStyles.miniLabel);
                    GUILayout.EndVertical();

                    GUILayout.FlexibleSpace();

                    // Details Editor button (Inline)
                    if (GUILayout.Button("Editar Visual", GUILayout.Width(95), GUILayout.Height(26)))
                    {
                        graphDetailsToEdit = graph;
                        GUI.FocusControl(null);
                    }

                    // Embedded edit button (Opens visual logic graph editor within the wizard itself!)
                    GUI.backgroundColor = new Color(0.3f, 0.8f, 0.4f);
                    if (GUILayout.Button("Edit Nodes", GUILayout.Width(90), GUILayout.Height(26)))
                    {
                        EnterGraphEditorMode(graph);
                    }
                    GUI.backgroundColor = Color.white;

                    // Remove button
                    if (GUILayout.Button("Remover", GUILayout.Width(70), GUILayout.Height(26)))
                    {
                        associatedGraphs.RemoveAt(i);
                        i--;
                    }

                    GUILayout.EndHorizontal();
                }
            }
        }

        private void CreateAndAddGraphOnDisk()
        {
            if (string.IsNullOrWhiteSpace(newAbilityName))
            {
                EditorUtility.DisplayDialog("Erro", "Forneça um nome válido para a habilidade.", "OK");
                return;
            }

            // 1. Ensure unit folders exist first
            EnsureFoldersExist();

            // 2. Create the asset
            AbilityGraphSO newGraph = CreateInstance<AbilityGraphSO>();
            newGraph.abilityName = newAbilityName;
            newGraph.abilityDescription = newAbilityDescription;
            newGraph.abilityIcon = newAbilityIcon;
            newGraph.displayRange = newAbilityRange;
            
            // Default seed nodes so that GetAbilityType works properly initially
            var startNode = new AbilityNodeData
            {
                Guid = Guid.NewGuid().ToString(),
                NodeType = "StartNode",
                NodeTitle = "Start",
                Position = new Vector2(100, 200),
                JsonData = $"{{\"type\":{(int)newAbilityType},\"isBuff\":false}}"
            };
            newGraph.NodeData.Add(startNode);

            string abilitiesFolder = $"{GetUnitFolderPath(unitName, unitType)}/Abilities";
            string graphPath = $"{abilitiesFolder}/Ability_{unitName}_{newAbilityName.Replace(" ", "")}.asset";
            graphPath = AssetDatabase.GenerateUniqueAssetPath(graphPath);

            AssetDatabase.CreateAsset(newGraph, graphPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            associatedGraphs.Add(newGraph);

            // Reset fields
            newAbilityName = "New Ability";
            newAbilityDescription = "Ability description here...";
            newAbilityIcon = null;
            newAbilityRange = 1;
            newAbilityType = AbilityType.Active;

            GUI.FocusControl(null);
            ShowNotification(new GUIContent("Grafo criado e salvo no disco!"));
        }

        // ==========================================
        // STEP 4: EXTRA SYSTEMS
        // ==========================================
        private void DrawExtraSystemsStep()
        {
            GUILayout.Label("Passo 4: Configuração de Sistemas Extras", stepHeaderStyle);
            GUILayout.Label("Configurações extras da árvore de habilidades (Skill Tree) e constelações associadas.", subtitleStyle);
            GUILayout.Space(10);

            // Skill Tree Config Group
            GUILayout.BeginVertical(cardStyle);
            generateSkillTree = EditorGUILayout.BeginToggleGroup("Gerar Árvore de Habilidades (SkillTreeConfigSO)", generateSkillTree);
            GUILayout.Space(5);

            if (associatedGraphs.Count == 0)
            {
                EditorGUILayout.HelpBox("Nenhuma habilidade adicionada ao Wizard ainda. Vá ao Passo 3 para criar/associar habilidades primeiro.", MessageType.Info);
            }
            else
            {
                GUILayout.Label("Associe as habilidades criadas nos slots correspondentes da Árvore:", EditorStyles.miniBoldLabel);
                GUILayout.Space(5);

                var graphNames = associatedGraphs.Select(g => g.abilityName).ToArray();

                // Dropdown for Basic Attack
                int basicIndex = GetGraphIndexInList(basicAttackGraph);
                int newBasicIndex = EditorGUILayout.Popup("Habilidade Básica", basicIndex, graphNames);
                if (newBasicIndex >= 0 && newBasicIndex < associatedGraphs.Count)
                {
                    var newBasic = associatedGraphs[newBasicIndex];
                    if (newBasic != basicAttackGraph)
                    {
                        basicAttackGraph = newBasic;
                        if (basicAttackGraph != null)
                        {
                            combatSkillGraphs.Remove(basicAttackGraph);
                            skillSlotAssignments.Remove(basicAttackGraph);
                        }
                    }
                }

                // Dropdown for Movement Skill
                int moveIndex = GetGraphIndexInList(movementSkillGraph);
                int newMoveIndex = EditorGUILayout.Popup("Habilidade de Movimentação", moveIndex, graphNames);
                if (newMoveIndex >= 0 && newMoveIndex < associatedGraphs.Count)
                {
                    var newMove = associatedGraphs[newMoveIndex];
                    if (newMove != movementSkillGraph)
                    {
                        movementSkillGraph = newMove;
                        if (movementSkillGraph != null)
                        {
                            combatSkillGraphs.Remove(movementSkillGraph);
                            skillSlotAssignments.Remove(movementSkillGraph);
                        }
                    }
                }

                // Enforce retroactive exclusion
                if (basicAttackGraph != null)
                {
                    combatSkillGraphs.Remove(basicAttackGraph);
                    skillSlotAssignments.Remove(basicAttackGraph);
                }
                if (movementSkillGraph != null)
                {
                    combatSkillGraphs.Remove(movementSkillGraph);
                    skillSlotAssignments.Remove(movementSkillGraph);
                }

                GUILayout.Space(12);
                GUILayout.Label("Configurar Habilidades nos Slots de Combate:", EditorStyles.boldLabel);
                GUILayout.Label("Defina se cada habilidade pertence ao Slot 1, Slot 2 ou se está desativada. As habilidades básicas e de deslocamento não aparecem nesta lista.", subtitleStyle);
                GUILayout.Space(8);

                foreach (var graph in associatedGraphs)
                {
                    // Skip basic attack and movement skill
                    if (graph == null || graph == basicAttackGraph || graph == movementSkillGraph)
                        continue;

                    GUILayout.BeginHorizontal();
                    GUILayout.Label(graph.abilityName, GUILayout.Width(250));

                    int currentSlotAssignment = 0;
                    skillSlotAssignments.TryGetValue(graph, out currentSlotAssignment);

                    string[] slotOptions = { "Desativada (None)", "Slot 1", "Slot 2" };
                    int newSlotAssignment = EditorGUILayout.Popup(currentSlotAssignment, slotOptions, GUILayout.Width(150));

                    if (newSlotAssignment != currentSlotAssignment)
                    {
                        skillSlotAssignments[graph] = newSlotAssignment;

                        if (newSlotAssignment == 1 || newSlotAssignment == 2)
                        {
                            if (!combatSkillGraphs.Contains(graph))
                            {
                                combatSkillGraphs.Add(graph);
                            }
                        }
                        else
                        {
                            combatSkillGraphs.Remove(graph);
                        }
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(2);
                }
            }

            EditorGUILayout.EndToggleGroup();
            GUILayout.EndVertical();

            GUILayout.Space(15);

            // Constellation Config Group (Disabled as per request)
            GUILayout.BeginVertical(cardStyle);
            
            GUI.enabled = false;
            generateConstellation = EditorGUILayout.ToggleLeft("Gerar Constelação (ConstellationConfigSO)", generateConstellation);
            GUILayout.Label("Nota: A geração automática de constelação está temporariamente desativada no momento.", EditorStyles.miniLabel);
            GUI.enabled = true;

            GUILayout.EndVertical();
        }

        private int GetGraphIndexInList(AbilityGraphSO target)
        {
            if (target == null) return 0;
            int idx = associatedGraphs.IndexOf(target);
            return idx >= 0 ? idx : 0;
        }

        // ==========================================
        // STEP 5: VISUALS
        // ==========================================
        private void DrawVisualsStep()
        {
            GUILayout.Label("Passo 5: Recursos Visuais e Animações", stepHeaderStyle);
            GUILayout.Label("Associe os recursos visuais de sprites e clipes de animação que serão consumidos em combate.", subtitleStyle);
            GUILayout.Space(10);

            GUILayout.BeginVertical(cardStyle);

            unitIcon = (Sprite)EditorGUILayout.ObjectField("Ícone da Unidade (Sprite)", unitIcon, typeof(Sprite), false);
            GUILayout.Space(8);

            GUILayout.Label("Animações Base da Unidade (Usadas no combate):", EditorStyles.boldLabel);
            idleAnim = (AnimationClip)EditorGUILayout.ObjectField("Idle Animation Clip", idleAnim, typeof(AnimationClip), false);
            combatIdleAnim = (AnimationClip)EditorGUILayout.ObjectField("Combat Idle Animation Clip", combatIdleAnim, typeof(AnimationClip), false);

            GUILayout.EndVertical();
        }

        // ==========================================
        // STEP 6: REVIEW & GENERATION
        // ==========================================
        private void DrawReviewStep()
        {
            GUILayout.Label("Passo 6: Revisão e Geração de Assets", stepHeaderStyle);
            GUILayout.Label("Revise todas as configurações antes de criar ou atualizar os assets no projeto.", subtitleStyle);
            GUILayout.Space(10);

            // Summary Card
            GUILayout.BeginVertical(cardStyle);
            GUILayout.Label("RESUMO DAS CONFIGURAÇÕES:", EditorStyles.boldLabel);
            GUILayout.Space(5);

            GUILayout.Label($"<b>ID/Nome:</b> {unitName}", richTextStyle);
            GUILayout.Label($"<b>Nome de Exibição:</b> {displayName}", richTextStyle);
            GUILayout.Label($"<b>Tipo:</b> {unitType} | <b>Função:</b> {role} | <b>Classe:</b> {unitClass}", richTextStyle);
            GUILayout.Label($"<b>Stats Iniciais (HP/ATK/DEF/SPD):</b> {baseStats.health}/{baseStats.attack}/{baseStats.defense}/{baseStats.speed}", richTextStyle);
            GUILayout.Label($"<b>Habilidades Vinculadas:</b> {associatedGraphs.Count}", richTextStyle);
            GUILayout.Label($"<b>Gerar/Atualizar SkillTree:</b> {(generateSkillTree ? "Sim" : "Não")}", richTextStyle);
            GUILayout.Label($"<b>Ícone e Animações Configurados:</b> {(unitIcon != null && idleAnim != null && combatIdleAnim != null ? "Sim" : "Parcial/Não")}", richTextStyle);
            GUILayout.Label($"<b>Modo de Salvamento:</b> {(loadedUnitData != null ? "<color=yellow>Atualizar Unidade Existente</color>" : "<color=green>Criar Nova Unidade</color>")}", richTextStyle);
            
            GUILayout.EndVertical();
            GUILayout.Space(15);

            // Validation messages
            List<string> errors = new List<string>();
            List<string> warnings = new List<string>();

            if (string.IsNullOrWhiteSpace(unitName)) errors.Add("- ID/Nome do Asset está vazio.");
            if (associatedGraphs.Count == 0) warnings.Add("- Nenhuma habilidade vinculada (lista vazia).");
            if (unitIcon == null) warnings.Add("- Nenhum Ícone de Unidade foi selecionado.");
            if (idleAnim == null || combatIdleAnim == null) warnings.Add("- Clipes de Animação de Idle/Combat não atribuídos.");
            if (generateSkillTree && (basicAttackGraph == null || movementSkillGraph == null)) warnings.Add("- Árvore ativa mas faltam mapeamentos de Habilidade Básica/Movimento.");

            if (errors.Count > 0)
            {
                GUILayout.Label("CORRIJA OS SEGUINTES ERROS ANTES DE GERAR:", EditorStyles.boldLabel);
                foreach (var err in errors)
                {
                    EditorGUILayout.HelpBox(err, MessageType.Error);
                }
            }

            if (warnings.Count > 0)
            {
                GUILayout.Label("ALERTAS/AVISOS (Você pode continuar se quiser):", EditorStyles.boldLabel);
                foreach (var warn in warnings)
                {
                    EditorGUILayout.HelpBox(warn, MessageType.Warning);
                }
            }

            GUILayout.Space(20);

            // Main Action Button
            GUI.enabled = (errors.Count == 0);
            GUI.backgroundColor = loadedUnitData != null ? new Color(0.9f, 0.6f, 0.2f) : new Color(0.3f, 0.8f, 0.4f);
            
            string buttonText = loadedUnitData != null ? "Salvar Alterações na Unidade" : "Criar Unidade Completa";
            if (GUILayout.Button(buttonText, GUILayout.Height(45)))
            {
                GenerateUnitAssets();
            }

            GUI.backgroundColor = Color.white;
            GUI.enabled = true;
        }

        private void GenerateUnitAssets()
        {
            string unitFolder = GetUnitFolderPath(unitName, unitType);
            string dataFolder = $"{unitFolder}/Data";

            try
            {
                // 1. Ensure folders are fully created
                EnsureFoldersExist();

                // 2. Save all active edits on associated graphs
                SaveAllAssociatedGraphs();

                // 3. Create or Update UnitData SO
                UnitData unitData = loadedUnitData;
                bool isNewUnit = (unitData == null);

                if (isNewUnit)
                {
                    unitData = CreateInstance<UnitData>();
                }

                unitData.displayName = displayName;
                unitData.role = role;
                unitData.unitClass = unitClass;
                unitData.icon = unitIcon;
                unitData.idleAnim = idleAnim;
                unitData.combatIdleAnim = combatIdleAnim;
                unitData.baseStats = baseStats;
                unitData.maxStats = maxStats;
                unitData.abilityGraphs = associatedGraphs;

                if (isNewUnit)
                {
                    string unitDataPath = $"{dataFolder}/{unitName}.asset";
                    unitDataPath = AssetDatabase.GenerateUniqueAssetPath(unitDataPath);
                    AssetDatabase.CreateAsset(unitData, unitDataPath);
                }
                else
                {
                    EditorUtility.SetDirty(unitData);
                }

                string unitGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(unitData));

                // 4. Create or Update SkillTreeConfig SO
                if (generateSkillTree)
                {
                    SkillTreeConfigSO skillTree = unitData.skillTreeConfig;
                    bool isNewSkillTree = (skillTree == null);

                    if (isNewSkillTree)
                    {
                        skillTree = CreateInstance<SkillTreeConfigSO>();
                    }

                    skillTree.characterId = unitGuid;
                    skillTree.basicAttack = basicAttackGraph;
                    skillTree.movementSkill = movementSkillGraph;
                    
                    #pragma warning disable 612, 618
                    skillTree.slot1Skills = new List<AbilityGraphSO>();
                    skillTree.slot2Skills = new List<AbilityGraphSO>();
                    skillTree.combatSkills = new List<AbilityGraphSO>();
                    
                    foreach (var cGraph in combatSkillGraphs)
                    {
                        if (cGraph == null) continue;
                        
                        skillTree.combatSkills.Add(cGraph);
                        
                        skillSlotAssignments.TryGetValue(cGraph, out int slot);
                        if (slot == 1)
                        {
                            skillTree.slot1Skills.Add(cGraph);
                        }
                        else if (slot == 2)
                        {
                            skillTree.slot2Skills.Add(cGraph);
                        }
                    }
                    #pragma warning restore 612, 618

                    if (isNewSkillTree)
                    {
                        string skillTreePath = $"{dataFolder}/{unitName}_SkillTree.asset";
                        skillTreePath = AssetDatabase.GenerateUniqueAssetPath(skillTreePath);
                        AssetDatabase.CreateAsset(skillTree, skillTreePath);
                        
                        unitData.skillTreeConfig = skillTree;
                    }
                    else
                    {
                        EditorUtility.SetDirty(skillTree);
                    }
                }

                // 5. Final Save & Refresh
                EditorUtility.SetDirty(unitData);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // Select the UnitData in editor
                Selection.activeObject = unitData;

                string msg = isNewUnit 
                    ? $"A unidade '{displayName}' foi criada com sucesso na pasta:\n{unitFolder}"
                    : $"Os dados da unidade '{displayName}' foram atualizados com sucesso!";
                
                EditorUtility.DisplayDialog("Sucesso!", msg, "OK");
                Close();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UnitCreationWizard] Ocorreu um erro ao gerar a unidade: {ex.Message}");
                EditorUtility.DisplayDialog("Erro", $"Falha ao gerar os assets da unidade. Detalhes no console.", "OK");
            }
        }
    }
}
#endif
