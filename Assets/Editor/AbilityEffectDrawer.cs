using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using CelestialCross.Combat;

[CustomPropertyDrawer(typeof(AbilityEffectBase), true)]
public class AbilityEffectDrawer : PropertyDrawer
{
    private static List<Type> derivedTypes;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        if (derivedTypes == null)
            InitializeDerivedTypes();

        // 1. Dropdown para selecionar o tipo se estiver null ou quiser trocar
        string currentTypeName = property.managedReferenceFullTypename;
        int currentIndex = GetCurrentTypeIndex(currentTypeName);

        Rect typeSelectorRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        int newIndex = EditorGUI.Popup(typeSelectorRect, "Efeito", currentIndex, derivedTypes.Select(t => t.Name).ToArray());

        if (newIndex != currentIndex)
        {
            object newInstance = Activator.CreateInstance(derivedTypes[newIndex]);
            property.managedReferenceValue = newInstance;
        }

        // 2. Desenhar as propriedades do efeito
        Rect contentRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, position.height - EditorGUIUtility.singleLineHeight - 2);
        EditorGUI.PropertyField(contentRect, property, label, true);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true) + EditorGUIUtility.singleLineHeight + 4;
    }

    private void InitializeDerivedTypes()
    {
        derivedTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => typeof(AbilityEffectBase).IsAssignableFrom(p) && !p.IsAbstract)
            .ToList();
    }

    private int GetCurrentTypeIndex(string fullTypeName)
    {
        if (string.IsNullOrEmpty(fullTypeName)) return 0;
        
        string typeName = fullTypeName.Split(' ').Last();
        for (int i = 0; i < derivedTypes.Count; i++)
        {
            if (derivedTypes[i].FullName == typeName) return i;
        }
        return 0;
    }
}
