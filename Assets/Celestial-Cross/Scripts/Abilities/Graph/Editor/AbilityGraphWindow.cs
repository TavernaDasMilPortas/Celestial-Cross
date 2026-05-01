using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor
{
    public class AbilityGraphWindow : EditorWindow
    {
        private AbilityGraphView _graphView;
        private AbilityGraphSO _currentGraphAsset;
        private ObjectField _assetField;

        [MenuItem("Celestial Cross/Ability Graph Editor")]
        public static void OpenWindow()
        {
            var window = GetWindow<AbilityGraphWindow>();
            window.titleContent = new GUIContent("Ability Graph");
            window.Show();
        }

        private void OnEnable()
        {
            ConstructGraphView();
            GenerateToolbar();
        }

        private void OnDisable()
        {
            if (_graphView != null)
            {
                rootVisualElement.Remove(_graphView);
            }
        }

        private void ConstructGraphView()
        {
            _graphView = new AbilityGraphView(this)
            {
                name = "Ability Graph"
            };

            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
        }

        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();

            // Campo para selecionar o arquivo do Grafo
            _assetField = new ObjectField("Graph Asset")
            {
                objectType = typeof(AbilityGraphSO),
                allowSceneObjects = false,
                value = _currentGraphAsset
            };
            _assetField.RegisterValueChangedCallback(evt => {
                _currentGraphAsset = evt.newValue as AbilityGraphSO;
                _graphView.SetGraphAsset(_currentGraphAsset);
            });
            toolbar.Add(_assetField);

            var saveButton = new Button(() => { SaveData(); }) { text = "Save Graph" };
            var loadButton = new Button(() => { LoadData(); }) { text = "Load Graph" };

            toolbar.Add(saveButton);
            toolbar.Add(loadButton);

            rootVisualElement.Add(toolbar);
        }

        private void SaveData()
        {
            if (_currentGraphAsset == null)
            {
                EditorUtility.DisplayDialog("Erro", "Por favor, selecione ou crie um Ability Graph Object antes de salvar.", "OK");
                return;
            }

            GraphSaveUtility.GetInstance(_graphView).SaveGraph(_currentGraphAsset);
        }

        private void LoadData()
        {
            if (_currentGraphAsset == null)
            {
                EditorUtility.DisplayDialog("Erro", "Por favor, selecione um Ability Graph Object para carregar.", "OK");
                return;
            }

            _graphView.SetGraphAsset(_currentGraphAsset);
            GraphSaveUtility.GetInstance(_graphView).LoadGraph(_currentGraphAsset);
        }

        // Método para abrir a janela já carregando um asset (útil para double-click)
        public static void OpenWithAsset(AbilityGraphSO asset)
        {
            var window = GetWindow<AbilityGraphWindow>();
            window.titleContent = new GUIContent("Ability Graph");
            window._currentGraphAsset = asset;
            window.LoadData();
            window.Show();
        }
    }
}
