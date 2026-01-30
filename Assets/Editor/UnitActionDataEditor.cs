using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

[CustomPropertyDrawer(typeof(UnitActionData), true)]
public class UnitActionDataDrawer : PropertyDrawer
{
    static Type[] actionTypes;
    static string[] actionTypeNames;
    static bool initialized;

    void Init()
    {
        if (initialized) return;

        actionTypes = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t =>
                !t.IsAbstract &&
                typeof(UnitActionData).IsAssignableFrom(t)
            )
            .ToArray();

        actionTypeNames = actionTypes
            .Select(t => t.Name.Replace("ActionData", ""))
            .ToArray();

        initialized = true;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Init();
        EditorGUI.BeginProperty(position, label, property);

        // 🔧 CORREÇÃO PRINCIPAL:
        // se ainda não existe instância, cria a padrão
        if (property.managedReferenceValue == null && actionTypes.Length > 0)
        {
            property.managedReferenceValue = Activator.CreateInstance(actionTypes[0]);
            property.serializedObject.ApplyModifiedProperties();
        }

        Rect header = new Rect(
            position.x,
            position.y,
            position.width,
            EditorGUIUtility.singleLineHeight
        );

        // Foldout
        property.isExpanded = EditorGUI.Foldout(
            new Rect(header.x, header.y, 18, header.height),
            property.isExpanded,
            GUIContent.none
        );

        int currentIndex = GetCurrentTypeIndex(property);

        // Dropdown
        int newIndex = EditorGUI.Popup(
            new Rect(header.x, header.y, header.width, header.height),
            currentIndex,
            actionTypeNames
        );


        // Change type
        if (newIndex != currentIndex && newIndex >= 0)
        {
            property.managedReferenceValue = Activator.CreateInstance(actionTypes[newIndex]);
            property.serializedObject.ApplyModifiedProperties();
        }

        // Draw fields
        if (property.isExpanded && property.managedReferenceValue != null)
        {
            EditorGUI.indentLevel++;

            SerializedProperty iterator = property.Copy();
            SerializedProperty end = iterator.GetEndProperty();

            float y = header.y + header.height + 2;
            iterator.NextVisible(true);

            while (iterator.NextVisible(false) &&
                   !SerializedProperty.EqualContents(iterator, end))
            {
                float h = EditorGUI.GetPropertyHeight(iterator, true);
                EditorGUI.PropertyField(
                    new Rect(position.x-3, y, position.width - 1, h),
                    iterator,
                    true
                );
                y += h + 2;
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;

        if (property.isExpanded && property.managedReferenceValue != null)
        {
            SerializedProperty iterator = property.Copy();
            SerializedProperty end = iterator.GetEndProperty();

            iterator.NextVisible(true);

            while (iterator.NextVisible(false) &&
                   !SerializedProperty.EqualContents(iterator, end))
            {
                height += EditorGUI.GetPropertyHeight(iterator, true) + 2;
            }
        }

        return height;
    }

    int GetCurrentTypeIndex(SerializedProperty property)
    {
        if (property.managedReferenceValue == null)
            return 0;

        Type t = property.managedReferenceValue.GetType();
        return Array.IndexOf(actionTypes, t);
    }
}
