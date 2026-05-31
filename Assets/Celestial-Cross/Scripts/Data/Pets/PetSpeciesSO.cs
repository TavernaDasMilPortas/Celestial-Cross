using System.Collections.Generic;
using UnityEngine;
using Celestial_Cross.Scripts.Abilities;

namespace CelestialCross.Data.Pets
{
    public enum PetMovementType
    {
        Ground,
        Flying
    }

    [CreateAssetMenu(fileName = "NewPetSpecies", menuName = "Celestial Cross/Pets/Pet Species Base")]
    public class PetSpeciesSO : ScriptableObject
    {
        [HideInInspector] // o id das especies não deve ser gerado no inspector
        public string id;

        public string SpeciesName;
        public Sprite Icon;
        public Sprite sprite;

        [Header("Visuals (Combat)")]
        [Tooltip("Define o tipo de locomoção. O Prefab Base lerá isso para se posicionar automaticamente no chão ou voando.")]
        public PetMovementType MovementType = PetMovementType.Ground;

        [Tooltip("Animação principal de ficar parado (Idle)")]
        public AnimationClip IdleAnimation;
        
        [Tooltip("Animação quando o pet utiliza a habilidade dele")]
        public AnimationClip SkillAnimation;

        [Tooltip("Escala visual do pet no combate.")]
        public Vector3 CombatScale = Vector3.one;

        [Header("Status Ranges (Limites por Level e Estrela)")]
        [Tooltip("Vida Base Mínima num roll ruim")]
        public float MinBaseHealth;
        [Tooltip("Vida Base Máxima num roll perfeito")]
        public float MaxBaseHealth;

        [Tooltip("Ataque Base Mínimo")]
        public float MinBaseAttack;
        [Tooltip("Ataque Base Máximo")]
        public float MaxBaseAttack;

        [Tooltip("Defesa Base Mínima")]
        public float MinBaseDefense;
        [Tooltip("Defesa Base Máxima")]
        public float MaxBaseDefense;

        [Tooltip("Velocidade Base Mínima")]
        public float MinBaseSpeed;
        [Tooltip("Velocidade Base Máxima")]
        public float MaxBaseSpeed;

        [Tooltip("Chance de Crítico Base Mínima")]
        public float MinBaseCriticalChance;
        [Tooltip("Chance de Crítico Base Máxima")]
        public float MaxBaseCriticalChance;

        [Tooltip("Precisão de Efeito Base Mínima")]
        public float MinBaseEffectAccuracy;
        [Tooltip("Precisão de Efeito Base Máxima")]
        public float MaxBaseEffectAccuracy;

        [Tooltip("Dano Crítico Base Mínimo")]
        public float MinBaseCriticalDamage;
        [Tooltip("Dano Crítico Base Máximo")]
        public float MaxBaseCriticalDamage;

        [Tooltip("Resistência a Efeito Base Mínima")]
        public float MinBaseEffectResistance;
        [Tooltip("Resistência a Efeito Base Máxima")]
        public float MaxBaseEffectResistance;
        
        [Header("Habilidades (Skills)")]
        [Tooltip("Habilidades ativas gerais deste pet.")]
        public List<AbilityBlueprint> ActiveSkills = new List<AbilityBlueprint>();

        [Tooltip("Habilidades passivas intrínsecas a esta espécie.")]
        public List<AbilityBlueprint> PassiveSkills = new List<AbilityBlueprint>();

        [Tooltip("Habilidades via Grafo.")]
        public List<Celestial_Cross.Scripts.Abilities.Graph.AbilityGraphSO> AbilityGraphs = new List<Celestial_Cross.Scripts.Abilities.Graph.AbilityGraphSO>();

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(id) || id != name)
            {
                id = name;
            }
        }
    }
}
