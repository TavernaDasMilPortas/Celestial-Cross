#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(AIBehaviorRule))]
public class AIBehaviorRuleDrawer : PropertyDrawer
{
    private bool IsPortuguese => EditorPrefs.GetBool("AILang_PT", true);

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded) return EditorGUIUtility.singleLineHeight;
        
        float baseHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        float totalHeight = baseHeight; // Foldout

        // Propriedades comuns
        totalHeight += GetPropHeight(property, "ruleName");
        totalHeight += GetPropHeight(property, "priority");
        totalHeight += GetPropHeight(property, "behavior");
        totalHeight += GetPropHeight(property, "targetPreference");
        
        var targetPref = property.FindPropertyRelative("targetPreference");
        bool showRoleClass = targetPref != null && (targetPref.enumValueIndex == (int)AITargetPreference.PrioritizeRole || targetPref.enumValueIndex == (int)AITargetPreference.PrioritizeClass);
        
        if (showRoleClass)
        {
            totalHeight += GetPropHeight(property, "preferredRole");
            totalHeight += GetPropHeight(property, "preferredClass");
        }
        
        // Headers e margins (Condições)
        totalHeight += 10f; // Espaçamento antes do header
        totalHeight += baseHeight; // O header label em si
        
        totalHeight += GetPropHeight(property, "activateWhenHpBelow");
        totalHeight += GetPropHeight(property, "activateWhenAlone");
        totalHeight += GetPropHeight(property, "activateWhenAlliesBelow");

        // Headers e margins (Pesos)
        totalHeight += 10f; // Espaçamento antes do header
        totalHeight += baseHeight; // O header label em si
        
        totalHeight += GetPropHeight(property, "attackWeight");
        totalHeight += GetPropHeight(property, "moveWeight");
        totalHeight += GetPropHeight(property, "abilityWeight");
        
        return totalHeight;
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
        
        var ruleNameProp = property.FindPropertyRelative("ruleName");
        string ruleName = ruleNameProp != null ? ruleNameProp.stringValue : "Rule";
        
        property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, new GUIContent(ruleName), true);
        rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            
            DrawProp(ref rect, property, "ruleName", "Nome da Regra", "Rule Name");
            DrawProp(ref rect, property, "priority", "Prioridade", "Priority");
            DrawProp(ref rect, property, "behavior", "Comportamento", "Behavior");
            DrawProp(ref rect, property, "targetPreference", "Preferência de Alvo", "Target Preference");
            
            var targetPref = property.FindPropertyRelative("targetPreference");
            if (targetPref != null && (targetPref.enumValueIndex == (int)AITargetPreference.PrioritizeRole || targetPref.enumValueIndex == (int)AITargetPreference.PrioritizeClass))
            {
                EditorGUI.indentLevel++;
                DrawProp(ref rect, property, "preferredRole", "Papel Preferido", "Preferred Role");
                DrawProp(ref rect, property, "preferredClass", "Classe Preferida", "Preferred Class");
                EditorGUI.indentLevel--;
            }
            
            rect.y += 10f;
            EditorGUI.LabelField(rect, IsPortuguese ? "Condições de Ativação" : "Activation Conditions", EditorStyles.boldLabel);
            rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            
            DrawProp(ref rect, property, "activateWhenHpBelow", "HP Abaixo de (%)", "HP Below (%)");
            DrawProp(ref rect, property, "activateWhenAlone", "Apenas se Sozinho", "Only If Alone");
            DrawProp(ref rect, property, "activateWhenAlliesBelow", "Aliados Vivos Máx.", "Max Alive Allies");

            rect.y += 10f;
            EditorGUI.LabelField(rect, IsPortuguese ? "Pesos de Escoragem" : "Scoring Weights", EditorStyles.boldLabel);
            rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            
            DrawProp(ref rect, property, "attackWeight", "Peso: Atacar", "Weight: Attack");
            DrawProp(ref rect, property, "moveWeight", "Peso: Mover", "Weight: Move");
            DrawProp(ref rect, property, "abilityWeight", "Peso: Habilidades", "Weight: Ability");

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    private void DrawProp(ref Rect rect, SerializedProperty parent, string propName, string labelPt, string labelEn)
    {
        var prop = parent.FindPropertyRelative(propName);
        if (prop != null)
        {
            // Pega a altura dinâmica da propriedade (útil caso a Unity desenhe um campo de múltiplas linhas)
            float propHeight = EditorGUI.GetPropertyHeight(prop, true);
            rect.height = propHeight;
            
            EditorGUI.PropertyField(rect, prop, new GUIContent(IsPortuguese ? labelPt : labelEn), true);
            rect.y += propHeight + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}
#endif