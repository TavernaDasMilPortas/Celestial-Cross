using UnityEngine;
using Celestial_Cross.Scripts.Abilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Units/Pet Data")]
public class PetData : ScriptableObject
{
    [SerializeField, HideInInspector]
    private string petID;
    public string PetID => petID;

    public string displayName;
    public Sprite icon;
    public CombatStats baseStats = new CombatStats(5, 2, 0, 1, 10, 0);
    public AbilityBlueprint ability;

#if UNITY_EDITOR
    private void OnValidate()
    {
        string assetPath = AssetDatabase.GetAssetPath(this);
        if (string.IsNullOrWhiteSpace(assetPath))
            return;

        string guid = AssetDatabase.AssetPathToGUID(assetPath);
        if (string.IsNullOrWhiteSpace(guid) || petID == guid)
            return;

        petID = guid;
        EditorUtility.SetDirty(this);
    }
#endif
}
