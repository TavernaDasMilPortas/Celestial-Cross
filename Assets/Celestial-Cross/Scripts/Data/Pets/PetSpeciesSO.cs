using System.Collections.Generic;
using UnityEngine;
using Celestial_Cross.Scripts.Abilities;

namespace CelestialCross.Data.Pets
{
    [CreateAssetMenu(fileName = "NewPetSpecies", menuName = "RPG/Pets/Pet Species Base")]
    public class PetSpeciesSO : ScriptableObject
    {
        [HideInInspector] // o id das especies não deve ser gerado no inspector
        public string id;

        public string SpeciesName;
        public Sprite Icon;

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
        
        [Header("Habilidades (Skills)")]
        [Tooltip("Habilidades ativas gerais deste pet.")]
        public List<AbilityBlueprint> ActiveSkills = new List<AbilityBlueprint>();

        [Tooltip("Habilidades passivas intrínsecas a esta espécie.")]
        public List<AbilityBlueprint> PassiveSkills = new List<AbilityBlueprint>();

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(id) || id != name)
            {
                id = name;
            }
        }
    }
}
