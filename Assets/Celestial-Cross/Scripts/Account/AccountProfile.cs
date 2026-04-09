using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "AccountProfile", menuName = "Account/Profile")]
public class AccountProfile : ScriptableObject
{
    public int Money = 100;
    public int Energy = 50;

    public List<UnitData> OwnedUnits = new List<UnitData>();
    public List<PetData> OwnedPets = new List<PetData>();

#if UNITY_EDITOR
    [Button("Auto-Preencher com TODAS as Unidades/Pets do Projeto", ButtonSizes.Large), GUIColor(0.2f, 0.8f, 0.2f)]
    private void FetchAllAvailable()
    {
        OwnedUnits.Clear();
        string[] unitGuids = AssetDatabase.FindAssets("t:UnitData");
        foreach (string guid in unitGuids)
        {
            UnitData ud = AssetDatabase.LoadAssetAtPath<UnitData>(AssetDatabase.GUIDToAssetPath(guid));
            if (ud != null && !OwnedUnits.Contains(ud))
            {
                OwnedUnits.Add(ud);
            }
        }

        OwnedPets.Clear();
        string[] petGuids = AssetDatabase.FindAssets("t:PetData");
        foreach (string guid in petGuids)
        {
            PetData pd = AssetDatabase.LoadAssetAtPath<PetData>(AssetDatabase.GUIDToAssetPath(guid));
            if (pd != null && !OwnedPets.Contains(pd))
            {
                OwnedPets.Add(pd);
            }
        }

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        Debug.Log($"Perfil de teste '{name}' atualizado: {OwnedUnits.Count} Unidades e {OwnedPets.Count} Pets adicionados automaticamente.");
    }
#endif
}
