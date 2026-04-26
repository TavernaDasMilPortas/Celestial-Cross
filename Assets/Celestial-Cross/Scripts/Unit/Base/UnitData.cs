using UnityEngine;
using System.Collections.Generic;
using Celestial_Cross.Scripts.Abilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum UnitRole
{
    Attacker,   // Foco puro em causar dano
    Tank,       // Sobrevivência, trava de movimentação e redirecionamento de ameaça
    Support     // Utilitários, manter equipe viva ou manipular combate
}

public enum UnitClass
{
    Warrior,    // Dano de curtas distâncias consistente, aguenta pancada
    Mage,       // Danos em áreas vastas, alto custo de mana e baixa sobrevivência
    Ranger,     // Ataques focados bem distantes
    Assassin,   // Foca em matar suportes/magos alheios numa explosão
    Healer,     // Mantém a sobrevivência direta resgatando HP
    Buffer,     // Concede vantagem tática como velocidade, escudos e +ataque
    Hexer,      // Especializado em jogar debuffs, veneno, redução de defesa/velocidade
    Summoner    // Pode colocar obstáculos reais ou pets menores em campo
}

[CreateAssetMenu(menuName = "Celestial Cross/Units/Unit Data")]
public class UnitData : ScriptableObject
{
    [SerializeField, HideInInspector]
    private string unitID;
    public string UnitID => unitID;
    public string displayName;

    [Header("Tactical Identity")]
    public UnitRole role;
    public UnitClass unitClass;

    [Header("UI & Visuals")]
    public Sprite icon;
    
    [Tooltip("Animação base usada quando a unidade está apenas esperando.")]
    public AnimationClip idleAnim;
    
    [Tooltip("Animação usada quando for o turno desta unidade no combate.")]
    public AnimationClip combatIdleAnim;

    [Header("Stats")]
    public CombatStats baseStats = new CombatStats(30, 10, 6, 7, 7, 1);

    [Header("Abilities (Blueprints)")]
    [Tooltip("Lista de habilidades e passivas usando o novo sistema de Blueprints.")]
    public List<AbilityBlueprint> abilities = new();

    [Header("Ability Graphs")]
    [Tooltip("Lista de habilidades usando o sistema de Grafos.")]
    public List<Celestial_Cross.Scripts.Abilities.Graph.AbilityGraphSO> abilityGraphs = new();

    [Header("Actions (Native)")]
    [SerializeReference]
    public List<UnitActionData> nativeActions = new();

    public List<AbilityBlueprint> GetAbilities() => abilities;
    public List<Celestial_Cross.Scripts.Abilities.Graph.AbilityGraphSO> GetAbilityGraphs() => abilityGraphs;

    // Adaptado para Unit.cs - UnitActionContext se comunica com UnitActionData
    public IEnumerable<UnitActionData> GetExecutableDefinitions()
    {
        foreach (var action in nativeActions)
        {
            if (action != null)
                yield return action;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        string assetPath = AssetDatabase.GetAssetPath(this);
        if (string.IsNullOrWhiteSpace(assetPath))
            return;

        string guid = AssetDatabase.AssetPathToGUID(assetPath);
        if (string.IsNullOrWhiteSpace(guid) || unitID == guid)
            return;

        unitID = guid;
        EditorUtility.SetDirty(this);
    }
#endif
}

