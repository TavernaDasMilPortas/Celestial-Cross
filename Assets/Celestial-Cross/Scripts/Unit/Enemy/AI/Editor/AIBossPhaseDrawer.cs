#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(AIBossPhase))]
public class AIBossPhaseDrawer : PropertyDrawer
{
    private bool IsPortuguese => EditorPrefs.GetBool("AILang_PT", true);

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded) return EditorGUIUtility.singleLineHeight;
        
        float baseHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        float totalHeight = baseHeight; // Foldout header

        totalHeight += GetPropHeight(property, "phaseName");
        totalHeight += GetPropHeight(property, "triggerHpBelowPercent");
        totalHeight += GetPropHeight(property, "newBehaviorProfile");
        
        return totalHeight + 8f; // padding inferior
    }

    private float GetPropHeight(SerializedProperty parent, string propName)
    {
        var prop = parent.FindPropertyRelative(propName);
        if (prop != null)
        {
            return EditorGUI.GetPropertyHeight(prop, true) + EditorGUIUtility.standardVerticalSpacing;
        }
        return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        
        var phaseNameProp = property.FindPropertyRelative("phaseName");
        string pName = phaseNameProp != null && !string.IsNullOrEmpty(phaseNameProp.stringValue) 
            ? phaseNameProp.stringValue : (IsPortuguese ? "Nova Fase" : "New Phase");
        
        property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, new GUIContent(pName), true);
        rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            
            DrawProp(ref rect, property, "phaseName", "Nome descritivo", "Phase Name");
            DrawProp(ref rect, property, "triggerHpBelowPercent", "Ativar se HP < (%)", "Trigger if HP < (%)");
            DrawProp(ref rect, property, "newBehaviorProfile", "Novo Perfil de IA", "New Behavior Profile");

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    private void DrawProp(ref Rect rect, SerializedProperty parent, string propName, string labelPt, string labelEn)
    {
        var prop = parent.FindPropertyRelative(propName);
        if (prop != null)
        {
            float propHeight = EditorGUI.GetPropertyHeight(prop, true);
            rect.height = propHeight;
            
            EditorGUI.PropertyField(rect, prop, new GUIContent(IsPortuguese ? labelPt : labelEn), true);
            rect.y += propHeight + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}
#endif