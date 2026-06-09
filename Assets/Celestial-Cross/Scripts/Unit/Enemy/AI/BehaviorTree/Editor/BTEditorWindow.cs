using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree;

namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Editor
{
    public class BTEditorWindow : EditorWindow
    {
        private BTGraphView _graphView;
        private BehaviorTreeSO _currentAsset;
        private ObjectField _assetField;

        [MenuItem("Celestial Cross/1. Editors/Behavior Tree Editor")]
        public static void OpenWindow()
        {
            var window = GetWindow<BTEditorWindow>();
            window.titleContent = new GUIContent("Behavior Tree");
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
                rootVisualElement.Remove(_graphView);
        }

        private void ConstructGraphView()
        {
            _graphView = new BTGraphView(this)
            {
                name = "Behavior Tree"
            };

            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
        }

        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();

            _assetField = new ObjectField("Tree Asset")
            {
                objectType = typeof(BehaviorTreeSO),
                allowSceneObjects = false,
                value = _currentAsset
            };
            _assetField.RegisterValueChangedCallback(evt => {
                _currentAsset = evt.newValue as BehaviorTreeSO;
                LoadData();
            });
            toolbar.Add(_assetField);

            var saveButton = new Button(() => { SaveData(); }) { text = BTLocalizationManager.GetString("Save Tree") };
            var loadButton = new Button(() => { LoadData(); }) { text = BTLocalizationManager.GetString("Load Tree") };

            var langButton = new Button(() => { BTLocalizationManager.ToggleLanguage(); }) { text = BTLocalizationManager.GetString("Language: EN") };

            BTLocalizationManager.OnLanguageChanged += () => {
                saveButton.text = BTLocalizationManager.GetString("Save Tree");
                loadButton.text = BTLocalizationManager.GetString("Load Tree");
                langButton.text = BTLocalizationManager.GetString("Language: EN");
                LoadData(); // reload graph to translate node titles
            };

            toolbar.Add(saveButton);
            toolbar.Add(loadButton);
            toolbar.Add(langButton);

            rootVisualElement.Add(toolbar);
        }

        private void SaveData()
        {
            if (_currentAsset == null)
            {
                EditorUtility.DisplayDialog("Erro", "Por favor, selecione ou crie um Behavior Tree Object antes de salvar.", "OK");
                return;
            }

            BTSaveUtility.GetInstance(_graphView).SaveGraph(_currentAsset);
        }

        private void LoadData()
        {
            if (_currentAsset == null) return;

            BTSaveUtility.GetInstance(_graphView).LoadGraph(_currentAsset);
        }

        public static void OpenWithAsset(BehaviorTreeSO asset)
        {
            var window = GetWindow<BTEditorWindow>();
            window.titleContent = new GUIContent("Behavior Tree");
            window._currentAsset = asset;
            if (window._assetField != null) window._assetField.value = asset;
            window.LoadData();
            window.Show();
        }
    }
}
