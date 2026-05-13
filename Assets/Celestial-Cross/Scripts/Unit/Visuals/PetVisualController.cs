using UnityEngine;

namespace CelestialCross.UnitVisuals
{
    [RequireComponent(typeof(Animator))]
    public class PetVisualController : MonoBehaviour
    {
        private Animator animator;
        private static readonly int IdleHash = Animator.StringToHash("Idle");
        private static readonly int SkillHash = Animator.StringToHash("Skill");

        [Header("Positioning Configurations")]
        [Tooltip("Posição em relação ao dono caso seja um Pet do tipo Ground (Chão)")]
        public Vector3 groundOffset = new Vector3(-0.7f, 0f, 0f);
        
        [Tooltip("Posição em relação ao dono caso seja um Pet do tipo Flying (Voador)")]
        public Vector3 flyOffset = new Vector3(-0.5f, 0.8f, 0f);

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public void PlayIdle()
        {
            if (animator != null)
                animator.SetTrigger(IdleHash);
        }

        public void PlaySkill()
        {
            if (animator != null)
                animator.SetTrigger(SkillHash);
        }

        /// <summary>
        /// Configura a posição inicial relativa ao dono baseado no seu tipo de movimentação.
        /// </summary>
        public void Setup(Transform owner, CelestialCross.Data.Pets.PetMovementType moveType, Vector3 scale)
        {
            transform.SetParent(owner, false);
            transform.localPosition = (moveType == CelestialCross.Data.Pets.PetMovementType.Flying) ? flyOffset : groundOffset;
            transform.localScale = scale;
            // A rotação foi mantida de acordo com o que foi definido no Prefab
        }
        
        public void FaceDirection(bool lookLeft)
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.flipX = lookLeft;
        }
    }
}
