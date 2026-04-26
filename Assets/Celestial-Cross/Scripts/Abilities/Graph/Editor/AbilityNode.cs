using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor
{
    public abstract class AbilityNode : Node
    {
        public string GUID;
        public bool EntryPoint = false;

        // Chama isso na criação do node para configurar a UI específica dele
        public virtual void Initialize(string nodeGuid, Vector2 position)
        {
            GUID = nodeGuid;
            SetPosition(new Rect(position, new Vector2(200, 150)));
            
            // Estilo padrão
            mainContainer.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f, 0.9f));
        }

        // Deve retornar um JSON contendo os valores atuais do nó
        public abstract string GetJsonData();

        // Deve ler o JSON e restaurar os valores na UI
        public abstract void LoadFromJson(string json);

        public virtual string GetDescription() => "";

        // Para salvar referências a ScriptableObjects ou outros Assets do Unity
        public virtual void OnSave(AbilityNodeData data) { }
        public virtual void OnLoad(AbilityNodeData data) { }
    }
}
