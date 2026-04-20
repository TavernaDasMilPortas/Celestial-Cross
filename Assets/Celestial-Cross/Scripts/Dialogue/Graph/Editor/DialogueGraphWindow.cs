using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using System.Linq;

namespace CelestialCross.Dialogue.Graph.Editor
{
    /// <summary>
    /// Janela principal do Editor de Diálogo.
    /// </summary>
    public class DialogueGraphWindow : EditorWindow
    {
        private DialogueGraphView _graphView;
        private DialogueGraph _currentGraph;

        [MenuItem("Celestial Cross/Dialogue Graph Editor")]
        public static void OpenWindow()
        {
            var window = GetWindow<DialogueGraphWindow>();
            window.titleContent = new GUIContent("Dialogue Graph");
        }

        private void OnEnable()
        {
            ConstructGraphView();
            GenerateToolbar();
        }

        private void OnDisable()
        {
            if (_graphView != null && rootVisualElement.Contains(_graphView))
            {
                rootVisualElement.Remove(_graphView);
            }
        }

        private void ConstructGraphView()
        {
            _graphView = new DialogueGraphView
            {
                name = "Dialogue Graph"
            };

            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);

            AddBlackboard();
        }

        private void AddBlackboard()
        {
            var blackboard = new Blackboard(_graphView);
            blackboard.Add(new BlackboardSection { title = "Exposed Variables" });
            blackboard.addItemRequested = _view => { _graphView.AddPropertyToBlackboard(new ExposedProperty()); };
            blackboard.editTextRequested = (view1, element, newValue) =>
            {
                var oldPropertyName = ((BlackboardField)element).text;
                if (_graphView.ExposedProperties.Any(x => x.propertyName == newValue))
                {
                    EditorUtility.DisplayDialog("Error", "This property name already exists!", "OK");
                    return;
                }

                var propertyIndex = _graphView.ExposedProperties.FindIndex(x => x.propertyName == oldPropertyName);
                _graphView.ExposedProperties[propertyIndex].propertyName = newValue;
                ((BlackboardField)element).text = newValue;
            };
            blackboard.SetPosition(new Rect(10, 30, 200, 300));
            _graphView.Add(blackboard);
            _graphView.Blackboard = blackboard;
        }

        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();

            // Campo para selecionar um ScriptableObject existente
            var assetField = new ObjectField("Dialogue Graph:")
            {
                objectType = typeof(DialogueGraph),
                allowSceneObjects = false,
                value = _currentGraph
            };
            assetField.RegisterValueChangedCallback(evt =>
            {
                _currentGraph = evt.newValue as DialogueGraph;
                if (_currentGraph != null)
                {
                    RequestDataLoad();
                }
            });
            toolbar.Add(assetField);

            // Botão para criar um novo arquivo
            toolbar.Add(new Button(() => CreateNewGraph()) { text = "New Graph" });
            
            // Botão para salvar
            toolbar.Add(new Button(() => RequestDataSave()) { text = "Save" });

            rootVisualElement.Add(toolbar);
        }

        private void CreateNewGraph()
        {
            string path = EditorUtility.SaveFilePanelInProject("Create New Dialogue Graph", "NewDialogueGraph", "asset", "Select location to save the new graph");
            if (string.IsNullOrEmpty(path)) return;

            var newGraph = ScriptableObject.CreateInstance<DialogueGraph>();
            AssetDatabase.CreateAsset(newGraph, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _currentGraph = newGraph;
            
            // Força o campo de objeto a atualizar (re-gerando o toolbar ou encontrando o campo)
            var assetField = rootVisualElement.Q<ObjectField>();
            if (assetField != null) assetField.value = _currentGraph;

            // Limpa o canvas para o novo grafo
            _graphView.ClearGraph(); 
            // O GraphSaveUtility costuma ter o ClearGraph, mas aqui queremos apenas resetar a visualização.
            // Para simplificar, vamos carregar o recém criado (vazio)
            RequestDataLoad();
        }

        private void RequestDataSave()
        {
            if (_currentGraph == null)
            {
                EditorUtility.DisplayDialog("No Graph Selected", "Please select or create a Dialogue Graph asset first.", "OK");
                return;
            }

            var saveUtility = GraphSaveUtility.GetInstance(_graphView);
            saveUtility.SaveGraph(_currentGraph);
        }

        private void RequestDataLoad()
        {
            if (_currentGraph == null) return;

            var saveUtility = GraphSaveUtility.GetInstance(_graphView);
            saveUtility.LoadGraph(_currentGraph);
        }
    }
}
